namespace FootballManager.Api.Responses.Finances;

public class WageGroupResponse
{
    public required string Name { get; set; }
    public int Players { get; set; }
    public string? TotalWages { get; set; }
    public string? AverageWage { get; set; }
    public string? HighestWage { get; set; }
    public string? LowestWage { get; set; }
    public string? AdvisedMaxWage { get; set; }
    public bool IsOverWages { get; set; }
}
