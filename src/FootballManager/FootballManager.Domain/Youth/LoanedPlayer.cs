namespace FootballManager.Domain.Youth;

/// <summary>A player of the club out on loan, with the game's loan report on him.</summary>
public class LoanedPlayer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }

    /// <summary>The club he plays for on loan.</summary>
    public string? Club { get; set; }

    /// <summary>When the loan ends (yyyy-MM-dd).</summary>
    public string? LoanEndDate { get; set; }

    /// <summary>The monthly fee the loan club pays to play him, as the game shows it, e.g. "R$0".</summary>
    public string? MonthlyFee { get; set; }

    /// <summary>Matches played at the loan club.</summary>
    public int? Appearances { get; set; }

    /// <summary>Average rating at the loan club; null without appearances.</summary>
    public double? AverageRating { get; set; }

    /// <summary>How happy he is with the loan, 0 to 100.</summary>
    public int? Happiness { get; set; }

    /// <summary>The loan report's comments, e.g. "Playing regularly enough for us to be satisfied".</summary>
    public List<string> Comments { get; set; } = [];
}
