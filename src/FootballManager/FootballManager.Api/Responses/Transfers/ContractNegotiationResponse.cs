using FootballManager.Domain.Transfers;

namespace FootballManager.Api.Responses.Transfers;

public class ContractNegotiationResponse
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public string? Club { get; set; }
    public bool Applied { get; set; }

    /// <summary>The inbox message the negotiation opens from, when the panel isn't open yet.</summary>
    public int? MessageId { get; set; }

    public string? MessageTitle { get; set; }

    /// <summary>The steps pressed in this call, in order.</summary>
    public List<ContractStep>? Steps { get; set; }

    /// <summary>The panel's lines as it last showed them: the salary and length once on the contract's page.</summary>
    public List<NegotiationTermResponse>? Terms { get; set; }

    public List<string>? Buttons { get; set; }

    /// <summary>What the agent said in this call.</summary>
    public List<string>? Response { get; set; }

    public bool Closed { get; set; }
    public List<string>? Notes { get; set; }
}
