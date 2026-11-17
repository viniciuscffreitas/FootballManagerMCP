namespace FootballManager.Domain.Scouting;

/// <summary>
/// Cuts on a player's season numbers, which the game's player search can't make: kept only when he plays in one of
/// the leagues (any, when none are named), with enough minutes, a high enough average rating, and every number of
/// <see cref="Minimums"/> and <see cref="Maximums"/> within its bound.
/// </summary>
public class PerformanceFilter
{
    /// <summary>Whole league names as the game writes them ("Brasileirão" leaves out "Brasileirão Série B"); case and accents don't matter.</summary>
    public List<string> Leagues { get; set; } = [];
    public int? MinMinutes { get; set; }
    public double? MinAverageRating { get; set; }

    /// <summary>The least each named number may be, by its name in <see cref="Players.PlayerPerformance"/> (e.g. goalsPer90: 0.33).</summary>
    public Dictionary<string, double> Minimums { get; set; } = [];

    /// <summary>The most each named number may be (e.g. mistakesLeadingToGoal: 0).</summary>
    public Dictionary<string, double> Maximums { get; set; } = [];

    public bool IsEmpty => Leagues.Count == 0 && MinMinutes is null && MinAverageRating is null
                           && Minimums.Count == 0 && Maximums.Count == 0;
}
