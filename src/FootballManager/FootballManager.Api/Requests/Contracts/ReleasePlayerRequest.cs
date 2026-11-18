using FootballManager.Domain.Contracts;

namespace FootballManager.Api.Requests.Contracts;

public class ReleasePlayerRequest
{
    /// <summary>FM id of one of the club's own players. Required: left out, it's a 400, never player 0.</summary>
    public int? PlayerId { get; set; }

    /// <summary>
    /// Free (released now, contract paid off), Mutual (ended by mutual consent) or EndOfContract (leaves when it ends).
    /// Required: left out, it's a 400, never Free.
    /// </summary>
    public ReleaseKind? How { get; set; }
}
