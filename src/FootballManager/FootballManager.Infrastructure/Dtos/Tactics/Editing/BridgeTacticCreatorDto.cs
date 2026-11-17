using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>The step the tactic creator is on: 0 style, 1 formation with the ball, 2 without it.</summary>
internal sealed class BridgeTacticCreatorDto
{
    [JsonPropertyName(TacticConstants.CreatorStage)]
    public int? Stage { get; set; }
}
