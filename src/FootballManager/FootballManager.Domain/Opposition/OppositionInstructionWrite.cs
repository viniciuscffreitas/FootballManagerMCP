namespace FootballManager.Domain.Opposition;

/// <summary>One instruction to change on an opponent player, from its value now to the one asked.</summary>
public class OppositionInstructionWrite
{
    public required int PlayerId { get; set; }
    public string? Name { get; set; }
    public required OppositionInstruction Instruction { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }

    /// <summary>After an apply: whether the read back shows the new value; null when it couldn't be read.</summary>
    public bool? Confirmed { get; set; }
}
