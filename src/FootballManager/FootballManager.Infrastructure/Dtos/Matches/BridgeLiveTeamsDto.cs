using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>The user's team, and the date of the match the manager plays next (or is playing).</summary>
internal sealed class BridgeLiveTeamsDto
{
    [JsonPropertyName(GameConstants.TeamId)]
    public int? TeamId { get; set; }

    /// <summary>"yyyy-MM-dd".</summary>
    [JsonPropertyName(LiveMatchConstants.MatchDate)]
    public string? MatchDate { get; set; }
}
