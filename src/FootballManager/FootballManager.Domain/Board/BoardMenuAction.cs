namespace FootballManager.Domain.Board;

/// <summary>The game's menu actions for asking the board for something, all on the club.</summary>
public enum BoardMenuAction
{
    /// <summary>"Pedido à Diretoria" (MakeBoardRequest): opens the talk with the board, whose lines are the requests (facilities, youth, staff, ...).</summary>
    Request,

    /// <summary>Ask for a bigger wage or transfer budget (ClubRequestIncreaseBudgets): one entry each.</summary>
    IncreaseBudgets,

    /// <summary>Ask the board for a new contract for the user (ClubRequestManagerNewContract).</summary>
    ManagerNewContract,

    /// <summary>Ask for a new affiliate club (ClubRequestAffiliateLink).</summary>
    AffiliateLink,

    /// <summary>Ask for a senior affiliate (ClubRequestSeniorAffiliateLink).</summary>
    SeniorAffiliateLink
}
