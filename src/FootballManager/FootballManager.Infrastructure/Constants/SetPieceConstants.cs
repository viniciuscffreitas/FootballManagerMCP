namespace FootballManager.Infrastructure.Constants;

// The captaincy and the set piece takers, as the game keeps them (read in the test save on 17/11/2026).
// The captain and the vice are the team's (Club.MainTeam.Captain / ViceCaptain). Everything else is the active tactic's
// SetPiece record (Team.Tactic.SetPiece, with the tactic flag): the penalty takers and the armband's stand-ins are
// lists of {Index, Player}; every other taker list is SetPieceTakerDataForSide.SetPieceTakerList, chosen by the
// contexts SetPieceType (the set piece screen's dropdown value), SetPieceScenario (1: attacking) and SetPiecePitchSide.
internal static class SetPieceConstants
{
    public const string Team = TeamConstants.OwnTeam;
    public const string Squad = TeamConstants.OwnTeam + "." + TeamConstants.Players;

    public const string CaptainId = "Captain." + GameConstants.UniqueId;
    public const string CaptainName = "Captain." + GameConstants.Name;
    public const string ViceCaptainId = "ViceCaptain." + GameConstants.UniqueId;
    public const string ViceCaptainName = "ViceCaptain." + GameConstants.Name;

    public const string SetPiece = TacticConstants.Tactic + ".SetPiece";
    public const string PenaltyTakers = SetPiece + ".SetPiecePenaltyTakersList";
    public const string CaptainStandIns = SetPiece + ".CaptainStandInList";
    public const string SideTakers = SetPiece + ".SetPieceTakerDataForSide.SetPieceTakerList";

    public const string PlayerId = "Player." + GameConstants.UniqueId;
    public const string PlayerName = "Player." + GameConstants.Name;

    public const string TypeContext = "SetPieceType";
    public const string ScenarioContext = "SetPieceScenario";
    public const string SideContext = "SetPiecePitchSide";
    public const int Attacking = 1;

    // SetPiecePitchSide: 0 and 1. Which one the screen shows as left hasn't been checked against the screen yet; SI's
    // order (left first) is assumed.
    public const int LeftSide = 0;
    public const int RightSide = 1;

    public const string PenaltiesKey = "Penalties";

    // The taker lists of the set piece screen's dropdown (SetPieceTypeDropdownList): its value, the key the tools
    // take, and its label. Throw-ins have one entry per third of the pitch, all with the same takers: one key for them.
    public static readonly (string Key, int Type, string Label)[] SideLists =
    [
        ("Corners", 0, "Corners"),
        ("FreeKicksShooting", 1, "Free kicks, shooting range"),
        ("FreeKicksWide", 3, "Free kicks, wide"),
        ("FreeKicksLong", 4, "Free kicks, long"),
        ("ThrowIns", 6, "Throw-ins")
    ];

    // Making a player captain or vice: his menu's entry, an EventFmxDoAction. The current captain's menu doesn't offer
    // "Tornar Capitão" (it offers taking the armband off instead), nor the vice's "Tornar Vice-capitão".
    public const uint MakeCaptainAction = 1349150752;
    public const uint MakeViceCaptainAction = 944393847;
    public const string EntryData = "Data:@" + ScoutingConstants.MenuItems + "[0].ActionData";
}
