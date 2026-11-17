namespace FootballManager.Api.Responses.Finances;

public class NotableIncomeResponse
{
    public required string Name { get; set; }
    public string? ThisMonth { get; set; }
    public string? ThisSeason { get; set; }
    public string? LastSeason { get; set; }
}
