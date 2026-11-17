namespace FootballManager.Infrastructure.Dtos.Youth;

/// <summary>A facility rating as the game sends it: out of 20, with its short and long texts.</summary>
internal sealed class BridgeFacilityRatingDto
{
    public int? Rating { get; set; }
    public string? Description { get; set; }
    public string? DescriptionShort { get; set; }
}
