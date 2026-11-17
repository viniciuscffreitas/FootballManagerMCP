using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Messages;

/// <summary>A story of the portal's club or league news, as the game lists it.</summary>
internal sealed class BridgeNewsStoryDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(InboxConstants.Date)]
    public string? Date { get; set; }

    [JsonPropertyName(InboxConstants.Title)]
    public string? Title { get; set; }

    [JsonPropertyName(InboxConstants.Subtitle)]
    public string? Subtitle { get; set; }

    [JsonPropertyName(InboxConstants.Source)]
    public string? Source { get; set; }

    [JsonPropertyName(InboxConstants.Body)]
    public string? Body { get; set; }
}
