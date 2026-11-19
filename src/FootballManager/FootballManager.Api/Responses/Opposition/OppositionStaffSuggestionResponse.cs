using FootballManager.Domain.Opposition;

namespace FootballManager.Api.Responses.Opposition;

public class OppositionStaffSuggestionResponse
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }

    /// <summary>Null when the suggestion's kind isn't mapped yet: then ApplyStaffAdvice leaves it out.</summary>
    public OppositionInstruction? Instruction { get; set; }

    public string? Value { get; set; }
    public bool IsDisabled { get; set; }
}
