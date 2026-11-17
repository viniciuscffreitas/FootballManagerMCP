namespace FootballManager.Api.Responses.Board;

public class BoardConfidenceResponse
{
    public int? Grade { get; set; }
    public string? Summary { get; set; }
    public string? Matches { get; set; }
    public string? SquadSupport { get; set; }
    public string? Tactics { get; set; }
    public string? Vision { get; set; }
    public string? Transfers { get; set; }
    public List<string> Highlights { get; set; } = [];
    public List<string> Criticism { get; set; } = [];
}
