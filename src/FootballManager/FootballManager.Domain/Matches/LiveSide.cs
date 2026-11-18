namespace FootballManager.Domain.Matches;

/// <summary>The user's side in the match being played, read before changing it.</summary>
public class LiveSide
{
    public bool IsHome { get; set; }

    /// <summary>As the match's tactic counts them; null when the game doesn't say.</summary>
    public int? SubstitutionsRemaining { get; set; }
}
