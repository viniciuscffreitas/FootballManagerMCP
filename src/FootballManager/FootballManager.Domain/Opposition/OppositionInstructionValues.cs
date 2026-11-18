namespace FootballManager.Domain.Opposition;

/// <summary>The values each opposition instruction takes, as keys; "none" is the game's "-" (no instruction).</summary>
public static class OppositionInstructionValues
{
    public const string None = "none";

    public static IReadOnlyList<string> Of(OppositionInstruction instruction)
    {
        return instruction switch
        {
            OppositionInstruction.Marking or OppositionInstruction.Pressing => [None, "never", "always"],
            OppositionInstruction.Tackling => [None, "normal", "soft", "hard"],
            OppositionInstruction.ShowOntoFoot => [None, "left", "right", "weaker"],
            _ => throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null)
        };
    }
}
