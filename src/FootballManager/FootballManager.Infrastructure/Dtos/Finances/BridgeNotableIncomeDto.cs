namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>A main source of income, as texts in the screens' currency.</summary>
internal sealed class BridgeNotableIncomeDto
{
    public string? Name { get; set; }
    public string? ThisMonth { get; set; }
    public string? ThisSeason { get; set; }
    public string? LastSeason { get; set; }
}
