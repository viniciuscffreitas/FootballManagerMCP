namespace FootballManager.Domain.Scouting;

/// <summary>
/// Cuts on a player's season numbers, which the game's player search can't make: kept only when he plays in one of
/// the leagues (any, when none are named), with enough minutes and a high enough average rating.
/// </summary>
public class PerformanceFilter
{
    /// <summary>Whole league names as the game writes them ("Brasileirão" leaves out "Brasileirão Série B"); case and accents don't matter.</summary>
    public List<string> Leagues { get; set; } = [];
    public int? MinMinutes { get; set; }
    public double? MinAverageRating { get; set; }

    public bool IsEmpty => Leagues.Count == 0 && MinMinutes is null && MinAverageRating is null;
}
