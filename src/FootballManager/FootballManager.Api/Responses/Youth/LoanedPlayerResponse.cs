namespace FootballManager.Api.Responses.Youth;

public class LoanedPlayerResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Club { get; set; }
    public string? LoanEndDate { get; set; }

    /// <summary>As the game shows it, e.g. "R$0".</summary>
    public string? MonthlyFee { get; set; }

    public int? Appearances { get; set; }
    public double? AverageRating { get; set; }

    /// <summary>0 to 100.</summary>
    public int? Happiness { get; set; }

    public List<string>? Comments { get; set; }
}
