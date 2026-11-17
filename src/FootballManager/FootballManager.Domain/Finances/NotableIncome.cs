namespace FootballManager.Domain.Finances;

/// <summary>One of the club's main sources of income over the month, the season and last season.</summary>
public class NotableIncome
{
    public required string Name { get; set; }
    public string? ThisMonth { get; set; }
    public string? ThisSeason { get; set; }
    public string? LastSeason { get; set; }
}
