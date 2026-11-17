using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A player in the match being played with his physical condition now (percent).</summary>
internal sealed class BridgeLiveConditionDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Condition)]
    public int? Condition { get; set; }
}
