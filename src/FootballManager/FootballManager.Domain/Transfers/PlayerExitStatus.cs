namespace FootballManager.Domain.Transfers;

/// <summary>Where a player stands for leaving the club, as his menu in the game shows it.</summary>
public class PlayerExitStatus
{
    /// <summary>He's on the transfer list: the menu offers to take him off it.</summary>
    public bool TransferListed { get; set; }

    /// <summary>He's on the loan list: the menu offers to take him off it.</summary>
    public bool LoanListed { get; set; }

    /// <summary>He's out on loan at another club: the menu offers to call him back.</summary>
    public bool OutOnLoan { get; set; }
}
