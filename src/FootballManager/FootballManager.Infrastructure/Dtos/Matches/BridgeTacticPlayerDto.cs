using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>The player at a lineup place of the tactic the team plays.</summary>
internal sealed class BridgeTacticPlayerDto
{
    [JsonPropertyName(TacticConstants.LineupPlayerId)]
    public int? PlayerId { get; set; }
}
