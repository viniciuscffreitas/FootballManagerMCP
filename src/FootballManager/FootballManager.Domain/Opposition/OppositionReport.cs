namespace FootballManager.Domain.Opposition;

/// <summary>
/// The game's report on an opponent, as its pre-match analysis shows it: the tactic and lineup it expects the team to
/// use, the analysis' points, and for the user's next opponent the game's assessment of the threat it poses.
/// </summary>
public class OppositionReport
{
    public required int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>True when the team is the user's next opponent, the only one the game assesses the threat of.</summary>
    public bool IsNextOpponent { get; set; }

    /// <summary>The game's one-line assessment of the next match, e.g. "Deve ser um jogo equilibrado...".</summary>
    public string? ThreatLevel { get; set; }

    /// <summary>The formation the game expects, as it names it (e.g. "4-2-3-1 VOL MEA Com Pontas").</summary>
    public string? Formation { get; set; }

    /// <summary>The game's summary of how the team plays (e.g. home and away alike).</summary>
    public string? TacticSummary { get; set; }

    /// <summary>The eleven the game expects, in its order, each with position and in-possession role.</summary>
    public List<ExpectedPlayer> ExpectedLineup { get; set; } = [];

    /// <summary>The analysis' points: one per chart of the report, with the game's verdict on it.</summary>
    public List<ReportPoint> Points { get; set; } = [];
}
