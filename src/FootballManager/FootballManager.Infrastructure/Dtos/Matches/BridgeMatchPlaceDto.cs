using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A place of the matchday squad in the match's tactic (context TeamSelectionIndex).</summary>
internal sealed class BridgeMatchPlaceDto
{
    [JsonPropertyName(TacticConstants.PositionValue)]
    public ulong? PositionMask { get; set; }

    [JsonPropertyName(TacticConstants.LineupPlayerId)]
    public int? PlayerId { get; set; }

    [JsonPropertyName(TacticConstants.LineupPlayerName)]
    public string? PlayerName { get; set; }
}
