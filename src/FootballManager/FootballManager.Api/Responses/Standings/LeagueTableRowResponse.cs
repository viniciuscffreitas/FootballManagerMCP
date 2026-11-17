using FootballManager.Domain.Standings;

namespace FootballManager.Api.Responses.Standings;

public class LeagueTableRowResponse
{
    public int Position { get; set; }
    public int TeamId { get; set; }
    public required string TeamName { get; set; }
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

    /// <summary>What the place leads to at the end of the season, as the game names it; null when nothing.</summary>
    public string? Fate { get; set; }

    /// <summary>True on the user's own team; left out on the others.</summary>
    public bool? IsUser { get; set; }
}
