namespace FootballManager.Infrastructure.Dtos.Messages;

/// <summary>The answers a message offers: its buttons, and whether one is needed before Continue moves on.</summary>
internal sealed class BridgeSpeakToActionsDto
{
    public bool? HasRequiredAction { get; set; }
    public int? RecommendedActionIndex { get; set; }
    public List<BridgeSpeakToActionDto>? ActionsForButtonsList { get; set; }
}
