namespace FootballManager.Domain.Availability;

/// <summary>What the game says about who can play: every first-team player's injury, the team's bans and the next match's rules.</summary>
public class AvailabilityReadout
{
    /// <summary>One per first-team player, injured or not.</summary>
    public List<PlayerInjury> Injuries { get; set; } = [];

    public List<PlayerBan> Bans { get; set; } = [];
    public required NextMatchRules NextMatch { get; set; }
}
