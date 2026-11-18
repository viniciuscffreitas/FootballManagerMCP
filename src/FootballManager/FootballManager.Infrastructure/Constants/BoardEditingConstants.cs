using FootballManager.Domain.Board;

namespace FootballManager.Infrastructure.Constants;

// Dealings with the board, as the club's menu offers them. Every board action hangs on the club's menu (seen
// 17/11/2026 on Records:Human/Club): the budget and new-contract requests send EventFmxDoAction with the game's notice
// "A diretoria entrará em contato com você em breve"; "Pedido à Diretoria" and the affiliate requests send no event
// and open a talk with the board; the budget adjustment opens its panel (NewsAction 18).
internal static class BoardEditingConstants
{
    public const string Club = "Club";

    // FM.GamePlugin.ActionID values on the club's menu.
    public const uint RequestAction = 1297567845;
    public const uint IncreaseBudgetsAction = 1497974126;
    public const uint ManagerNewContractAction = 896087905;
    public const uint AffiliateLinkAction = 1229607543;
    public const uint SeniorAffiliateLinkAction = 1349411447;
    public const uint BudgetAdjustmentAction = 1885623898;

    public static uint ActionIdOf(BoardMenuAction action)
    {
        return action switch
        {
            BoardMenuAction.Request => RequestAction,
            BoardMenuAction.IncreaseBudgets => IncreaseBudgetsAction,
            BoardMenuAction.ManagerNewContract => ManagerNewContractAction,
            BoardMenuAction.AffiliateLink => AffiliateLinkAction,
            BoardMenuAction.SeniorAffiliateLink => SeniorAffiliateLinkAction,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No menu action for it.")
        };
    }

    // The budget adjustment panel. Its record answers only while the panel is open (pending otherwise). The menu entry's
    // SecondaryText sums up both budgets ("R$30 MI / R$52.5 mil p/s"); its references open the panel through /newsaction.
    public const int BudgetPanelNewsAction = 18;
    public const string BudgetPanel = Club + ".BudgetAdjustment";
    public const string SliderValue = "BudgetAdjustmentSliderValue";
    public const string TransferValue = "BudgetAdjustmentEditedTransferValue";
    public const string WageValue = "BudgetAdjustmentEditedOtherValue";
    public const string CanEdit = "BudgetAdjustmentCanEditBudgets";
    public const string CanEditUp = "BudgetAdjustmentCanEditBudgetUp";
    public const string CanEditDown = "BudgetAdjustmentCanEditBudgetDown";
    public const string BoardText = "BudgetAdjustmentBoardString";

    public static readonly string[] PanelProperties = [SliderValue, TransferValue, WageValue, CanEdit, CanEditUp, CanEditDown, BoardText];

    // The panel's events (InteropEventIDs). Not yet seen sent by hand: each step sends Adjust with +1 or -1 on the panel's
    // record, then Confirm with no data, as the game's confirm events usually go. To check in the smoke.
    public const string AdjustEvent = "EventFmxBudgetAdjustmentAdjust";
    public const string ConfirmEvent = "EventFmxBudgetAdjustmentConfirm";

    // A talk with the board on screen. The vision and expectations meeting's lines are #SpeakToMeetingAnswerActionButton;
    // the first SIButton inside one is what takes the click (the inner one does nothing; C:\Code\meeting.py, 16/11). A
    // quick chat card's lines are buttons with the suggestion-button class, as in a player conversation.
    public const string UiPath = "/ui?maxDepth=160";
    public const string MeetingAnswer = "#SpeakToMeetingAnswerActionButton";
    public const string SuggestionClass = "suggestion-button";
}
