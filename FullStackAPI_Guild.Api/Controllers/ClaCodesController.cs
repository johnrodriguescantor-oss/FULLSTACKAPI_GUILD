using System.Security.Claims;
using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.DTOs;
using FullStackAPI_Guild.Api.Enums;
using FullStackAPI_Guild.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FullStackAPI_Guild.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ClaCodesController : ControllerBase
{
    private readonly ClaCodeService _claCodeService;
    private readonly AppDbContext _context;

    public ClaCodesController(ClaCodeService claCodeService, AppDbContext context)
    {
        _claCodeService = claCodeService;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<CreateClaCodeResponse>> Create()
    {
        var userId = GetAuthenticatedUserId();

        var claCode = await _claCodeService.GenerateAsync(userId);

        var response = new CreateClaCodeResponse
        {
            Code = claCode.Code,
            ExpiresAt = claCode.ExpiresAt
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<ClaCodeListItemResponse>>> GetAll()
    {
        var userId = GetAuthenticatedUserId();

        var actingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (actingUser is null)
        {
            throw new KeyNotFoundException("Usuario autenticado nao encontrado.");
        }

        if (actingUser.Role is not UserRole.AssistantMaster and not UserRole.GuildMaster and not UserRole.DEV)
        {
            throw new UnauthorizedAccessException("Usuario sem permissao para listar ClaCodes.");
        }

        var now = DateTime.UtcNow;

        var claCodes = await _context.ClaCodes
            .Include(x => x.CreatedByUser)
            .Include(x => x.UsedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ClaCodeListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                CreatedAt = x.CreatedAt,
                ExpiresAt = x.ExpiresAt,
                CreatedByUsername = x.CreatedByUser != null ? x.CreatedByUser.Username : string.Empty,
                UsedAt = x.UsedAt,
                UsedByUsername = x.UsedByUser != null ? x.UsedByUser.Username : null,
                Status = x.UsedAt.HasValue
                    ? "Used"
                    : x.ExpiresAt < now
                        ? "Expired"
                        : "Active"
            })
            .ToListAsync();

        return Ok(claCodes);
    }

    private Guid GetAuthenticatedUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            throw new UnauthorizedAccessException("Usuario nao autenticado.");
        }

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Token invalido.");
        }

        return userId;
    }
}
