namespace FootballManager.Api.Responses.Transfers;

public class TransferActivityResponse
{
    public required TransferOfferCountsResponse Counts { get; set; }

    /// <summary>What the club's bids would spend, guaranteed then potential, e.g. "R$0 (R$0)".</summary>
    public string? IncomingValue { get; set; }

    /// <summary>What the offers for the club's players would bring in, guaranteed then potential.</summary>
    public string? OutgoingValue { get; set; }

    /// <summary>One per offer, those waiting on the user first; empty when nothing is under way.</summary>
    public List<TransferNegotiationResponse> Negotiations { get; set; } = [];
}
