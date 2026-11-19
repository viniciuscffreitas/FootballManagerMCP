using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Opposition;

/// <summary>The opposition instructions screen as read on the active tactic.</summary>
internal sealed class BridgeOppositionInstructionsDto
{
    [JsonPropertyName(OppositionInstructionConstants.TeamId)]
    public int? TeamId { get; set; }

    [JsonPropertyName(OppositionInstructionConstants.TeamName)]
    public string? TeamName { get; set; }

    [JsonPropertyName(OppositionInstructionConstants.Instructions)]
    public BridgeOppositionRowsDto? Instructions { get; set; }

    [JsonPropertyName(OppositionInstructionConstants.Suggestions)]
    public List<BridgeOppositionSuggestionDto>? Suggestions { get; set; }
}
