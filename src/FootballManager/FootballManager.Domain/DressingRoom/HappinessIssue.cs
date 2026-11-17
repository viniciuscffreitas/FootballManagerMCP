namespace FootballManager.Domain.DressingRoom;

/// <summary>An open happiness issue: who raised it, how serious it is, and how many teammates back him.</summary>
public class HappinessIssue
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }

    /// <summary>How serious, as the game words it, e.g. "Preocupado".</summary>
    public string? Severity { get; set; }

    public string? Description { get; set; }
    public int PlayersAgreeing { get; set; }

    /// <summary>Whether a promise the manager made is tied to this issue.</summary>
    public bool HasPromises { get; set; }
}
