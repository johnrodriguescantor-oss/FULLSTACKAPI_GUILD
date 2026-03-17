namespace FullStackAPI_Guild.Api.DTOs;

public class UpdateUserRoleResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string OldRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
