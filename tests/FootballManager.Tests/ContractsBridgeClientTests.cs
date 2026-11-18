using System.Net;
using System.Text;
using FootballManager.Application.Exceptions;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Mappings;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

// A squad row whose id the game didn't give is unknown: read as 0, a released player would look gone from the squad.
public class ContractsBridgeClientTests
{
    static ContractsBridgeClientTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Theory]
    [InlineData("""{"values":{"Name":"Reserva"}}""")]
    [InlineData("""{"values":{"Name":"Reserva"},"pending":["UniqueId"]}""")]
    [InlineData("""{"values":{"Name":"Reserva"},"errors":{"UniqueId":"Reading the value failed"}}""")]
    public async Task A_row_without_its_id_makes_the_read_unavailable(string row)
    {
        var client = Client($$"""{"count":1,"items":[{{row}}]}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => client.GetContractsAsync());
    }

    [Theory]
    [InlineData("null")]
    [InlineData("""{"values":null}""")]
    public async Task A_row_the_game_sent_empty_makes_the_read_unavailable(string row)
    {
        var client = Client("""{"count":2,"items":[{"values":{"UniqueId":7,"Name":"Reserva"}},""" + row + "]}");

        await Assert.ThrowsAsync<GameUnavailableException>(() => client.GetContractsAsync());
    }

    [Fact]
    public async Task A_list_the_game_sent_as_null_makes_the_read_unavailable()
    {
        await Assert.ThrowsAsync<GameUnavailableException>(() => Client("""{"count":0,"items":null}""").GetContractsAsync());
    }

    [Fact]
    public async Task Rows_with_their_ids_are_read()
    {
        var client = Client("""{"count":1,"items":[{"values":{"UniqueId":7,"Name":"Reserva"}}]}""");

        Assert.Equal(7, Assert.Single(await client.GetContractsAsync()).PlayerId);
    }

    private static ContractsBridgeClient Client(string squad)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(new SquadPlugin(squad)) { BaseAddress = new Uri("http://localhost:5121") });
        return new ContractsBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
    }

    private sealed class SquadPlugin(string squad) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(squad, Encoding.UTF8, "application/json")
            });
        }
    }
}
