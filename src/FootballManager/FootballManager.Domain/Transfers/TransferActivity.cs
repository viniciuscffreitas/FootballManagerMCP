namespace FootballManager.Domain.Transfers;

/// <summary>The club's transfers under way and the offers it has received, as the game's transfer activity screen shows them.</summary>
public class TransferActivity
{
    public TransferOfferCounts Counts { get; set; } = new();

    /// <summary>What the club's bids would spend, guaranteed then potential, e.g. "R$0 (R$0)".</summary>
    public string? IncomingValue { get; set; }

    /// <summary>What the offers for the club's players would bring in, guaranteed then potential.</summary>
    public string? OutgoingValue { get; set; }

    /// <summary>One per offer, those waiting on the user first.</summary>
    public List<TransferNegotiation> Negotiations { get; set; } = [];
}
