using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>Where the match day stands, as the Continue button names it.</summary>
internal sealed class BridgeMatchDayStateDto
{
    [JsonPropertyName(LiveControlConstants.State)]
    public string? State { get; set; }
}
