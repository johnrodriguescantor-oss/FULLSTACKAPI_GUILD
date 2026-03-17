namespace FullStackAPI_Guild.Api.DTOs;

public class CharacterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public string RoleTag { get; set; } = string.Empty;
    public int PrioritySlot { get; set; }
    public int LastKnownLevel { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
