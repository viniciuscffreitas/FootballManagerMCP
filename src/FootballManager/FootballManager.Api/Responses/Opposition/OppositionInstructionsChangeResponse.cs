using FootballManager.Domain.Opposition;

namespace FootballManager.Api.Responses.Opposition;

public class OppositionInstructionsChangeResponse
{
    public bool Applied { get; set; }
    public OppositionInstructionsAction Action { get; set; }
    public OppositionInstructionsResponse Current { get; set; } = new();
    public List<OppositionInstructionWriteResponse> Plan { get; set; } = [];
    public OppositionInstructionsResponse? After { get; set; }
    public List<string> Notes { get; set; } = [];
}
