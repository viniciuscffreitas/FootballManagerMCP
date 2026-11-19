namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>
/// The payload a menu entry sends. Only its dialogs are read here: the payload itself is sent from the game's own copy
/// when the entry is picked.
/// </summary>
internal sealed class BridgeActionDataDto
{
    public BridgeConfirmationDto? ConfirmationDialogData { get; set; }

    /// <summary>The notice the game shows once the entry ran, e.g. the board's "it will get back to you".</summary>
    public BridgeConfirmationDto? OKDialogData { get; set; }

    /// <summary>The panel the entry opens (-1 or missing: none), e.g. 19 the board meeting card.</summary>
    public int? NewsAction { get; set; }

    /// <summary>The card that panel shows, e.g. 26 the board meeting.</summary>
    public int? DynamicCardId { get; set; }
}
