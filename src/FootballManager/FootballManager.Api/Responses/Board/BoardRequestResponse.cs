namespace FootballManager.Api.Responses.Board;

public class BoardRequestResponse
{
    public bool Applied { get; set; }
    public required string Action { get; set; }
    public List<string> Offered { get; set; } = [];
    public List<BoardMenuEntryResponse> Menu { get; set; } = [];
    public string? Choice { get; set; }
    public string? Confirmation { get; set; }
    public BoardCardResponse? Card { get; set; }
    public List<string> Notes { get; set; } = [];
}
