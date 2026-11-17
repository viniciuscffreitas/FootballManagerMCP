namespace FootballManager.Api.Responses.Youth;

public class YouthAndLoansResponse
{
    public required YouthSetupResponse Youth { get; set; }

    /// <summary>Every player of the highest youth team, those out on loan marked.</summary>
    public List<YouthPlayerResponse> YouthPlayers { get; set; } = [];

    /// <summary>Every player of the club out on loan.</summary>
    public List<LoanedPlayerResponse> Loans { get; set; } = [];

    public string? LoanReport { get; set; }
    public int? PlayersSuggestedForLoan { get; set; }
}
