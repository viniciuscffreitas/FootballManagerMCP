namespace FootballManager.Domain.Transfers;

/// <summary>What the game's transfer activity screen holds: its counts, the deals' values, and every player with an offer.</summary>
public class TransferActivityReadout
{
    public TransferOfferCounts Counts { get; set; } = new();

    /// <summary>What the club's bids would spend, guaranteed then potential, e.g. "R$0 (R$0)".</summary>
    public string? IncomingValue { get; set; }

    /// <summary>What the offers for the club's players would bring in, guaranteed then potential.</summary>
    public string? OutgoingValue { get; set; }

    public List<TransferActivityPlayer> Players { get; set; } = [];
}
