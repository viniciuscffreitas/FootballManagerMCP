namespace FootballManager.Api.Responses.Opposition;

public class OppositionInstructionsResponse
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public List<OppositionPlayerInstructionsResponse> Players { get; set; } = [];
    public List<OppositionPlayerInstructionsResponse> Positions { get; set; } = [];
    public List<OppositionStaffSuggestionResponse> StaffSuggestions { get; set; } = [];
}
