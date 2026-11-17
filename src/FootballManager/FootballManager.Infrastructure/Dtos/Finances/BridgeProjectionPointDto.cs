namespace FootballManager.Infrastructure.Dtos.Finances;

/// <summary>A projected amount in pounds at a season's end, with the game's title for it ("Fim da temporada 2026").</summary>
internal sealed class BridgeProjectionPointDto
{
    public string? Title { get; set; }
    public long? PropertyValue { get; set; }
}
