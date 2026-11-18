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

    /// <summary>
    /// Needed to apply: the dry run's confirmation (its cost) as the user agreed to it. The release is sent only while
    /// the game's menu still shows it (case and spaces aside); otherwise it's a 400 and nothing is sent.
    /// </summary>
    public string? ExpectedConfirmation { get; set; }
}
