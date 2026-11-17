using FootballManager.Api.Responses.Availability;
using FootballManager.Api.Responses.Tactics.Editing;

namespace FootballManager.Api.Responses.Rotation;

public class RotationPlanResponse
{
    /// <summary>The active tactic slot whose eleven is rotated.</summary>
    public int Slot { get; set; }

    /// <summary>False for a dry run: nothing was written.</summary>
    public bool Applied { get; set; }

    public required NextMatchRulesResponse NextMatch { get; set; }
    public List<RotationSwapResponse> Swaps { get; set; } = [];

    /// <summary>Starters who should go out but stay, as no reserve qualifies.</summary>
    public List<RotationSwapResponse> Unfilled { get; set; } = [];

    public List<string> Notes { get; set; } = [];

    /// <summary>set_tactic's steps for the swaps, planned or applied; null when nobody changes.</summary>
    public TacticEditResponse? Edit { get; set; }
}
