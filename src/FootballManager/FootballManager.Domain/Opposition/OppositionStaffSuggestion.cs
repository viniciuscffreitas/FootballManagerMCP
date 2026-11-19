namespace FootballManager.Domain.Opposition;

/// <summary>
/// One instruction the staff suggests against an opponent player, with its line in the game's language. Instruction and
/// Value are null when the game's suggestion is of a kind not mapped yet: it is listed, never applied.
/// </summary>
public class OppositionStaffSuggestion
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public OppositionInstruction? Instruction { get; set; }
    public string? Value { get; set; }

    /// <summary>The game greys the suggestion out (e.g. already given).</summary>
    public bool IsDisabled { get; set; }
}
