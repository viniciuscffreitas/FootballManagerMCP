namespace FootballManager.Domain.Transfers;

/// <summary>A transfer offer for a player, or what a dry run says it would send.</summary>
public class TransferOffer
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Club { get; set; }

    /// <summary>The fee asked for, in the game's currency units.</summary>
    public long Fee { get; set; }

    /// <summary>False for a dry run, which opens nothing.</summary>
    public bool Applied { get; set; }

    /// <summary>The fee the panel started at when it opened, before the buttons moved it.</summary>
    public long? StartingFee { get; set; }

    /// <summary>What the director of football advised beside the offer, his suggested fee among it.</summary>
    public List<string> Advice { get; set; } = [];

    /// <summary>Whether the offer was sent to the club.</summary>
    public bool Sent { get; set; }

    /// <summary>The texts the panel showed after the offer went, which it didn't before.</summary>
    public List<string> Response { get; set; } = [];

    public List<string> Notes { get; set; } = [];
}
