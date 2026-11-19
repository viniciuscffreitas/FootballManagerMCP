using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Registration;

internal sealed class BridgeNeedingNumbersDto
{
    // The players as references; only their count is used.
    [JsonPropertyName(RegistrationConstants.NeedingNumbers)]
    public List<JsonElement>? Players { get; set; }
}
