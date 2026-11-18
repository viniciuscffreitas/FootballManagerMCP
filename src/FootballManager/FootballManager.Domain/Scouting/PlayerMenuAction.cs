namespace FootballManager.Domain.Scouting;

/// <summary>The entries of a player's menu that recruitment uses (the game's own menu actions).</summary>
public enum PlayerMenuAction
{
    /// <summary>Have a scout, or any scout, watch the player for a while.</summary>
    AssignScout,
    AddToShortlist,
    RemoveFromShortlist,

    /// <summary>Offer the player a trial at the club, for one to four weeks.</summary>
    OfferTrial,

    /// <summary>Withdraw an offer made to the player that he hasn't answered yet, a trial offer among them.</summary>
    WithdrawOffer,

    /// <summary>End the trial of a player on trial at the club: he leaves it.</summary>
    EndTrial,

    /// <summary>Make a transfer offer for the player: opens the game's transfer offer panel.</summary>
    MakeOffer,

    /// <summary>Start a contract negotiation with one of the club's own players: opens the contract negotiation panel.</summary>
    NegotiateContract,

    /// <summary>Mark a player of another club as a transfer target of the director of football.</summary>
    MarkDofTarget,

    /// <summary>Take a player off the director of football's transfer targets.</summary>
    UnmarkDofTarget
}
