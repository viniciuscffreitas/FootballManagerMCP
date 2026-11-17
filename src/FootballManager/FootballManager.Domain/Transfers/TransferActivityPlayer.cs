namespace FootballManager.Domain.Transfers;

/// <summary>A player on the game's transfer activity screen, with the offers made for him.</summary>
public class TransferActivityPlayer
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public TransferDirection Direction { get; set; }

    /// <summary>Where his move stands, as the screen's row shows it.</summary>
    public string? Status { get; set; }

    /// <summary>The wage the deal carries, as the game shows it.</summary>
    public string? Wage { get; set; }

    public List<TransferActivityOffer> Offers { get; set; } = [];
}
