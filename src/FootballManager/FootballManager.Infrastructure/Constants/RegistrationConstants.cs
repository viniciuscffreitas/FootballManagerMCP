using System.Globalization;

namespace FootballManager.Infrastructure.Constants;

// The registration and squad numbers screens, as the game binds them on the first team (seen 18/11/2026 in the test
// save). Each competition with a registration is an edit session in Club.MainTeam.SquadRegistrationComps; a player's
// tick there is its "Selected", read with the player passed as "Person".
internal static class RegistrationConstants
{
    public const string Competitions = TeamConstants.OwnTeam + ".SquadRegistrationComps";

    // On a registration's edit session.
    public const string Name = "SquadSelectionName";
    public const string CompetitionName = "Competition.Name";
    public const string CompetitionId = "Competition.UniqueId";
    public const string WindowOpen = "IsWindowOpen";
    public const string Window = "RegistrationDaysLeft";
    public const string Deadline = "DeadlineDate";
    public const string MaxSquadSize = "MaxSquadSize";
    public const string MeetsRules = "CurrentlyMeetsRule";
    public const string RulesSummary = "RuleWarningStrings";
    public const string Notes = "ExtraSquadRules";
    public const string Rules = "SquadSelectionRules";
    public const string Registered = "RegisteredPlayers";
    public const string Unregistered = "UnregisteredPlayers";
    public const string CanChange = "CanChangeSelection";
    public const string Person = "Person";

    public static readonly string[] CompetitionProperties =
        [Name, CompetitionName, CompetitionId, WindowOpen, Window, Deadline, MaxSquadSize, MeetsRules, RulesSummary, Notes, Rules];

    // On a player of those lists.
    public const string SquadNumber = "SquadNumber";
    public const string IsGoalkeeper = "IsGoalkeeper";
    public static readonly string[] PlayerProperties = [GameConstants.UniqueId, GameConstants.Name, SquadNumber, IsGoalkeeper];

    // The game's squad number for a player who has none.
    public const int NoNumber = -1;

    // The squad numbers screen's edit session, on the first team, for its first-team view (the filter its screen passes).
    public const string NumbersSession = TeamConstants.OwnTeam + ".SquadNumbersEditSession";
    public const string NumbersFilter = "SquadNumbersFilter";
    public const int FirstTeamFilter = 2;
    public const string CanChangeNumber = "CanChangeSquadNumber";
    public const string NumberDropdown = "SquadNumbersDropdown";
    public const string SuggestedNumber = "SuggestedNumber.SquadNumber";
    public const string NeedingNumbers = TeamConstants.OwnTeam + ".PlayersThatNeedSquadNumbers";
    public const string FirstTeamPlayers = TeamConstants.OwnTeam + "." + TeamConstants.Players;

    public static string Competition(int index)
    {
        return $"{Competitions}[{index.ToString(CultureInfo.InvariantCulture)}]";
    }

    public static string ById(string list, int playerId)
    {
        return $"{list}[{GameConstants.UniqueId}={playerId.ToString(CultureInfo.InvariantCulture)}]";
    }
}
