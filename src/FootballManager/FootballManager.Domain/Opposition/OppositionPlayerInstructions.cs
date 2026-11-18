using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Opposition;

/// <summary>
/// The instructions against one row of the opposition instructions screen: an opponent player of the lineup the game
/// expects, or a position (then no player). Each value is a key of <see cref="OppositionInstructionValues"/>.
/// </summary>
public class OppositionPlayerInstructions
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }

    /// <summary>Where the game expects him to play, e.g. "AMC" or "DCL".</summary>
    public TacticalPosition? Position { get; set; }

    public string Marking { get; set; } = OppositionInstructionValues.None;
    public string Pressing { get; set; } = OppositionInstructionValues.None;
    public string Tackling { get; set; } = OppositionInstructionValues.None;
    public string ShowOntoFoot { get; set; } = OppositionInstructionValues.None;

    public string Get(OppositionInstruction instruction)
    {
        return instruction switch
        {
            OppositionInstruction.Marking => Marking,
            OppositionInstruction.Pressing => Pressing,
            OppositionInstruction.Tackling => Tackling,
            _ => ShowOntoFoot
        };
    }
}
