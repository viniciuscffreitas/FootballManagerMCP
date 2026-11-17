using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>The tactic the match is played with, and the substitutions it has left.</summary>
internal sealed class BridgeLiveTacticDto
{
    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(TacticConstants.FormationName)]
    public string? Formation { get; set; }

    [JsonPropertyName(TacticConstants.Mentality)]
    public string? Mentality { get; set; }

    [JsonPropertyName(LiveMatchConstants.SubstitutionsRemaining)]
    public JsonElement? SubstitutionsRemaining { get; set; }

    [JsonPropertyName(LiveMatchConstants.SubstitutionsAllowed)]
    public JsonElement? SubstitutionsAllowed { get; set; }
}
