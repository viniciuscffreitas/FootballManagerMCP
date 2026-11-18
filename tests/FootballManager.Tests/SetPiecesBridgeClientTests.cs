using System.Net;
using System.Text;
using FootballManager.Application.Exceptions;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

// The captaincy as the plugin answers it: the team's captain and vice, the player's menu actions and the entries of one.
public class SetPiecesBridgeClientTests
{
    private const int Player = 7;

    [Fact]
    public async Task A_captain_the_game_failed_to_read_is_unknown_not_none()
    {
        var plugin = new CaptaincyPlugin(team: """{"values":{"ViceCaptain.UniqueId":2},"errors":{"Captain.UniqueId":"Reading the value failed"}}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => Client(plugin).ReadAsync());
    }

    [Fact]
    public async Task A_vice_captain_the_game_didnt_answer_is_unknown_not_none()
    {
        var plugin = new CaptaincyPlugin(team: """{"values":{"Captain.UniqueId":1},"pending":["ViceCaptain.UniqueId"]}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => Client(plugin).ReadAsync());
    }

    [Fact]
    public async Task No_captain_read_without_errors_is_none()
    {
        var setup = await Client(new CaptaincyPlugin(team: """{"values":{}}""")).ReadAsync();

        Assert.Null(setup.Captain);
        Assert.Null(setup.ViceCaptain);
    }

    private static SetPiecesBridgeClient Client(CaptaincyPlugin plugin)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        return new SetPiecesBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
    }

    // The plugin as the client sees it: both captaincy actions offered on the player, with the given entries.
    private sealed class CaptaincyPlugin(string team = """{"values":{}}""", string items = "[]") : HttpMessageHandler
    {
        public List<Uri> Writes { get; } = [];
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (request.Method == HttpMethod.Post)
            {
                Writes.Add(request.RequestUri!);
                return Answer("{}");
            }

            var url = Uri.UnescapeDataString(request.RequestUri!.Query);

            if (url.Contains(SetPieceConstants.CaptainId))
            {
                return Answer(team);
            }

            if (url.Contains($"props={ScoutingConstants.MenuItems}"))
            {
                return Answer($$$"""{"values":{"Items":{{{items}}}}}""");
            }

            if (url.Contains($"props={ScoutingConstants.Actions}"))
            {
                return Answer($"{{\"values\":{{\"Actions\":{{\"#{SetPieceConstants.MakeCaptainAction}\":{{}},\"#{SetPieceConstants.MakeViceCaptainAction}\":{{}}}}}}}}");
            }

            return Answer("""{"count":0,"items":[]}""");
        }

        private static Task<HttpResponseMessage> Answer(string body)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
