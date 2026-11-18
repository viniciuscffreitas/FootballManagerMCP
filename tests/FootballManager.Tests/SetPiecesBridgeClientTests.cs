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

    [Fact]
    public async Task The_menu_entry_is_sent_when_the_menu_still_has_it()
    {
        var plugin = new CaptaincyPlugin(items: Entry("Tornar Capitão"));

        await Client(plugin).MakeCaptainAsync(Player);

        Assert.Single(plugin.Writes);
    }

    [Fact]
    public async Task The_menu_is_checked_by_the_read_right_before_the_send()
    {
        var plugin = new CaptaincyPlugin(items: Entry("Tornar Vice-capitão"));

        await Client(plugin).MakeViceCaptainAsync(Player);

        var send = plugin.Requests.FindIndex(request => request.Method == HttpMethod.Post);
        Assert.Contains($"props={ScoutingConstants.MenuItems}", Uri.UnescapeDataString(plugin.Requests[send - 1].RequestUri!.Query));
    }

    [Theory]
    [InlineData("Tornar Vice-capitão", false, 44)]
    [InlineData("Tornar Capitão", true, 44)]
    [InlineData("Tornar Capitão", false, 0)]
    [InlineData("Tornar Capitão", false, 99)]
    public async Task An_entry_that_isnt_the_one_expected_is_refused_and_nothing_is_sent(string label, bool disabled, int eventId)
    {
        var plugin = new CaptaincyPlugin(items: Entry(label, disabled, eventId));

        await Assert.ThrowsAsync<GameRejectedException>(() => Client(plugin).MakeCaptainAsync(Player));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task A_menu_without_entries_is_refused_and_nothing_is_sent()
    {
        var plugin = new CaptaincyPlugin(items: "[]");

        await Assert.ThrowsAsync<GameRejectedException>(() => Client(plugin).MakeCaptainAsync(Player));

        Assert.Empty(plugin.Writes);
    }

    private static SetPiecesBridgeClient Client(CaptaincyPlugin plugin)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        return new SetPiecesBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
    }

    private static string Entry(string label, bool disabled = false, int eventId = ScoutingConstants.DoActionEventId)
    {
        return $$"""[{"Text":"{{label}}","Event":{{eventId}},"Disabled":{{(disabled ? "true" : "false")}}}]""";
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
