using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>One side of the match being played: its numbers and the substitutions its tactic has left.</summary>
internal sealed class BridgeLiveSideDto
{
    [JsonPropertyName(MatchConstants.TeamStats)]
    public BridgeTeamMatchStatsDto? Stats { get; set; }

    [JsonPropertyName(LiveMatchConstants.SubstitutionsRemaining)]
    public JsonElement? SubstitutionsRemaining { get; set; }

    [JsonPropertyName(LiveMatchConstants.SubstitutionsAllowed)]
    public JsonElement? SubstitutionsAllowed { get; set; }
}
