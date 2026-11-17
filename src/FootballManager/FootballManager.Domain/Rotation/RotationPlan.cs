using FootballManager.Domain.Availability;
using FootballManager.Domain.Tactics.Editing;

namespace FootballManager.Domain.Rotation;

/// <summary>A rotation of the starting eleven for the next match: who goes out, who comes in, and why.</summary>
public class RotationPlan
{
    /// <summary>The active tactic slot, 1 to 3, whose eleven is rotated.</summary>
    public int Slot { get; set; }

    /// <summary>False for a dry run, which only plans.</summary>
    public bool Applied { get; set; }

    public required NextMatchRules NextMatch { get; set; }

    /// <summary>Starters going out with the reserve coming in for each, by slot index.</summary>
    public List<RotationSwap> Swaps { get; set; } = [];

    /// <summary>Starters who should go out but no reserve qualifies for, so they stay.</summary>
    public List<RotationSwap> Unfilled { get; set; } = [];

    public List<string> Notes { get; set; } = [];

    /// <summary>The lineup edit through set_tactic's path, planned or applied; null when nobody changes.</summary>
    public TacticEdit? Edit { get; set; }
}
