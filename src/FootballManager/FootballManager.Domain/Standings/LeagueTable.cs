namespace FootballManager.Domain.Standings;

/// <summary>The user's league table: every team's line, the user's place and where the press expects it to finish.</summary>
public class LeagueTable
{
    public string? League { get; set; }
    public required string TeamName { get; set; }

    /// <summary>The user's place in the table; null when there's no table yet.</summary>
    public int? Position { get; set; }

    public int? MediaExpectedPosition { get; set; }

    /// <summary>One per team, top of the table first.</summary>
    public List<LeagueTableRow> Rows { get; set; } = [];

    /// <summary>Why the table is empty, e.g. the league's season hasn't started; null when it has rows.</summary>
    public string? Reason { get; set; }
}
