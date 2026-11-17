namespace FootballManager.Domain.Transfers;

/// <summary>The game's contract negotiation panel as it shows now (texts in the game's display language).</summary>
public class ContractPanel
{
    /// <summary>The player the contract is for, as the panel names him, e.g. "(Walter)".</summary>
    public string? Player { get; set; }

    public string? Club { get; set; }

    /// <summary>The panel's lines: the promises on the first page, the salary, length and the rest on the contract's.</summary>
    public List<NegotiationTerm> Terms { get; set; } = [];

    /// <summary>The steps the panel's buttons offer now.</summary>
    public List<ContractStep> Steps { get; set; } = [];

    /// <summary>The texts of the panel's own buttons.</summary>
    public List<string> Buttons { get; set; } = [];

    /// <summary>What the player's agent says beside the panel.</summary>
    public List<string> Lines { get; set; } = [];

    /// <summary>Every text the panel shows, to tell what changed after a step.</summary>
    public List<string> Texts { get; set; } = [];
}
