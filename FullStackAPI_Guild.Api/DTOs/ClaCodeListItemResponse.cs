namespace FullStackAPI_Guild.Api.DTOs;

public class ClaCodeListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime? UsedAt { get; set; }
    public string? UsedByUsername { get; set; }
    public string Status { get; set; } = string.Empty;
}
