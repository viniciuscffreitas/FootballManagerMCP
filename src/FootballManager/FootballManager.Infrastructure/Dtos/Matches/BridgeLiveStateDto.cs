using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>Whether the game is in a match, and its date.</summary>
internal sealed class BridgeLiveStateDto
{
    [JsonPropertyName(LiveMatchConstants.IsDuringMatch)]
    public bool? IsDuringMatch { get; set; }

    /// <summary>"yyyy-MM-dd".</summary>
    [JsonPropertyName(LiveMatchConstants.CurrentDate)]
    public string? CurrentDate { get; set; }
}
