using System.Text.Json.Serialization;
using FootballManager.Domain.Opposition;

namespace FootballManager.Api.Responses.Opposition;

public class OppositionInstructionWriteResponse
{
    public int PlayerId { get; set; }
    public string? Name { get; set; }
    public OppositionInstruction Instruction { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;

    /// <summary>After an apply, whether the read back shows the new value; null in a dry run or without a read back.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Confirmed { get; set; }
}
