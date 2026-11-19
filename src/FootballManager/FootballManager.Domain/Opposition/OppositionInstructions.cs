namespace FootballManager.Domain.Opposition;

/// <summary>
/// The instructions against the user's next opponent, as the opposition instructions screen shows them: one row per
/// player of the lineup the game expects, one per position, and what the staff suggests.
/// </summary>
public class OppositionInstructions
{
    public required int TeamId { get; set; }
    public required string TeamName { get; set; }
    public List<OppositionPlayerInstructions> Players { get; set; } = [];
    public List<OppositionPlayerInstructions> Positions { get; set; } = [];
    public List<OppositionStaffSuggestion> StaffSuggestions { get; set; } = [];
}
