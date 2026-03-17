namespace FullStackAPI_Guild.Api.DTOs;

public class DeactivateUserResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Reason { get; set; } = string.Empty;
}
