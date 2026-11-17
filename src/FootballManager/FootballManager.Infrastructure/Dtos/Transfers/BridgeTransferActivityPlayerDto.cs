using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Transfers;

/// <summary>A player's row on the transfer activity screen, keyed by the game's property paths.</summary>
internal sealed class BridgeTransferActivityPlayerDto
{
    [JsonPropertyName(TransferActivityConstants.PlayerId)]
    public int? Id { get; set; }

    [JsonPropertyName(TransferActivityConstants.PlayerName)]
    public string? Name { get; set; }

    [JsonPropertyName(TransferActivityConstants.PlayerStatus)]
    public string? Status { get; set; }

    [JsonPropertyName(TransferActivityConstants.PlayerWage)]
    public string? Wage { get; set; }
}
