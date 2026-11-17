namespace FootballManager.Api.Responses.Availability;

public class InjuredPlayerResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public string? Injury { get; set; }

    /// <summary>The game's estimate, e.g. "Expected back in 8 days - 2 weeks".</summary>
    public string? ExpectedReturn { get; set; }

    public int? ReturnDays { get; set; }
}
