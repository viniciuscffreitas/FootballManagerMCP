using System.Text.Json;
using System.Text.Json.Serialization;
using FootballManager.Api.Responses.Contracts;

namespace FootballManager.Tests;

// The API leaves nulls out of its JSON; in a contract change a null says "not known" and must reach the caller.
public class ContractChangeResponseTests
{
    private static readonly JsonSerializerOptions ApiJson = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Theory]
    [InlineData("dialog")]
    [InlineData("stillInSquad")]
    [InlineData("confirmed")]
    [InlineData("confirmation")]
    public void A_null_that_means_not_known_is_written_as_null(string field)
    {
        var json = JsonSerializer.Serialize(new ContractChangeResponse { PlayerName = "Reserva", Choice = "Rotação" }, ApiJson);

        Assert.Equal(JsonValueKind.Null, JsonDocument.Parse(json).RootElement.GetProperty(field).ValueKind);
    }
}
