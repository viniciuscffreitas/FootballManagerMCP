namespace FootballManager.Api.Responses.Finances;

public class FinanceLineResponse
{
    public required string Name { get; set; }
    public string? Amount { get; set; }
    public int? Percentage { get; set; }
    public int? ChangeFromLastMonth { get; set; }
}
