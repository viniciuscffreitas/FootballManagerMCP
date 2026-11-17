namespace FootballManager.Domain.Players;

/// <summary>
/// A player's numbers this season for his club, all competitive matches together, as the game's player profile shows
/// them for any player: totals and per 90 minutes. Null where the game has no number (no match played, a goalkeeper's
/// numbers for an outfield player, ...).
/// </summary>
public class PlayerPerformance
{
    // Playing time.
    public int? Appearances { get; set; }
    public int? Minutes { get; set; }
    public double? AverageRating { get; set; }

    // Scoring.
    public int? Goals { get; set; }
    public int? Assists { get; set; }
    public int? PenaltiesTaken { get; set; }
    public int? PenaltiesScored { get; set; }
    public double? ExpectedGoals { get; set; }
    public double? NonPenaltyExpectedGoals { get; set; }

    /// <summary>Goals minus expected goals: below zero means scoring less than the chances were worth.</summary>
    public double? ExpectedGoalsOverperformance { get; set; }
    public double? ExpectedGoalsPerShot { get; set; }
    public double? ExpectedAssists { get; set; }
    public int? Shots { get; set; }
    public int? ShotsOnTarget { get; set; }
    public double? ShotsOnTargetPercentage { get; set; }

    /// <summary>Goals per shot, as a percentage: the conversion rate.</summary>
    public double? GoalsPerShotPercentage { get; set; }

    // Creating.
    public int? KeyPasses { get; set; }
    public int? ChancesCreated { get; set; }
    public int? PassesAttempted { get; set; }
    public double? PassCompletionPercentage { get; set; }
    public int? ProgressivePasses { get; set; }
    public int? CrossesAttempted { get; set; }
    public int? CrossesCompleted { get; set; }
    public int? Dribbles { get; set; }

    // Defending.
    public int? TacklesAttempted { get; set; }
    public double? TackleCompletionPercentage { get; set; }
    public int? Interceptions { get; set; }
    public int? PressuresAttempted { get; set; }
    public int? PressuresCompleted { get; set; }
    public int? HeadersAttempted { get; set; }
    public double? HeaderCompletionPercentage { get; set; }
    public int? Clearances { get; set; }
    public int? Blocks { get; set; }

    /// <summary>Mistakes that led to a goal.</summary>
    public int? MistakesLeadingToGoal { get; set; }
    public int? PossessionLost { get; set; }
    public double? DistanceCoveredKm { get; set; }

    // Per 90 minutes.
    public double? GoalsPer90 { get; set; }
    public double? AssistsPer90 { get; set; }
    public double? ExpectedGoalsPer90 { get; set; }
    public double? NonPenaltyExpectedGoalsPer90 { get; set; }
    public double? ExpectedAssistsPer90 { get; set; }
    public double? ShotsPer90 { get; set; }
    public double? KeyPassesPer90 { get; set; }
    public double? ChancesCreatedPer90 { get; set; }
    public double? ProgressivePassesPer90 { get; set; }
    public double? CrossesCompletedPer90 { get; set; }
    public double? DribblesPer90 { get; set; }
    public double? TacklesCompletedPer90 { get; set; }
    public double? InterceptionsPer90 { get; set; }
    public double? PressuresAttemptedPer90 { get; set; }
    public double? PressuresCompletedPer90 { get; set; }
    public double? HeadersWonPer90 { get; set; }
    public double? PossessionWonPer90 { get; set; }
    public double? PossessionLostPer90 { get; set; }
    public double? DistanceCoveredKmPer90 { get; set; }
    public double? SprintsPer90 { get; set; }

    // Goalkeeping.
    public int? Saves { get; set; }
    public double? SavePercentage { get; set; }
    public double? ExpectedSavePercentage { get; set; }
    public double? ExpectedGoalsPrevented { get; set; }
    public double? ExpectedGoalsPreventedPer90 { get; set; }
    public int? CleanSheets { get; set; }
    public int? GoalsConceded { get; set; }
    public double? GoalsConcededPer90 { get; set; }
}
