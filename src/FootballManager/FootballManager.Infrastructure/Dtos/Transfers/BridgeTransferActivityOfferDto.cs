using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Transfers;

/// <summary>One offer for a player on the transfer activity screen, keyed by the game's property paths.</summary>
internal sealed class BridgeTransferActivityOfferDto
{
    [JsonPropertyName(TransferActivityConstants.BuyingClub)]
    public string? BuyingClub { get; set; }

    [JsonPropertyName(TransferActivityConstants.SellingClub)]
    public string? SellingClub { get; set; }

    [JsonPropertyName(TransferActivityConstants.OfferType)]
    public string? Type { get; set; }

    [JsonPropertyName(TransferActivityConstants.OfferFee)]
    public string? Fee { get; set; }

    [JsonPropertyName(TransferActivityConstants.OfferStatus)]
    public string? Status { get; set; }

    [JsonPropertyName(TransferActivityConstants.OfferRequiresResponse)]
    public bool? RequiresResponse { get; set; }
}
