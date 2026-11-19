namespace FootballManager.Api.Responses.Board;

public class BoardAnswerResponse
{
    public bool Applied { get; set; }
    public required BoardCardResponse Card { get; set; }
    public string? Line { get; set; }
    public BoardCardResponse? After { get; set; }
    public List<string> Notes { get; set; } = [];
}
