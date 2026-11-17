using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>The tactic the game expects an opponent to use.</summary>
internal sealed class BridgeExpectedTacticDto
{
    [JsonPropertyName("FormationType")]
    public string? Formation { get; set; }

    public bool? IsValid { get; set; }

    public List<BridgeExpectedPlayerDto>? Players { get; set; }

    [JsonPropertyName("TacticSummaryString")]
    public string? Summary { get; set; }
}
