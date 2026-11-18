namespace FootballManager.Domain.Transfers;

/// <summary>A change to a player's way out of the club, planned from his menu in the game, and sent when applied.</summary>
public class PlayerExit
{
    /// <summary>NotWritten for a dry run; Unknown when the game's answer was lost and the read back can't tell.</summary>
    public PlayerExitWrite Applied { get; set; }

    /// <summary>
    /// Whether the read back after the send shows him where the entry puts him; null for a dry run, an entry whose
    /// effect his menu doesn't show (an offer, a loan extension), or a read back that failed.
    /// </summary>
    public bool? Confirmed { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerExitAction Action { get; set; }

    /// <summary>The menu entry picked, as the game labels each level ("Transfer status > Transfer list").</summary>
    public string? Choice { get; set; }

    /// <summary>Every entry the game's menu offers for the action, labelled the same way, for the user to pick from.</summary>
    public List<string> Choices { get; set; } = [];

    /// <summary>What the game asks the user to confirm before running the entry, when it asks.</summary>
    public string? Confirmation { get; set; }

    /// <summary>Where the player stands: before the change on a dry run, as the game shows him after it once applied.</summary>
    public PlayerExitStatus Status { get; set; } = new();

    public List<string> Notes { get; set; } = [];
}
