using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.DTOs;
using FullStackAPI_Guild.Api.Entities;
using FullStackAPI_Guild.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace FullStackAPI_Guild.Api.Services;

public class UserRoleService
{
    private readonly AppDbContext _context;

    public UserRoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserRoleResponse> UpdateRoleAsync(Guid actingUserId, Guid targetUserId, UpdateUserRoleRequest request)
    {
        var actingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == actingUserId);
        if (actingUser is null)
        {
            throw new InvalidOperationException("Usuario autenticado nao encontrado.");
        }

        if (!actingUser.IsActive)
        {
            throw new InvalidOperationException("Usuario autenticado esta inativo.");
        }

        if (actingUser.Role is not UserRole.GuildMaster and not UserRole.DEV)
        {
            throw new InvalidOperationException("Usuario sem permissao para alterar cargos.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == targetUserId);
        if (targetUser is null)
        {
            throw new InvalidOperationException("Usuario alvo nao encontrado.");
        }

        if (!targetUser.IsActive)
        {
            throw new InvalidOperationException("Nao e permitido alterar o cargo de um usuario inativo.");
        }

        if (!Enum.TryParse<UserRole>(request.NewRole, true, out var newRole))
        {
            throw new InvalidOperationException("Cargo informado e invalido.");
        }

        if (actingUser.Id == targetUser.Id && actingUser.Role == UserRole.DEV && newRole != UserRole.DEV)
        {
            throw new InvalidOperationException("DEV nao pode se auto-rebaixar.");
        }

        if (actingUser.Role == UserRole.GuildMaster)
        {
            if (targetUser.Role == UserRole.GuildMaster)
            {
                throw new InvalidOperationException("GuildMaster nao pode alterar outro GuildMaster.");
            }

            if (targetUser.Role == UserRole.DEV)
            {
                throw new InvalidOperationException("GuildMaster nao pode alterar DEV.");
            }

            if (newRole == UserRole.GuildMaster || newRole == UserRole.DEV)
            {
                throw new InvalidOperationException("GuildMaster nao pode promover para GuildMaster ou DEV.");
            }
        }

        if (actingUser.Role == UserRole.DEV)
        {
            if (newRole == UserRole.DEV && targetUser.Role != UserRole.DEV)
            {
                var activeDevCount = await _context.Users.CountAsync(x => x.Role == UserRole.DEV && x.IsActive);
                if (activeDevCount >= 2)
                {
                    throw new InvalidOperationException("O sistema permite no maximo 2 usuarios DEV ativos.");
                }
            }

            if (targetUser.Role == UserRole.DEV && newRole != UserRole.DEV)
            {
                var activeDevCount = await _context.Users.CountAsync(x => x.Role == UserRole.DEV && x.IsActive);
                if (activeDevCount <= 1)
                {
                    throw new InvalidOperationException("O sistema deve manter pelo menos 1 DEV ativo.");
                }
            }
        }

        var oldRole = targetUser.Role;
        targetUser.Role = newRole;

        var history = new RoleChangeHistory
        {
            Id = Guid.NewGuid(),
            TargetUserId = targetUser.Id,
            OldRole = oldRole,
            NewRole = newRole,
            ChangedByUserId = actingUser.Id,
            Reason = request.Reason?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoleChangeHistories.Add(history);
        await _context.SaveChangesAsync();

        return new UpdateUserRoleResponse
        {
            UserId = targetUser.Id,
            Username = targetUser.Username,
            OldRole = oldRole.ToString(),
            NewRole = newRole.ToString(),
            Reason = history.Reason
        };
    }

    public async Task<DeactivateUserResponse> DeactivateUserAsync(Guid actingUserId, Guid targetUserId, DeactivateUserRequest request)
    {
        var actingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == actingUserId);
        if (actingUser is null)
        {
            throw new InvalidOperationException("Usuario autenticado nao encontrado.");
        }

        if (!actingUser.IsActive)
        {
            throw new InvalidOperationException("Usuario autenticado esta inativo.");
        }

        if (actingUser.Role is not UserRole.GuildMaster and not UserRole.DEV)
        {
            throw new InvalidOperationException("Usuario sem permissao para inativar membros.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == targetUserId);
        if (targetUser is null)
        {
            throw new InvalidOperationException("Usuario alvo nao encontrado.");
        }

        if (!targetUser.IsActive)
        {
            throw new InvalidOperationException("Usuario alvo ja esta inativo.");
        }

        if (actingUser.Role == UserRole.GuildMaster)
        {
            if (targetUser.Role is not UserRole.AssistantMaster and not UserRole.BattleMaster and not UserRole.Member)
            {
                throw new InvalidOperationException("GuildMaster so pode inativar usuarios de AssistantMaster para baixo.");
            }
        }

        if (actingUser.Role == UserRole.DEV && targetUser.Role == UserRole.DEV)
        {
            var activeDevCount = await _context.Users.CountAsync(x => x.Role == UserRole.DEV && x.IsActive);

            if (activeDevCount <= 1)
            {
                throw new InvalidOperationException("O sistema deve manter pelo menos 1 DEV ativo.");
            }
        }

        targetUser.IsActive = false;

        await _context.SaveChangesAsync();

        return new DeactivateUserResponse
        {
            UserId = targetUser.Id,
            Username = targetUser.Username,
            Role = targetUser.Role.ToString(),
            IsActive = targetUser.IsActive,
            Reason = request.Reason?.Trim() ?? string.Empty
        };
    }
    public async Task<List<UserListItemResponse>> GetUsersAsync(Guid actingUserId, string? status)
    {
        var actingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == actingUserId);
        if (actingUser is null)
        {
            throw new InvalidOperationException("Usuario autenticado nao encontrado.");
        }

        if (!actingUser.IsActive)
        {
            throw new InvalidOperationException("Usuario autenticado esta inativo.");
        }

        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? "active"
            : status.Trim().ToLowerInvariant();

        if (actingUser.Role == UserRole.AssistantMaster)
        {
            if (normalizedStatus != "active")
            {
                throw new InvalidOperationException("AssistantMaster pode consultar apenas usuarios ativos.");
            }
        }
        else if (actingUser.Role is not UserRole.GuildMaster and not UserRole.DEV)
        {
            throw new InvalidOperationException("Usuario sem permissao para listar usuarios.");
        }

        var query = _context.Users.AsQueryable();

        query = normalizedStatus switch
        {
            "active" => query.Where(x => x.IsActive),
            "inactive" when actingUser.Role is UserRole.GuildMaster or UserRole.DEV => query.Where(x => !x.IsActive),
            "all" when actingUser.Role is UserRole.GuildMaster or UserRole.DEV => query,
            _ => throw new InvalidOperationException("Filtro de status invalido. Use active, inactive ou all.")
        };

        return await query
            .OrderBy(x => x.DisplayName)
            .Select(x => new UserListItemResponse
            {
                Id = x.Id,
                Username = x.Username,
                DisplayName = x.DisplayName,
                Email = x.Email,
                Role = x.Role.ToString(),
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                LastLoginAt = x.LastLoginAt
            })
            .ToListAsync();
    }

}
