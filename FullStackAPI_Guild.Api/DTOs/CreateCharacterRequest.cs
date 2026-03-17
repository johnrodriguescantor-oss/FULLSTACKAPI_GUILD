namespace FullStackAPI_Guild.Api.DTOs;

public class CreateCharacterRequest
{
    public string Name { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public string RoleTag { get; set; } = string.Empty;
    public int PrioritySlot { get; set; }
    public int LastKnownLevel { get; set; }
}
