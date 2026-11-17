namespace FootballManager.Domain.Youth;

/// <summary>A player of the club's highest youth team, which keeps its players while they're out on loan.</summary>
public class YouthPlayer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Positions { get; set; }

    /// <summary>His morale as the game words it, e.g. "Very Good".</summary>
    public string? Morale { get; set; }

    /// <summary>When his contract with the club ends (yyyy-MM-dd).</summary>
    public string? ContractEndDate { get; set; }

    public bool OnLoan { get; set; }

    /// <summary>The club he's on loan at; null when he isn't.</summary>
    public string? LoanClub { get; set; }
}
