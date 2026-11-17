namespace FootballManager.Domain.Transfers;

/// <summary>One offer for a player on the game's transfer activity screen. Texts as the game shows them, money in R$.</summary>
public class TransferActivityOffer
{
    public string? BuyingClub { get; set; }
    public string? SellingClub { get; set; }

    /// <summary>The kind of deal, e.g. a transfer or a loan.</summary>
    public string? Type { get; set; }

    /// <summary>The fee as the screen's table shows it, e.g. "R$1,5 mi".</summary>
    public string? Fee { get; set; }

    public string? Status { get; set; }

    /// <summary>Whether the offer waits on the user's answer.</summary>
    public bool RequiresResponse { get; set; }
}
