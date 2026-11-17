using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Transfers;

/// <summary>The transfer activity screen's counts and values, keyed by the game's property paths.</summary>
internal sealed class BridgeTransferActivityDto
{
    [JsonPropertyName(TransferActivityConstants.OffersAll)]
    public int? OffersAll { get; set; }

    [JsonPropertyName(TransferActivityConstants.OffersIn)]
    public int? OffersIn { get; set; }

    [JsonPropertyName(TransferActivityConstants.OffersOut)]
    public int? OffersOut { get; set; }

    [JsonPropertyName(TransferActivityConstants.AwaitingResponseAll)]
    public int? AwaitingResponseAll { get; set; }

    [JsonPropertyName(TransferActivityConstants.AwaitingResponseIn)]
    public int? AwaitingResponseIn { get; set; }

    [JsonPropertyName(TransferActivityConstants.AwaitingResponseOut)]
    public int? AwaitingResponseOut { get; set; }

    [JsonPropertyName(TransferActivityConstants.Trades)]
    public int? Trades { get; set; }

    [JsonPropertyName(TransferActivityConstants.IncomingValue)]
    public string? IncomingValue { get; set; }

    [JsonPropertyName(TransferActivityConstants.OutgoingValue)]
    public string? OutgoingValue { get; set; }
}
