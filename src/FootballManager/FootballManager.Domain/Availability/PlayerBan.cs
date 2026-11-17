namespace FootballManager.Domain.Availability;

/// <summary>A suspension a player of the team is serving, in any competition.</summary>
public class PlayerBan
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>Where the ban applies, as the game names it, e.g. "National league" or a competition.</summary>
    public string? Scope { get; set; }

    /// <summary>Matches still to sit out; null for a ban counted in days.</summary>
    public int? MatchesRemaining { get; set; }

    /// <summary>Days still to sit out; null for a ban counted in matches.</summary>
    public int? DaysRemaining { get; set; }

    /// <summary>Why, as the game words it, e.g. "3 yellow cards".</summary>
    public string? Reason { get; set; }
}
