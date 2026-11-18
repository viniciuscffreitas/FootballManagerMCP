namespace FootballManager.Domain.Transfers;

/// <summary>A player the user may bid for or offer a contract, as the game shows him.</summary>
public class TransferTarget
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Club { get; set; }

    /// <summary>Whether his menu offers a transfer offer now (not for the club's own players, say).</summary>
    public bool CanMakeOffer { get; set; }

    /// <summary>Whether his menu offers a contract negotiation now (one of the club's own players).</summary>
    public bool CanNegotiateContract { get; set; }
}
