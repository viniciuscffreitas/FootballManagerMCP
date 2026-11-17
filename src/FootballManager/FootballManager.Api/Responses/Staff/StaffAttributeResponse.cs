namespace FootballManager.Api.Responses.Staff;

public class StaffAttributeResponse
{
    public required string Name { get; set; }

    /// <summary>1 to 20.</summary>
    public int Value { get; set; }

    /// <summary>The game's word for the value, e.g. "Bom".</summary>
    public string? Level { get; set; }
}
