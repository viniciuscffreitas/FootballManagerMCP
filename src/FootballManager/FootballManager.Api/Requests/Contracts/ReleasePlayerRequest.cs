using FootballManager.Domain.Contracts;

namespace FootballManager.Api.Requests.Contracts;

public class ReleasePlayerRequest
{
    /// <summary>FM id of one of the club's own players.</summary>
    public int PlayerId { get; set; }

    /// <summary>Free (released now, contract paid off), Mutual (ended by mutual consent) or EndOfContract (leaves when it ends).</summary>
    public ReleaseKind How { get; set; }
}
