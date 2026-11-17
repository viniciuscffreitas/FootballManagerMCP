using FootballManager.Domain.Players;

namespace FootballManager.Infrastructure.Constants;

// A player's season numbers for his club, as the game's player profile reads them on any person: most come as a small
// record with the value and how many matches it covers ({"PropertyValue": 0.41, "Count": 17}), a few as plain numbers.
// Each path with where its number goes.
internal static class PerformanceConstants
{
    // The competition his club plays its league in: a reference, read by its name.
    public const string League = "Division.Name";

    public static readonly IReadOnlyList<(string Path, Action<PlayerPerformance, double?> Set)> Stats =
    [
        ("ClubAppearancesThisSeason", (p, v) => p.Appearances = Whole(v)),
        ("Minutes", (p, v) => p.Minutes = Whole(v)),
        ("ClubAverageRatingThisSeason", (p, v) => p.AverageRating = v),

        ("Goals", (p, v) => p.Goals = Whole(v)),
        ("Assists", (p, v) => p.Assists = Whole(v)),
        ("PenaltiesTaken", (p, v) => p.PenaltiesTaken = Whole(v)),
        ("PenaltiesScored", (p, v) => p.PenaltiesScored = Whole(v)),
        ("ExpectedGoals", (p, v) => p.ExpectedGoals = v),
        ("NonPenaltyExpectedGoals", (p, v) => p.NonPenaltyExpectedGoals = v),
        ("ExpectedGoalsOverperformance", (p, v) => p.ExpectedGoalsOverperformance = v),
        ("ExpectedGoalsPerShot", (p, v) => p.ExpectedGoalsPerShot = v),
        ("ExpectedAssists", (p, v) => p.ExpectedAssists = v),
        ("ShotAttempts", (p, v) => p.Shots = Whole(v)),
        ("ShotsOnTarget", (p, v) => p.ShotsOnTarget = Whole(v)),
        ("ShotsOnTargetPercentage", (p, v) => p.ShotsOnTargetPercentage = v),
        ("GoalsPerShotPercentage", (p, v) => p.GoalsPerShotPercentage = v),

        ("KeyPasses", (p, v) => p.KeyPasses = Whole(v)),
        ("ChancesCreated", (p, v) => p.ChancesCreated = Whole(v)),
        ("PassAttempts", (p, v) => p.PassesAttempted = Whole(v)),
        ("PassCompletionPercentage", (p, v) => p.PassCompletionPercentage = v),
        ("ProgressivePasses", (p, v) => p.ProgressivePasses = Whole(v)),
        ("CrossAttempts", (p, v) => p.CrossesAttempted = Whole(v)),
        ("CrossesCompleted", (p, v) => p.CrossesCompleted = Whole(v)),
        ("DribblesMade", (p, v) => p.Dribbles = Whole(v)),

        ("TackleAttempts", (p, v) => p.TacklesAttempted = Whole(v)),
        ("TackleCompletionPercentage", (p, v) => p.TackleCompletionPercentage = v),
        ("Interceptions", (p, v) => p.Interceptions = Whole(v)),
        ("PressuresAttempted", (p, v) => p.PressuresAttempted = Whole(v)),
        ("PressuresCompleted", (p, v) => p.PressuresCompleted = Whole(v)),
        ("HeaderAttempts", (p, v) => p.HeadersAttempted = Whole(v)),
        ("HeaderCompletionPercentage", (p, v) => p.HeaderCompletionPercentage = v),
        ("Clearances", (p, v) => p.Clearances = Whole(v)),
        ("Blocks", (p, v) => p.Blocks = Whole(v)),
        ("GoalMistakes", (p, v) => p.MistakesLeadingToGoal = Whole(v)),
        ("PossessionLost", (p, v) => p.PossessionLost = Whole(v)),
        ("DistanceRan", (p, v) => p.DistanceCoveredKm = v),

        ("GoalsPer90", (p, v) => p.GoalsPer90 = v),
        ("AssistsPer90", (p, v) => p.AssistsPer90 = v),
        ("ExpectedGoalsPer90", (p, v) => p.ExpectedGoalsPer90 = v),
        ("NonPenaltyExpectedGoalsPer90", (p, v) => p.NonPenaltyExpectedGoalsPer90 = v),
        ("ExpectedAssistsPer90", (p, v) => p.ExpectedAssistsPer90 = v),
        ("ShotsPer90", (p, v) => p.ShotsPer90 = v),
        ("KeyPassesPer90", (p, v) => p.KeyPassesPer90 = v),
        ("ChancesCreatedPer90", (p, v) => p.ChancesCreatedPer90 = v),
        ("ProgressivePassesPer90", (p, v) => p.ProgressivePassesPer90 = v),
        ("CrossesCompletedPer90", (p, v) => p.CrossesCompletedPer90 = v),
        ("DribblesMadePer90", (p, v) => p.DribblesPer90 = v),
        ("TacklesCompletedPer90", (p, v) => p.TacklesCompletedPer90 = v),
        ("InterceptionsPer90", (p, v) => p.InterceptionsPer90 = v),
        ("PressuresAttemptedPer90", (p, v) => p.PressuresAttemptedPer90 = v),
        ("PressuresCompletedPer90", (p, v) => p.PressuresCompletedPer90 = v),
        ("HeadersCompletedPer90", (p, v) => p.HeadersWonPer90 = v),
        ("PossessionWonPer90", (p, v) => p.PossessionWonPer90 = v),
        ("PossessionLostPer90", (p, v) => p.PossessionLostPer90 = v),
        ("DistanceRanPer90", (p, v) => p.DistanceCoveredKmPer90 = v),
        ("SprintsPer90", (p, v) => p.SprintsPer90 = v),

        ("TotalSaves", (p, v) => p.Saves = Whole(v)),
        ("TotalSavePercentage", (p, v) => p.SavePercentage = v),
        ("TotalExpectedSavePercentage", (p, v) => p.ExpectedSavePercentage = v),
        ("ExpectedGoalsPrevented", (p, v) => p.ExpectedGoalsPrevented = v),
        ("ExpectedGoalsPreventedPer90", (p, v) => p.ExpectedGoalsPreventedPer90 = v),
        ("CleanSheets", (p, v) => p.CleanSheets = Whole(v)),
        ("Conceded", (p, v) => p.GoalsConceded = Whole(v)),
        ("ConcededPer90", (p, v) => p.GoalsConcededPer90 = v)
    ];

    public static IEnumerable<string> Paths => Stats.Select(stat => stat.Path).Append(League);

    private static int? Whole(double? value)
    {
        return value is { } number ? (int)Math.Round(number) : null;
    }
}
