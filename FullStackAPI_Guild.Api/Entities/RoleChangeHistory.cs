using FullStackAPI_Guild.Api.Enums;

namespace FullStackAPI_Guild.Api.Entities;

public class RoleChangeHistory
{
    public Guid Id { get; set; }
    public Guid TargetUserId { get; set; }
    public User? TargetUser { get; set; }
    public UserRole OldRole { get; set; }
    public UserRole NewRole { get; set; }
    public Guid ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}