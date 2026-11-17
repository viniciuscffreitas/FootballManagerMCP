namespace FootballManager.Domain.Transfers;

/// <summary>Which way a player would move, as the game's transfer activity screen splits its list.</summary>
public enum TransferDirection
{
    /// <summary>A player the user's club bids for.</summary>
    In,

    /// <summary>One of the club's players another club bids for.</summary>
    Out
}
