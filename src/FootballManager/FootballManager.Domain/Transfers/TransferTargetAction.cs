namespace FootballManager.Domain.Transfers;

/// <summary>
/// What the user can do about a player of another club the club is after, each through the player's menu in the game.
/// No action has the value 0, so an action left out of a request is never read as one.
/// </summary>
public enum TransferTargetAction
{
    /// <summary>Withdraw the transfer offer made for him that his club hasn't answered yet.</summary>
    WithdrawOffer = 1,

    /// <summary>Mark him as a transfer target of the director of football.</summary>
    MarkDofTarget = 2,

    /// <summary>Take him off the director of football's transfer targets.</summary>
    UnmarkDofTarget = 3
}
