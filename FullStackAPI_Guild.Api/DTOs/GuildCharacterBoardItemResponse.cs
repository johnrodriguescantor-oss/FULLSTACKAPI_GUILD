namespace FullStackAPI_Guild.Api.DTOs;

public class GuildCharacterBoardItemResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<CharacterResponse> Characters { get; set; } = new();
}
