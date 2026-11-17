namespace FootballManager.Domain.Transfers;

/// <summary>A transfer under way: one offer for one player, either way. Texts as the game shows them, money in R$.</summary>
public class TransferNegotiation
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public TransferDirection Direction { get; set; }

    /// <summary>The other club of the deal: the seller of a player the user bids for, the buyer of one of his own.</summary>
    public string? Club { get; set; }

    public string? Type { get; set; }
    public string? Fee { get; set; }
    public string? Wage { get; set; }
    public string? Status { get; set; }

    /// <summary>Whether it waits on the user's answer.</summary>
    public bool RequiresResponse { get; set; }
}
