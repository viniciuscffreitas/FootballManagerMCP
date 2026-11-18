namespace FootballManager.Domain.Transfers;

/// <summary>A change to how the club pursues a player of another club, planned from his menu in the game, and sent or not.</summary>
public class TransferTargetChange
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Club { get; set; }
    public TransferTargetAction Action { get; set; }

    /// <summary>The menu entry run, labelled as the game labels each level.</summary>
    public string? Choice { get; set; }

    /// <summary>What the game asks the user to confirm before running the entry, when it asks.</summary>
    public string? Confirmation { get; set; }

    /// <summary>The write was sent to the game (apply), whatever came of it.</summary>
    public bool Applied { get; set; }

    public WriteState Write { get; set; }

    /// <summary>The menu read back shows the change; null when it couldn't be read back or nothing was sent.</summary>
    public bool? Confirmed { get; set; }

    /// <summary>An offer the club made for him waits for an answer: his menu offers to withdraw it.</summary>
    public bool OfferPending { get; set; }

    /// <summary>He is one of the director of football's transfer targets: his menu offers to take him off them.</summary>
    public bool IsDofTarget { get; set; }

    public List<string> Notes { get; set; } = [];
}
