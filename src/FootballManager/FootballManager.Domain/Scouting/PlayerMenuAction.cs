namespace FootballManager.Domain.Scouting;

/// <summary>The entries of a player's menu that recruitment and selling use (the game's own menu actions).</summary>
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

    /// <summary>Set one of the club's own players' transfer status: its entries are the statuses.</summary>
    SetTransferStatus,

    /// <summary>Take one of the club's own players off the transfer list.</summary>
    RemoveFromTransferList,

    /// <summary>Take one of the club's own players off the loan list.</summary>
    RemoveFromLoanList,

    /// <summary>Offer one of the club's own players to other clubs.</summary>
    OfferToClubs,

    /// <summary>Call back a player the club has out on loan.</summary>
    RecallFromLoan,

    /// <summary>Offer to extend the loan of a player the club has out on loan.</summary>
    ExtendLoan
}
