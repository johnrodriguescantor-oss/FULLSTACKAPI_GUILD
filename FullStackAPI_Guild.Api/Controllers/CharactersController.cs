using System.Security.Claims;
using FullStackAPI_Guild.Api.DTOs;
using FullStackAPI_Guild.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullStackAPI_Guild.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CharactersController : ControllerBase
{
    private readonly CharacterService _characterService;

    public CharactersController(CharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> Create(CreateCharacterRequest request)
    {
        var userId = GetAuthenticatedUserId();
        var response = await _characterService.CreateAsync(userId, request);
        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<ActionResult<List<CharacterResponse>>> GetMine([FromQuery] string? status)
    {
        var userId = GetAuthenticatedUserId();
        var activeOnly = !string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase);
        var response = await _characterService.GetMineAsync(userId, activeOnly);
        return Ok(response);
    }

    private Guid GetAuthenticatedUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Token invalido.");
        return userId;
    }
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CharacterResponse>> Update(Guid id, UpdateCharacterRequest request)
    {
        var userId = GetAuthenticatedUserId();
        var response = await _characterService.UpdateAsync(userId, id, request);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<CharacterResponse>> Deactivate(Guid id)
    {
        var userId = GetAuthenticatedUserId();
        var response = await _characterService.DeactivateAsync(userId, id);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<ActionResult<CharacterResponse>> Reactivate(Guid id)
    {
        var userId = GetAuthenticatedUserId();
        var response = await _characterService.ReactivateAsync(userId, id);
        return Ok(response);
    }

    [HttpGet("guild-board")]
    public async Task<ActionResult<List<GuildCharacterBoardItemResponse>>> GetGuildBoard()
    {
        var response = await _characterService.GetGuildBoardAsync();
        return Ok(response);
    }
}
