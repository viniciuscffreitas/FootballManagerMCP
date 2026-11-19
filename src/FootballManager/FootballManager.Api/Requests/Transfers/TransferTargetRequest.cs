using FootballManager.Domain.Transfers;

namespace FootballManager.Api.Requests.Transfers;

public class TransferTargetRequest
{
    /// <summary>The FM id of a player of another club; required.</summary>
    public int? PlayerId { get; set; }

    /// <summary>WithdrawOffer, MarkDofTarget or UnmarkDofTarget; required, with no default.</summary>
    public TransferTargetAction? Action { get; set; }
}
