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

    // The budget adjustment panel (NewsAction 18), opened with the menu entry's references through /newsaction; the
    // entry's SecondaryText sums up both budgets ("R$30 MI / R$52.5 mil p/s"). The panel is a modal dialog read from
    // the /ui tree (seen 18/11/2026): the wage budget on the left, the transfer budget on the right, a slider between
    // them with a button each side. "Down" moves one step of money from transfers to wages, "Up" back (the game sends
    // EventFmxBudgetAdjustmentAdjust with -1 or 1); a side the board won't let grow is greyed out (unity-disabled).
    // The primary button confirms and closes the panel; until then the edited values carry a "*".
    public const int BudgetPanelNewsAction = 18;
    public const string BudgetPanelUiPath = "/ui?maxDepth=40";
    public const string BudgetDialog = "Panel#ClubBudgetAdjustmentDialog";
    public const string WageLabel = "Orçamento salarial";
    public const string TransferLabel = "Orçamento de transferências";
    public const string BoardText = "SIText#BoardText";
    public const string DownEnabled = "SIEnabled#AdjustDownEnabled";
    public const string UpEnabled = "SIEnabled#AdjustUpEnabled";
    public const string DownButton = "SIButton#AdjustDownButton";
    public const string UpButton = "SIButton#AdjustUpButon";
    public const string Disabled = "unity-disabled";
    public const string ConfirmButton = "SIButton#button-primary";
    public const string SliderTracker = "VisualElement#unity-tracker";
    public const string SliderDragger = "VisualElement#unity-dragger";
    public const string EditedMark = "*";

    // A talk with the board on screen. The vision and expectations meeting's lines are #SpeakToMeetingAnswerActionButton;
    // the first SIButton inside one is what takes the click (the inner one does nothing). A
    // quick chat card's lines are buttons with the suggestion-button class, as in a player conversation.
    public const string UiPath = "/ui?maxDepth=160";
    public const string MeetingAnswer = "#SpeakToMeetingAnswerActionButton";
    public const string SuggestionClass = "suggestion-button";
}
