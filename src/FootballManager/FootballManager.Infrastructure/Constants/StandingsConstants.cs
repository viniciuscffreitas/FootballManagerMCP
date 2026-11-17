namespace FootballManager.Infrastructure.Constants;

// The user's league table, as the game's competition screen reads it: the table of the league's current stage, one
// item per team. A league between seasons has no current stage, and reading through it fails, so the stage is checked
// first.
internal static class StandingsConstants
{
    // On the user's team.
    public const string LeagueStage = "Division.CurrentStage";
    public const string MediaExpectedPosition = "MediaExpectedPosition";

    public static readonly string[] TeamProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, TeamConstants.DivisionName, LeagueStage, MediaExpectedPosition
    ];

    // What a stage reference says it is; a missing stage comes as the game's null reference, with no type.
    public const string StageType = "CompetitionStage";

    // The table; the game's table screen passes 0 for the overall table (not home or away).
    public const string Table = TeamConstants.OwnTeam + "." + LeagueStage + ".LeagueTable";
    public const string TableType = "LeagueTableType";
    public const long OverallTable = 0;

    // On a table row; its team is a reference, read through.
    public const string TeamName = "Team.Name";
    public const string Position = "TablePosition";
    public const string Played = "LeagueGamesPlayed";
    public const string Won = "LeagueGamesWon";
    public const string Drawn = "LeagueGamesDrawn";
    public const string Lost = "LeagueGamesLost";
    public const string GoalsFor = "LeagueGoalsFor";
    public const string GoalsAgainst = "LeagueGoalsAgainst";
    public const string GoalDifference = "LeagueGoalDifference";
    public const string Points = "LeaguePoints";
    public const string Fate = "LeagueFate.Name";
    public const string Rising = "IsRisingUpTable";
    public const string Falling = "IsFallingDownTable";

    public static readonly string[] RowProperties =
    [
        GameConstants.TeamId, TeamName, Position, Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst, GoalDifference, Points,
        Fate, Rising, Falling
    ];

}
