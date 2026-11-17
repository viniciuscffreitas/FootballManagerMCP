namespace FootballManager.Infrastructure.Constants;

// The game's opposition report, as its pre-match analysis binds it. The report is read on the opponent's team with the
// user's team as the one requesting it, in possession; the threat level is read on the user's team and is only about
// the next opponent.
internal static class OppositionConstants
{
    public const string NextOpponent = TeamConstants.OwnTeam + ".NextMatchOpponent";
    public const string NextOpponentId = "NextMatchOpponent." + GameConstants.UniqueId;
    public const string NextOpponentName = "NextMatchOpponent." + GameConstants.Name;
    public const string ThreatLevel = "NextOppositionThreatLevelAssessment";

    public static readonly string[] NextOpponentProperties = [NextOpponentId, NextOpponentName, ThreatLevel];

    // Context: the team the report is written for.
    public const string RequestingTeam = "TeamRequestingReport";

    // On the opponent: the tactic the game expects, {FormationType, IsValid, Players [{Position, Role, player}],
    // TacticSummaryString}; and the analysis' charts, each with the game's verdict on it.
    public const string ExpectedTactic = "TeamReportExpectedTactic";
    public const string Visuals = "OppositionReportVisuals";

    public static readonly string[] ReportProperties = [GameConstants.UniqueId, GameConstants.Name, ExpectedTactic, Visuals];
}
