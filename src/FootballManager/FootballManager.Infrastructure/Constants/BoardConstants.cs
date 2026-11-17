namespace FootballManager.Infrastructure.Constants;

// The board's and the supporters' view of the user, as the game's club vision screen reads it.
internal static class BoardConstants
{
    // The team's confidence record holds both the board's and the supporters' grades and texts.
    public const string TeamConfidence = TeamConstants.OwnTeam + ".TeamConfidence";

    public const string BoardGrade = "BoardConfidenceOverallGrade";
    public const string BoardSummary = "BoardConfidenceOverallString";
    public const string BoardMatches = "BoardMatchString";
    public const string BoardSquadSupport = "BoardSquadConfidenceString";
    public const string BoardTactics = "BoardTacticsString";
    public const string BoardVision = "BoardVisionString";
    public const string BoardTransfers = "BoardTransferConfidenceOverallSummaryText";
    public const string BoardHighlights = "BoardFeedbackHighlights";
    public const string BoardCriticism = "BoardFeedbackCriticism";
    public const string SupporterGrade = "SupporterConfidenceOverallGrade";
    public const string SupporterSummary = "SupporterConfidenceOverallString";
    public const string SupporterMatches = "SupporterMatchString";
    public const string SupporterVision = "SupporterVisionString";

    public static readonly string[] ConfidenceProperties =
    [
        BoardGrade, BoardSummary, BoardMatches, BoardSquadSupport, BoardTactics, BoardVision, BoardTransfers,
        BoardHighlights, BoardCriticism, SupporterGrade, SupporterSummary, SupporterMatches, SupporterVision
    ];

    // The club vision, read on the club. Its objectives answer only once told to use the vision's own targets rather
    // than performance data, as the vision screen does.
    public const string Club = "Club";
    public const string UsingPerformanceData = "VisionIsUsingPerformanceData";

    public const string ThisYearName = "VisionObjectiveThisYearObjective.VisionObjectiveName";
    public const string ThisYearYear = "VisionObjectiveThisYearObjective.VisionObjectiveYearString";
    public const string ThisYearProgress = "VisionObjectiveThisYearObjective.VisionObjectiveProgress";
    public const string ThisYearStatus = "VisionObjectiveThisYearObjective.VisionObjectiveStatus";
    public const string FinalName = "VisionObjectiveFinalYearObjective.VisionObjectiveName";
    public const string FinalYear = "VisionObjectiveFinalYearObjective.VisionObjectiveYearString";
    public const string FinalProgress = "VisionObjectiveFinalYearObjective.VisionObjectiveProgress";
    public const string FinalStatus = "VisionObjectiveFinalYearObjective.VisionObjectiveStatus";
    public const string BoardRequests = "BoardRequestsCount";
    public const string ActiveBoardRequests = "ActiveBoardRequestsCount";

    // One line per item, separated by line breaks; the board's reads "Nenhum(a)" when it has none.
    public const string SupporterCulture = "SupporterCulture";
    public const string BoardCulture = "BoardCulture";

    public static readonly string[] VisionProperties =
    [
        ThisYearName, ThisYearYear, ThisYearProgress, ThisYearStatus, FinalName, FinalYear, FinalProgress, FinalStatus,
        BoardRequests, ActiveBoardRequests, SupporterCulture, BoardCulture
    ];

    // On the user's manager: the promises made to the board; an empty list when there are none.
    public const string Promises = "HumanBoardPromises";
}
