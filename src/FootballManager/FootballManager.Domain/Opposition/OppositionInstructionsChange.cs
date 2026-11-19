namespace FootballManager.Domain.Opposition;

/// <summary>
/// A change to the instructions against the next opponent: the instructions as they are, the writes planned, and after
/// an apply the instructions read back.
/// </summary>
public class OppositionInstructionsChange
{
    public required OppositionInstructionsAction Action { get; set; }
    public required OppositionInstructions Current { get; set; }
    public List<OppositionInstructionWrite> Plan { get; set; } = [];
    public bool Applied { get; set; }

    /// <summary>The instructions read after the apply; null in a dry run or when the read back failed.</summary>
    public OppositionInstructions? After { get; set; }

    public List<string> Notes { get; set; } = [];
}
