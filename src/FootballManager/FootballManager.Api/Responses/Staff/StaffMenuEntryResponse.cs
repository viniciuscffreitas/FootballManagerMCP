namespace FootballManager.Api.Responses.Staff;

public class StaffMenuEntryResponse
{
    public required string Label { get; set; }

    /// <summary>The game greys the entry out.</summary>
    public bool Disabled { get; set; }

    public string? Confirmation { get; set; }
    public List<StaffMenuEntryResponse> Children { get; set; } = [];
}
