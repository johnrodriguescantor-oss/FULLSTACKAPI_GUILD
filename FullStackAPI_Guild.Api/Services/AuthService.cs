using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.DTOs;
using FullStackAPI_Guild.Api.Entities;
using FullStackAPI_Guild.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FullStackAPI_Guild.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var usernameExists = await _context.Users.AnyAsync(x => x.Username == request.Username);
        if (usernameExists)
        {
            throw new InvalidOperationException("Username ja esta em uso.");
        }

        var emailExists = await _context.Users.AnyAsync(x => x.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Email ja esta em uso.");
        }

        var claCode = await _context.ClaCodes
            .FirstOrDefaultAsync(x => x.Code == request.ClaCode);

        if (claCode is null)
        {
            throw new InvalidOperationException("ClaCode invalido.");
        }

        if (claCode.UsedAt.HasValue)
        {
            throw new InvalidOperationException("ClaCode ja foi utilizado.");
        }

        if (claCode.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("ClaCode expirado.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.Member,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        claCode.UsedByUserId = user.Id;
        claCode.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString()
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario ou senha invalidos.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new InvalidOperationException("Usuario ou senha invalidos.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Usuario inativo.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddHours(2);
        var token = GenerateJwtToken(user, expiresAt);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(User user, DateTime expiresAt)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key nao configurado.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer nao configurado.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience nao configurado.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
