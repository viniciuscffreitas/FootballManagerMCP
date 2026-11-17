namespace FootballManager.Api.Responses.Opposition;

public class OppositionReportResponse
{
    public required int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>True when the team is the user's next opponent, the only one the game assesses the threat of.</summary>
    public bool IsNextOpponent { get; set; }

    public string? ThreatLevel { get; set; }
    public string? Formation { get; set; }
    public string? TacticSummary { get; set; }
    public List<ExpectedPlayerResponse> ExpectedLineup { get; set; } = [];
    public List<ReportPointResponse> Points { get; set; } = [];
}
