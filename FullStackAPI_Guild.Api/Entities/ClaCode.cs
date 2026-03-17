namespace FullStackAPI_Guild.Api.Entities;

public class ClaCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public Guid? UsedByUserId { get; set; }
    public User? UsedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
