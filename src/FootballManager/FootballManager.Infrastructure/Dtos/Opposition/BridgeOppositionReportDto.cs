using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>An opponent's report, read on the opponent's team.</summary>
internal sealed class BridgeOppositionReportDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(OppositionConstants.ExpectedTactic)]
    public BridgeExpectedTacticDto? ExpectedTactic { get; set; }

    [JsonPropertyName(OppositionConstants.Visuals)]
    public List<BridgeReportVisualDto>? Visuals { get; set; }
}
