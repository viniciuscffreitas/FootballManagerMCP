using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Messages;

/// <summary>A message of the portal's list with the answers it offers.</summary>
internal sealed class BridgeMessageOptionsDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(InboxConstants.Title)]
    public string? Title { get; set; }

    [JsonPropertyName(InboxConstants.CanReply)]
    public bool? CanReply { get; set; }

    [JsonPropertyName(InboxConstants.HasReplied)]
    public bool? HasReplied { get; set; }

    [JsonPropertyName(InboxConstants.Actions)]
    public BridgeSpeakToActionsDto? Actions { get; set; }
}
