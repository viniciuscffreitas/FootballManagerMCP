using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Messages;

internal sealed class BridgeRequiredResponsesDto
{
    [JsonPropertyName(InboxConstants.RequiredResponses)]
    public int? RequiredResponses { get; set; }
}
