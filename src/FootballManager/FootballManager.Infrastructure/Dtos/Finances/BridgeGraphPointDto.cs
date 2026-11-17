namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>A point of a finance graph: a date and an amount in pounds.</summary>
internal sealed class BridgeGraphPointDto
{
    public string? GraphPointX { get; set; }
    public long? GraphPointY { get; set; }
}
