namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>One kind of player's weekly wages: texts in the screens' currency, TotalBasicWageValue in pounds.</summary>
internal sealed class BridgeWageGroupDto
{
    public string? Name { get; set; }
    public int? Quantity { get; set; }
    public string? TotalBasicWage { get; set; }
    public long? TotalBasicWageValue { get; set; }
    public string? AverageBasicWage { get; set; }
    public string? HighestBasicWage { get; set; }
    public string? LowestBasicWage { get; set; }
    public string? AdvisedMaxBasicWage { get; set; }
    public bool? IsOverWages { get; set; }
}
