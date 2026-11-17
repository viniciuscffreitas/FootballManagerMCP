namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>A row of the month's income or expenditure; DisplayValue is the screens' text, PropertyValue in pounds.</summary>
internal sealed class BridgeFinanceLineDto
{
    public string? Name { get; set; }
    public string? DisplayValue { get; set; }
    public int? Percentage { get; set; }
    public int? MonthToMonthPercentage { get; set; }
}
