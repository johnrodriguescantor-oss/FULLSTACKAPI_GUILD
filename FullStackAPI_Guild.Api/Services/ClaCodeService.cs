using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.Entities;
using FullStackAPI_Guild.Api.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FullStackAPI_Guild.Api.Services;

public class ClaCodeService
{
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly AppDbContext _context;

    public ClaCodeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClaCode> GenerateAsync(Guid createdByUserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == createdByUserId);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario nao encontrado.");
        }

        if (user.Role is not UserRole.AssistantMaster and not UserRole.GuildMaster and not UserRole.DEV)
        {
            throw new InvalidOperationException("Usuario sem permissao para gerar ClaCode.");
        }

        var code = await GenerateUniqueCodeAsync();

        var claCode = new ClaCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _context.ClaCodes.Add(claCode);
        await _context.SaveChangesAsync();

        return claCode;
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        while (true)
        {
            var code = GenerateCode();

            var exists = await _context.ClaCodes.AnyAsync(x => x.Code == code);

            if (!exists)
            {
                return code;
            }
        }
    }

    private static string GenerateCode()
    {
        Span<char> buffer = stackalloc char[6];
        Span<byte> data = stackalloc byte[6];

        RandomNumberGenerator.Fill(data);

        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = AllowedChars[data[i] % AllowedChars.Length];
        }

        return new string(buffer);
    }
}
