using FootballManager.Infrastructure.Dtos.Scouting;

namespace FootballManager.Infrastructure.Dtos.Messages;

/// <summary>
/// A message's answer button. The game lists some with their fields in a RowData of their own, as its dropdowns do.
/// </summary>
internal sealed class BridgeSpeakToActionDto
{
    public string? ActionTitle { get; set; }
    public long? Event { get; set; }
    public int? ActionResponseId { get; set; }
    public int? ActionResponseValue { get; set; }

    /// <summary>The panel the button opens; -1 for none.</summary>
    public int? NewsAction { get; set; }

    public bool? IsDisabled { get; set; }
    public bool? Recommended { get; set; }
    public BridgeConfirmationDto? ConfirmationDialogData { get; set; }
    public BridgeSpeakToActionDto? RowData { get; set; }
}
