namespace FootballManager.Domain.Registration;

/// <summary>A squad number change as planned or sent: one player's number, or the assistant numbering everyone.</summary>
public class SquadNumberChange
{
    public bool Applied { get; set; }

    /// <summary>After an apply: whether the read back matches the request; null when it couldn't be read.</summary>
    public bool? Confirmed { get; set; }

    public bool AskAssistant { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int? From { get; set; }
    public int? To { get; set; }

    /// <summary>For the assistant: how many first-team players still need a number, before and (after an apply) after.</summary>
    public int? NeedingNumbersBefore { get; set; }

    public int? NeedingNumbersAfter { get; set; }

    public RegistrationWrite Write { get; set; }

    public List<string> Warnings { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
