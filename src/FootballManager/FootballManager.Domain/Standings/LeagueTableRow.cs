namespace FootballManager.Domain.Standings;

/// <summary>One team's line of the league table, as the game's competition screen shows it.</summary>
public class LeagueTableRow
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }
    public int Position { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
    public int Points { get; set; }

    /// <summary>Whether the team climbed or dropped places with its last result.</summary>
    public TableMovement Movement { get; set; }

    /// <summary>What the place leads to when the season ends, as the game names it (e.g. a promotion); null when nothing.</summary>
    public string? Fate { get; set; }

    /// <summary>The user's own team.</summary>
    public bool IsUser { get; set; }
}
