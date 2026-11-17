namespace FootballManager.Infrastructure.Constants;

// The dressing room, as the game's squad dynamics screen reads it: the team's hierarchy lists, its open happiness
// issues and the club atmosphere on the team; the manager's promises on the user's manager; each player's level and
// happiness reasons on the player.
internal static class SquadDynamicsConstants
{
    // The hierarchy, one list of players per level, from the most influential.
    public const string TeamLeaders = TeamConstants.OwnTeam + ".HierarchyTeamLeaders";
    public const string HighlyInfluential = TeamConstants.OwnTeam + ".HierarchyHighlyInfluential";
    public const string Influential = TeamConstants.OwnTeam + ".HierarchyInfluential";
    public const string Other = TeamConstants.OwnTeam + ".HierarchyOther";

    public static readonly string[] MemberProperties = [GameConstants.UniqueId, GameConstants.Name];

    // The open happiness issues; each item names the player who raised it.
    public const string Issues = TeamConstants.OwnTeam + ".HappinessIssuesList";
    public const string IssuePlayerId = "player.UniqueId";
    public const string IssuePlayerName = "player.Name";
    public const string IssueSeverity = "SeverityString";
    public const string IssueDescription = "Description";
    public const string IssuePlayersAgreeing = "PlayersAgreeingCount";

    // The promises tied to the issue; the game's own "has promises" is a translated yes/no.
    public const string IssueLinkedPromises = "IssueLinkedPromises";

    public static readonly string[] IssueProperties =
    [
        IssuePlayerId, IssuePlayerName, IssueSeverity, IssueDescription, IssuePlayersAgreeing, IssueLinkedPromises
    ];

    // The club atmosphere: the short verdict, its 0-100 score, and the reasons (each a text and a weight).
    public const string Atmosphere = TeamConstants.OwnTeam + ".ClubAtmosphereFeedback";
    public const string AtmosphereSummary = "CurrentDynamicSupportStringShort";
    public const string AtmosphereScore = "CurrentDynamicSupportValue100";
    public const string AtmosphereFactors = "DynamicsProCons";
    public const string FactorText = "ProConsLongString";
    public const string FactorValue = "DynamicsProConValue";

    public static readonly string[] AtmosphereProperties = [AtmosphereSummary, AtmosphereScore, AtmosphereFactors];

    // The promises the user's manager made to players, on the manager (root Human).
    public const string Promises = "HumanPlayerPromises";
    public const string PromiseTitle = "InteractionPromiseTitle";
    public const string PromiseDescription = "InteractionPromiseDescription";
    public const string PromiseStatus = "InteractionPromiseStatusText";
    public const string PromiseDaysRemaining = "InteractionPromiseDaysRemaining";
    public const string PromiseEndDate = "EndDate";

    public static readonly string[] PromiseProperties =
    [
        PromiseTitle, PromiseDescription, PromiseStatus, PromiseDaysRemaining, PromiseEndDate
    ];

    // On each first-team player. The happiness reasons are lists of items whose Name is the game's sentence.
    public const string HierarchyLevel = "HierarchyInfluenceCategoryString";
    public const string HappinessPositives = "HappinessPositives";
    public const string HappinessNegatives = "HappinessNegatives";
    public const string PromisesCount = "PromisesCount";
    public const string ReasonText = "Name";

    public static readonly string[] PlayerProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, HierarchyLevel, HappinessPositives, HappinessNegatives, PromisesCount
    ];
}
