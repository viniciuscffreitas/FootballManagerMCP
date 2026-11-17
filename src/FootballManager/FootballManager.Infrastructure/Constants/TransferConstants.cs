using System.Text.RegularExpressions;

namespace FootballManager.Infrastructure.Constants;

// Transfers, through the game's own panels. The player's menu entry "make offer" opens the transfer offer panel
// (OpenNewsActionPanelEvent, NewsAction 6), and the club's acceptance opens the contract negotiation panel (NewsAction
// 7) from its message. The panels are UI only: they're read from the /ui tree and their buttons pressed with /click, as
// in the game on 16/11/2026 (Walter signed, Daniel Borges's R$790 mil offer sent).
internal static class TransferConstants
{
    public const int OfferPanelAction = 6;
    public const int ContractPanelAction = 7;

    // The whole tree, hidden elements included: the fee's own field (its number, "680000") lies under the combobox's
    // preview ("R$680 mil"). Deep enough for the panel's fee (depth 70 in the game).
    public const string UiPath = "/ui?maxDepth=100&visibleOnly=false";
    public const string ClickEndpoint = "/click";
    public const string ClickHow = "simulate";

    // A panel on screen; a closed one loses the visible class.
    public const string OfferPanel = "Panel#TransferOfferTool";
    public const string ContractPanel = "Panel#ContractNegotiationTool";
    public const string ShownPanel = "panel--visible";

    // The offer's addressee: "Mirassol" and "(Walter)".
    public const string OfferClub = "SIText#club-player-text";
    public const string OfferPlayer = "SIText#player-text";

    // The contract's: "Mirassol" and "(Walter)", under the page's title.
    public const string ContractClub = "SIText#club-subtitle-text";
    public const string ContractPlayer = "SIText#club-player-text";

    // A negotiation's line: its label, and four levels up the row holding it and what it's set to (a dropdown or a
    // combobox, e.g. "Goleiro titular", "R$680 mil" and its number).
    public const string TermLabel = "SIText#primary-text";
    public const string TermLabelClass = "body-md-strong";
    public const int TermRowLevels = 4;

    // The fee is the panel's first combobox: what it shows ("R$1 MI"), its number field's text ("1000000"), and its −
    // and + buttons (R$10 mil a press under R$1 mi, R$50 mil above).
    public const string Combobox = "FMCombobox";
    public const string FeePreview = "si-combobox__preview-element";
    public const string FeeField = "SILongField";
    public const string FeeText = "TextElement";

    // A short amount as the panel shows it, in Brazilian Portuguese: "R$1.05 MI", "R$990 mil".
    public static readonly Regex ShortAmount = new(@"(?<number>\d+(\.\d+)?)\s*(?<unit>MI|mil)\b", RegexOptions.CultureInvariant);
    public const string Millions = "MI";
    public const string LowerButton = "si-combobox__lower-button";
    public const string RaiseButton = "si-combobox__raise-button";

    // The column of the staff member beside the panel (the director of football on an offer): what he says, and the
    // panel's own buttons, whose texts are in the game's display language (Brazilian Portuguese in the user's game).
    public const string SpeakerColumn = "TemplateContainer#SingleNPCPanel";
    public const string SpeakerName = "SIText#Name";
    public const string SpeakerJob = "SIText#Job";
    public const string PanelButton = "SIButton#StandardButton";
    public const string MakeOffer = "Fazer Proposta";
    public const string FinalisePromises = "Finalizar Promessas";
    public const string NegotiateContract = "Negociar Contrato";
    public const string CloseDeal = "Fechar Acordo";
}
