namespace FootballManager.Api.Responses.Board;

public class VisionObjectiveResponse
{
    public required string Name { get; set; }

    /// <summary>When it's due, e.g. "Temporada atual" or "2030".</summary>
    public string? Year { get; set; }

    public int? Progress { get; set; }
    public int? Status { get; set; }
}
