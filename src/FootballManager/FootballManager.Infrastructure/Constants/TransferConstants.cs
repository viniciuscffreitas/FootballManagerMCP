namespace FootballManager.Infrastructure.Constants;

// Transfers, through the game's own panels. The player's menu entry "make offer" opens the transfer offer panel
// (OpenNewsActionPanelEvent, NewsAction 6). The panel is UI only: it's read from the /ui tree and its buttons pressed
// with /click, as in the game on 16/11/2026 (Walter signed, Daniel Borges's R$790 mil offer sent).
internal static class TransferConstants
{
    public const int OfferPanelAction = 6;

    // The whole tree, hidden elements included: the fee's own field (its number, "680000") lies under the combobox's
    // preview ("R$680 mil"). Deep enough for the panel's fee (depth 70 in the game).
    public const string UiPath = "/ui?maxDepth=100&visibleOnly=false";
    public const string ClickEndpoint = "/click";
    public const string ClickHow = "simulate";

    // A panel on screen; a closed one loses the visible class.
    public const string OfferPanel = "Panel#TransferOfferTool";
    public const string ShownPanel = "panel--visible";

    // The offer's addressee: "Mirassol" and "(Walter)".
    public const string OfferClub = "SIText#club-player-text";
    public const string OfferPlayer = "SIText#player-text";

    // The fee is the panel's first combobox: its number field's text, and its − and + buttons (R$10 mil a press).
    public const string Combobox = "FMCombobox";
    public const string FeeText = "TextElement";
    public const string LowerButton = "si-combobox__lower-button";
    public const string RaiseButton = "si-combobox__raise-button";

    // The column of the staff member beside the panel (the director of football on an offer): what he says, and the
    // panel's own buttons, whose texts are in the game's display language (Brazilian Portuguese in the user's game).
    public const string SpeakerColumn = "TemplateContainer#SingleNPCPanel";
    public const string SpeakerName = "SIText#Name";
    public const string SpeakerJob = "SIText#Job";
    public const string PanelButton = "SIButton#StandardButton";
    public const string MakeOffer = "Fazer Proposta";
}
