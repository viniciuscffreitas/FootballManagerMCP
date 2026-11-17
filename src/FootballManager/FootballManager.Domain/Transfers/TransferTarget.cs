namespace FootballManager.Domain.Transfers;

/// <summary>A player of another club the user may bid for, as the game shows him.</summary>
public class TransferTarget
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Club { get; set; }

    /// <summary>Whether his menu offers a transfer offer now (not for the club's own players, say).</summary>
    public bool CanMakeOffer { get; set; }
}
