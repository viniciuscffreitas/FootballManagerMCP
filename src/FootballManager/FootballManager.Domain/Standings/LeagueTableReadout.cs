namespace FootballManager.Domain.Standings;

/// <summary>What the game says about the user's league: its name, the user's team, the press's prediction and the table.</summary>
public class LeagueTableReadout
{
    public string? League { get; set; }
    public int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>Where the press expects the user's team to finish; null when the game gives no prediction.</summary>
    public int? MediaExpectedPosition { get; set; }

    /// <summary>Whether the league is in a stage played as a table; false before its season starts.</summary>
    public bool HasLeagueStage { get; set; }

    /// <summary>The table's rows in the game's order; empty when there's no league stage.</summary>
    public List<LeagueTableRow> Rows { get; set; } = [];
}
