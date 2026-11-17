namespace FootballManager.Domain.Transfers;

/// <summary>A contract negotiation with a player whose club accepted the user's offer, or what a dry run says it would do.</summary>
public class ContractNegotiation
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Club { get; set; }

    /// <summary>False for a dry run, which opens and presses nothing.</summary>
    public bool Applied { get; set; }

    /// <summary>The inbox message the negotiation opens from, when the panel isn't open yet.</summary>
    public int? MessageId { get; set; }

    public string? MessageTitle { get; set; }

    /// <summary>The steps pressed in this call, in order.</summary>
    public List<ContractStep> Steps { get; set; } = [];

    /// <summary>The panel's lines as it last showed them: the salary and length once on the contract's page.</summary>
    public List<NegotiationTerm> Terms { get; set; } = [];

    /// <summary>The panel's buttons as it last showed them.</summary>
    public List<string> Buttons { get; set; } = [];

    /// <summary>What the agent said in this call: the panel's new texts after each step.</summary>
    public List<string> Response { get; set; } = [];

    /// <summary>Whether the deal was offered at the panel's terms and the panel no longer asks for it.</summary>
    public bool Closed { get; set; }

    public List<string> Notes { get; set; } = [];
}
