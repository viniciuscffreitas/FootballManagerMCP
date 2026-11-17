namespace FootballManager.Domain.Availability;

/// <summary>A first-team player's current injury, as his medical record in the game shows it.</summary>
public class PlayerInjury
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// The game's availability code for the injury (CurrentInjury.InjuryAvailability): 0 when it doesn't keep him
    /// out, 1 when he can't play.
    /// </summary>
    public int Availability { get; set; }

    /// <summary>The injury as the game names it, e.g. "A quad muscle strain".</summary>
    public string? Injury { get; set; }

    /// <summary>The game's estimate of his return, e.g. "Expected back in 8 days - 2 weeks".</summary>
    public string? ExpectedReturn { get; set; }

    /// <summary>Days until he is expected back.</summary>
    public int? ReturnDays { get; set; }
}
