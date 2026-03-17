namespace FullStackAPI_Guild.Api.DTOs;

public class UpdateUserRoleRequest
{
    public string NewRole { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
