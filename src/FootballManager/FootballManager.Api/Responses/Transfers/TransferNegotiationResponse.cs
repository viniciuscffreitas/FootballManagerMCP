using FootballManager.Domain.Transfers;

namespace FootballManager.Api.Responses.Transfers;

public class TransferNegotiationResponse
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public TransferDirection Direction { get; set; }

    /// <summary>The other club of the deal.</summary>
    public string? Club { get; set; }

    public string? Type { get; set; }
    public string? Fee { get; set; }
    public string? Wage { get; set; }
    public string? Status { get; set; }
    public bool RequiresResponse { get; set; }
}
