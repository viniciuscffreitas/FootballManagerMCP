namespace FootballManager.Domain.Transfers;

/// <summary>The counts on the game's transfer activity screen: players with offers, and offers that wait on the user.</summary>
public class TransferOfferCounts
{
    /// <summary>Players with an offer either way.</summary>
    public int All { get; set; }

    public int In { get; set; }
    public int Out { get; set; }

    /// <summary>Offers that need the user's answer.</summary>
    public int AwaitingResponseAll { get; set; }

    public int AwaitingResponseIn { get; set; }
    public int AwaitingResponseOut { get; set; }

    /// <summary>Player trade offers.</summary>
    public int Trades { get; set; }
}
