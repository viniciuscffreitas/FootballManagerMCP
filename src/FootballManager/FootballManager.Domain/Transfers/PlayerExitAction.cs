namespace FootballManager.Domain.Transfers;

/// <summary>What the club can do about one of its own players leaving, each through the player's menu in the game.</summary>
public enum PlayerExitAction
{
    /// <summary>Set his transfer status: the game's menu lists the statuses (transfer list, loan list and so on).</summary>
    SetTransferStatus,

    /// <summary>Take him off the transfer list.</summary>
    RemoveFromTransferList,

    /// <summary>Take him off the loan list.</summary>
    RemoveFromLoanList,

    /// <summary>Offer him to other clubs, for a transfer or a loan, as the game's menu lists them.</summary>
    OfferToClubs,

    /// <summary>Call back a player the club has out on loan.</summary>
    Recall,

    /// <summary>Offer the club he's on loan at to extend his loan.</summary>
    ExtendLoan
}
