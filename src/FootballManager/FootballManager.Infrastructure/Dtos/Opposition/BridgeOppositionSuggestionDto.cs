using System.Text.Json.Serialization;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>A staff suggestion: its line, its kind, whether the game greys it out, and the player by reference.</summary>
internal sealed class BridgeOppositionSuggestionDto
{
    public string? FeedbackString { get; set; }
    public int? FeedbackType { get; set; }
    public bool? IsDisabled { get; set; }

    [JsonPropertyName("player")]
    public BridgeReferenceDto? Player { get; set; }
}
