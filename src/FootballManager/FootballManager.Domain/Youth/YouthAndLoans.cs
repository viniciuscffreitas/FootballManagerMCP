namespace FootballManager.Domain.Youth;

/// <summary>The club's youth set-up and highest youth team, and its players out on loan.</summary>
public class YouthAndLoans
{
    public required YouthSetup Youth { get; set; }
    public List<YouthPlayer> YouthPlayers { get; set; } = [];
    public List<LoanedPlayer> Loans { get; set; } = [];

    /// <summary>The game's summary of the loans, e.g. "All our loaned players seem happy with how things are going".</summary>
    public string? LoanReport { get; set; }

    /// <summary>Players the staff suggest sending out on loan.</summary>
    public int? PlayersSuggestedForLoan { get; set; }
}
