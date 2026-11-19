namespace FootballManager.Api.Responses.Opposition;

public class OppositionPlayerInstructionsResponse
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }

    /// <summary>FM's position code with the side for central slots, e.g. "DCL", as get_tactics gives it.</summary>
    public string? Position { get; set; }

    public string Marking { get; set; } = string.Empty;
    public string Pressing { get; set; } = string.Empty;
    public string Tackling { get; set; } = string.Empty;
    public string ShowOntoFoot { get; set; } = string.Empty;
}
