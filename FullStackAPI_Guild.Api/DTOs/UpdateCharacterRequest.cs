namespace FullStackAPI_Guild.Api.DTOs;

public class UpdateCharacterRequest
{
    public string RoleTag { get; set; } = string.Empty;
    public int PrioritySlot { get; set; }
    public int LastKnownLevel { get; set; }
    public string? ImageUrl { get; set; }
}
