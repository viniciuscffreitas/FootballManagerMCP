namespace FootballManager.Domain.Staff;

/// <summary>One of a staff member's coaching or medical attributes, as his profile shows it.</summary>
public class StaffAttribute
{
    /// <summary>In the game's display language, e.g. "Tático".</summary>
    public required string Name { get; set; }

    /// <summary>1 to 20.</summary>
    public int Value { get; set; }

    /// <summary>The game's word for the value, e.g. "Bom".</summary>
    public string? Level { get; set; }
}
