namespace FullStackAPI_Guild.Api.DTOs;

public class CreateClaCodeResponse
{
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}