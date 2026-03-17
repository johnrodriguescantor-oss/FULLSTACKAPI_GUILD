using FullStackAPI_Guild.Api.Enums;

namespace FullStackAPI_Guild.Api.Entities;

public class Character
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public CharacterClass CharacterClass { get; set; }
    public CharacterRoleTag RoleTag { get; set; }
    public int PrioritySlot { get; set; }
    public int LastKnownLevel { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? CurrentImageId { get; set; }
    public CharacterImage? CurrentImage { get; set; }

    public ICollection<CharacterImage> Images { get; set; } = new List<CharacterImage>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
