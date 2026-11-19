using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.SetPieces;

/// <summary>An entry of a set piece list (takers, penalty takers, armband stand-ins): the player at that place.</summary>
internal sealed class BridgeSetPieceListedPlayerDto
{
    [JsonPropertyName(SetPieceConstants.PlayerId)]
    public int? Id { get; set; }

    [JsonPropertyName(SetPieceConstants.PlayerName)]
    public string? Name { get; set; }
}
