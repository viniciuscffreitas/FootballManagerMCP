namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>One chart of the opposition report; what it shows and the game's verdict are in its observation.</summary>
internal sealed class BridgeReportVisualDto
{
    public BridgeObservationDto? ObservationData { get; set; }
}
