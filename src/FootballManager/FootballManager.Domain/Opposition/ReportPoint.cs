namespace FootballManager.Domain.Opposition;

/// <summary>One point of the analysis: what it looks at (e.g. "Criatividade - Meio-campistas") and the game's verdict.</summary>
public class ReportPoint
{
    public required string Topic { get; set; }
    public string? Summary { get; set; }

    /// <summary>The player the point is about, when it's one of the team's players.</summary>
    public string? PlayerName { get; set; }

    /// <summary>The span the point covers (e.g. "Esta Temporada").</summary>
    public string? TimeFrame { get; set; }
}
