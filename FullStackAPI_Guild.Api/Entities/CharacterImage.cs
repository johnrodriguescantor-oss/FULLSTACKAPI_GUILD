namespace FullStackAPI_Guild.Api.Entities;

public class CharacterImage
{
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }
    public Character? Character { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
