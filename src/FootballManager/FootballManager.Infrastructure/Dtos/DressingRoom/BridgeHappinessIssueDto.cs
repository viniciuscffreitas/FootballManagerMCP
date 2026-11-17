using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.DressingRoom;

/// <summary>An open happiness issue, keyed by the game's property paths.</summary>
internal sealed class BridgeHappinessIssueDto
{
    [JsonPropertyName(SquadDynamicsConstants.IssuePlayerId)]
    public int? PlayerId { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.IssuePlayerName)]
    public string? PlayerName { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.IssueSeverity)]
    public string? Severity { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.IssueDescription)]
    public string? Description { get; set; }

    [JsonPropertyName(SquadDynamicsConstants.IssuePlayersAgreeing)]
    public int? PlayersAgreeing { get; set; }

    // Promise records; only their count is used.
    [JsonPropertyName(SquadDynamicsConstants.IssueLinkedPromises)]
    public List<JsonElement>? LinkedPromises { get; set; }
}
