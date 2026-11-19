namespace FootballManager.Api.Responses.Transfers;

public class PlayerExitStatusResponse
{
    public bool TransferListed { get; set; }
    public bool LoanListed { get; set; }
    public bool OutOnLoan { get; set; }
}
