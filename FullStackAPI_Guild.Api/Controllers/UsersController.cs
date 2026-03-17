using System.Security.Claims;
using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.DTOs;
using FullStackAPI_Guild.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FullStackAPI_Guild.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserRoleService _userRoleService;

    public UsersController(AppDbContext context, UserRoleService userRoleService)
    {
        _context = context;
        _userRoleService = userRoleService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> Me()
    {
        var userId = GetAuthenticatedUserId();

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            throw new KeyNotFoundException("Usuario nao encontrado.");
        }

        var response = new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<UserListItemResponse>>> GetAll([FromQuery] string? status)
    {
        var actingUserId = GetAuthenticatedUserId();

        var users = await _userRoleService.GetUsersAsync(actingUserId, status);
        return Ok(users);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<UpdateUserRoleResponse>> UpdateRole(Guid id, UpdateUserRoleRequest request)
    {
        var actingUserId = GetAuthenticatedUserId();

        var response = await _userRoleService.UpdateRoleAsync(actingUserId, id, request);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<DeactivateUserResponse>> Deactivate(Guid id, DeactivateUserRequest request)
    {
        var actingUserId = GetAuthenticatedUserId();

        var response = await _userRoleService.DeactivateUserAsync(actingUserId, id, request);
        return Ok(response);
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