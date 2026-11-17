namespace FootballManager.Infrastructure.Constants;

// The game's medical centre: each first-team player's injury history and risk, and the team's season summary.
internal static class InjuryConstants
{
    public const string Players = TeamConstants.OwnTeam + "." + TeamConstants.Players;

    // On a player. InjuriesHistory is a list of injury records, oldest first, the one he has now included.
    public const string History = "InjuriesHistory";
    public const string ThisSeason = "InjuriesThisSeason";
    public const string RiskLevel = "InjuryRiskLevel";
    public const string RiskOverview = "InjuryRiskOverview";
    public const string RiskReason = "InjuryRiskFactorReason";

    public static readonly string[] PlayerProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, History, ThisSeason, RiskLevel, RiskOverview, RiskReason
    ];

    // On an injury record of the history list, which the game sends whole.
    public const string Date = "InjuryDate";
    public const string Description = "InjuryDescription";
    public const string Duration = "InjuryDuration";
    public const string DurationDays = "InjuryDurationDays";
    public const string Severity = "InjurySeverity";
    public const string Cause = "InjuryCauseShortString";
    public const string ReceivedIn = "InjuryReceivedTypeShortString";
    public const string IsActive = "IsActive";

    public const string DateFormat = "yyyy-MM-dd";

    // On the team. SeasonInjurySummaryData is the medical centre's season panel; its sub-paths are read one by one,
    // since the whole record carries every injury of the season and a graph.
    private const string Summary = "SeasonInjurySummaryData.";
    public const string SeasonCount = Summary + "CurrentInjuryListCount";
    public const string ComparedWithLastSeason = Summary + "InjuriesLastSeasonCountString";
    public const string ExpectedOverSeason = Summary + "ExpectedInjuriesOverSeasonString";
    public const string LeagueAverage = Summary + "LeagueAverageInjuriesString";
    public const string SlightCount = Summary + "SlightInjuryCount";
    public const string MinorCount = Summary + "MinorInjuryCount";
    public const string ModerateCount = Summary + "ModerateInjuryCount";
    public const string MajorCount = Summary + "MajorInjuryCount";
    public const string HighRiskCount = "HighRiskInjuryListCount";
    public const string ExhaustedAndRecurring = "ExhaustedAndRecurringInjuriesString";
    public const string ExpectedReturnsNextMatch = "InjuryExpectedReturnsNextFixtureString";

    public static readonly string[] SeasonProperties =
    [
        SeasonCount, ComparedWithLastSeason, ExpectedOverSeason, LeagueAverage, SlightCount, MinorCount, ModerateCount,
        MajorCount, HighRiskCount, ExhaustedAndRecurring, ExpectedReturnsNextMatch
    ];
}
