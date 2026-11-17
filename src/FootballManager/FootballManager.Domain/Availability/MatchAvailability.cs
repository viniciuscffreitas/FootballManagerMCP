namespace FootballManager.Domain.Availability;

/// <summary>Who can play the team's next match: the injured, the suspended, the match's rules, and every player's fitness.</summary>
public class MatchAvailability
{
    public required string TeamName { get; set; }
    public required NextMatchRules NextMatch { get; set; }

    /// <summary>Players an injury keeps out.</summary>
    public List<PlayerInjury> Injured { get; set; } = [];

    /// <summary>Every ban the team's players are serving, whatever the competition.</summary>
    public List<PlayerBan> Suspended { get; set; } = [];

    public List<PlayerAvailability> Players { get; set; } = [];
}
