namespace FootballManager.Infrastructure.Constants;

// Changing the match being played, as the match screen does. The match's own tactic is "Tactic" on the Match root,
// one per side (context isHomeTeam); its places are the matchday squad's (context TeamSelectionIndex: 0-10 the eleven,
// 11 on the bench), each with the player there and its position mask (0 on the bench).
internal static class LiveControlConstants
{
    public const string MatchTactic = "Tactic";
    public const int LastPlace = 22;

    // A substitution is the drag of a substitute onto a player on the pitch in the match's tactic: the game's
    // EventFmxSwapPlayers, with the dragged place (DragData) and the one dropped on (DropData), each the place's
    // position mask with its index and two flags the drag sets. The tactic swaps them at once (IsMakingSubstitutions
    // true) and the game makes the substitution at the next stoppage, about a minute of play later (seen on 17/11/2026,
    // Neymar off for Coutinho at 52': SubsRemaining 5 to 4).
    public const string SwapEvent = "EventFmxSwapPlayers";
    public const string Drag = "DragData";
    public const string Drop = "DropData";
    public const string DragIsPlace = "DragDropExtraData1";
    public const string DragIndex = "DragDropExtraData2";
    public const string DragKind = "DragDropExtraData3";
    public const string DragKindPlace = "u:1";

    // The Continue button's record: where the match day stands (TeamTalk, WarmUp, HalfTime, FullTimeTeamTalk...).
    public const string ContinueRoot = "continue";
    public const string State = "State";

    // The team talk (before kick-off, at half-time, at full time) is the TeamTalkPanel: its lines are suggestion
    // buttons, as on a conversation card, read from the /ui tree and pressed with /click (no event replays them). The
    // whole squad's tab is the one open. The assistant's advice is marked by its label over the first line.
    public const string TeamTalkPanel = "TeamTalkPanel";
    public const string AdviceLabel = "Conselhos";
}
