using System.Net;
using System.Text;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Mappings;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

// The exit service picks an entry from the menu it read; the client reads the menu again to send it. The game can change
// the menu in between (the save guard copies the save first), so the client checks the entry is still the one picked.
public class ScoutingEditorBridgeClientTests
{
    private const int Player = 7;
    private static readonly string[] TransferListed = ["Situação de transferência", "Na lista de transferências"];

    // The client maps what the game sends with the global settings, as the API registers them.
    static ScoutingEditorBridgeClientTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task The_entry_picked_is_sent_when_the_menu_still_has_it()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências"));

        await (await plugin.FoundAsync()).SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed);

        Assert.Single(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_that_changed_since_the_check_is_refused_and_nothing_is_sent()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Indisponível"));

        var refusal = await Assert.ThrowsAsync<GameRejectedException>(async () =>
            await (await plugin.FoundAsync()).SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Contains("Na lista de transferências", refusal.Message);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_the_game_greyed_out_since_the_check_is_refused_and_nothing_is_sent()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências", leafDisabled: true));

        await Assert.ThrowsAsync<GameRejectedException>(async () =>
            await (await plugin.FoundAsync()).SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_under_a_submenu_the_game_greyed_out_is_refused_and_nothing_is_sent()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências", topDisabled: true));

        await Assert.ThrowsAsync<GameRejectedException>(async () =>
            await (await plugin.FoundAsync()).SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_the_menu_no_longer_reaches_is_refused_and_nothing_is_sent()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida"));

        await Assert.ThrowsAsync<GameRejectedException>(async () =>
            await (await plugin.FoundAsync()).SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task The_menu_is_checked_by_the_read_right_before_the_send()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências"));

        await (await plugin.FoundAsync()).SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed);

        var send = plugin.Requests.FindIndex(request => request.Method == HttpMethod.Post);
        Assert.Contains($"props={ScoutingConstants.MenuItems}", Uri.UnescapeDataString(plugin.Requests[send - 1].RequestUri!.Query));
    }

    [Fact]
    public async Task A_recalled_player_is_found_again_in_the_squad()
    {
        var plugin = new PlayerPlugin("""[{"Text":"Trazer do empréstimo","Event":44,"Disabled":false}]""", outOnLoan: true);
        var client = await plugin.FoundAsync();

        await client.SendMenuEntryAsync(Player, PlayerMenuAction.RecallFromLoan, [0], ["Trazer do empréstimo"]);
        var offered = await client.GetOfferedActionsAsync(Player);

        Assert.DoesNotContain(PlayerMenuAction.RecallFromLoan, offered);
        Assert.Contains(PlayerMenuAction.SetTransferStatus, offered);
    }

    [Fact]
    public async Task A_send_the_game_doesnt_answer_is_a_lost_answer_not_a_refusal()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências"), sendTimesOut: true);
        var client = await plugin.FoundAsync();

        await Assert.ThrowsAsync<GameAnswerLostException>(() =>
            client.SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Single(plugin.Writes);
    }

    [Theory]
    [InlineData("""{"values":{},"pending":["Actions"]}""")]
    [InlineData("""{"values":{},"errors":{"Actions":"Reading the value failed"}}""")]
    [InlineData("""{"values":{}}""")]
    public async Task Actions_the_game_didnt_read_are_unknown_not_none(string actions)
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida"), actions: actions);
        var client = plugin.Client();
        await client.FindPlayersAsync([Player]);

        await Assert.ThrowsAsync<GameUnavailableException>(() => client.GetOfferedActionsAsync(Player));
    }

    [Theory]
    [InlineData("""{"values":{},"pending":["Items"]}""")]
    [InlineData("""{"values":{},"errors":{"Items":"Reading the value failed"}}""")]
    [InlineData("""{"values":{}}""")]
    [InlineData("""{"values":{"Items":[null]}}""")]
    [InlineData("""{"values":{"Items":[{"Text":"Situação de transferência","Event":0,"ChildList":[null]}]}}""")]
    public async Task A_menu_the_game_didnt_read_is_unknown_not_empty(string menu)
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida"), menu: menu);
        var client = await plugin.FoundAsync();

        await Assert.ThrowsAsync<GameUnavailableException>(() => client.GetMenuAsync(Player, PlayerMenuAction.SetTransferStatus));
    }

    [Theory]
    [InlineData("""{"values":{},"pending":["Items"]}""")]
    [InlineData("""{"values":{"Items":[null]}}""")]
    public async Task A_menu_the_game_didnt_read_right_before_the_send_sends_nothing(string menu)
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências"), menu: menu);
        var client = await plugin.FoundAsync();

        await Assert.ThrowsAsync<GameUnavailableException>(() =>
            client.SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task A_send_answered_with_something_unreadable_is_a_lost_answer()
    {
        var plugin = new PlayerPlugin(StatusMenu("Sem situação definida", "Na lista de transferências"), sendAnswer: "<html>oops</html>");
        var client = await plugin.FoundAsync();

        await Assert.ThrowsAsync<GameAnswerLostException>(() =>
            client.SendMenuEntryAsync(Player, PlayerMenuAction.SetTransferStatus, [0, 1], TransferListed));

        Assert.Single(plugin.Writes);
    }

    private static string StatusMenu(string first, string? second = null, bool leafDisabled = false, bool topDisabled = false)
    {
        var levels = new List<string> { Level(first, leafDisabled && second is null) };

        if (second is not null)
        {
            levels.Add(Level(second, leafDisabled));
        }

        return $$"""[{"Text":"Situação de transferência","Event":0,"Disabled":{{(topDisabled ? "true" : "false")}},"ChildList":[{{string.Join(',', levels)}}]}]""";
    }

    private static string Level(string label, bool disabled)
    {
        return $$$"""{"RowData":{"Text":"{{{label}}}","Event":44,"Disabled":{{{(disabled ? "true" : "false")}}}}}""";
    }

    // The plugin as the client sees it: the player in the club's squad, his menu offering the transfer status entries;
    // or out on loan, his menu offering the recall, and back in the squad once the recall is sent.
    private sealed class PlayerPlugin(
        string items, bool outOnLoan = false, bool sendTimesOut = false, string? actions = null, string? menu = null, string? sendAnswer = null)
        : HttpMessageHandler
    {
        private bool _outOnLoan = outOnLoan;

        public List<Uri> Writes { get; } = [];
        public List<HttpRequestMessage> Requests { get; } = [];

        // As the exit service does: the player is found first, in the club's squad, then his menu is used.
        public async Task<ScoutingEditorBridgeClient> FoundAsync()
        {
            var client = Client();
            Assert.Contains(Player, (await client.FindPlayersAsync([Player])).Keys);
            return client;
        }

        public ScoutingEditorBridgeClient Client()
        {
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(this) { BaseAddress = new Uri("http://localhost:5121") });
            var bridge = new GameBridge(factory, NullLogger<GameBridge>.Instance);
            var scouting = Substitute.For<IScoutingGameClient>();
            scouting.GetFocusesAsync(Arg.Any<CancellationToken>()).Returns([]);
            var search = new DatabaseSearchBridgeClient(bridge, new SearchedRecords());

            return new ScoutingEditorBridgeClient(
                bridge, scouting, new RemovedFocusLog(), new SearchPositions(), search,
                new TeamsBridgeClient(bridge, new GameVocabulary(bridge), search), new CompetitionPlaces(), new GameDialogs(bridge));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (request.Method == HttpMethod.Post)
            {
                Writes.Add(request.RequestUri!);
                _outOnLoan = false;

                // The request went out; its answer never comes back.
                return sendTimesOut ? Task.FromException<HttpResponseMessage>(new TaskCanceledException("timeout")) : Answer(sendAnswer ?? "{}");
            }

            var url = Uri.UnescapeDataString(request.RequestUri!.Query);
            var squad = $"{TeamConstants.OwnTeam}.{TeamConstants.Players}";

            if (url.Contains($"props={ScoutingConstants.MenuItems}"))
            {
                return Answer(menu ?? $$$"""{"values":{"Items":{{{items}}}}}""");
            }

            if (url.Contains($"props={ScoutingConstants.Actions}"))
            {
                // A player the list no longer holds isn't there to read.
                if (url.Contains(YouthConstants.Loans) != _outOnLoan)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("No item has that UniqueId") });
                }

                if (actions is not null)
                {
                    return Answer(actions);
                }

                var action = _outOnLoan ? ScoutingConstants.RecallFromLoanAction : ScoutingConstants.SetTransferStatusAction;
                return Answer($"{{\"values\":{{\"Actions\":{{\"#{action}\":{{}}}}}}}}");
            }

            // The list that holds him: the club's squad, or its players out on loan; every other list is empty.
            return url.Contains($"list={(_outOnLoan ? YouthConstants.Loans : squad)}&")
                ? Answer($$$"""{"count":1,"items":[{"values":{"{{{GameConstants.UniqueId}}}":{{{Player}}},"{{{GameConstants.Name}}}":"Walter"}}]}""")
                : Answer("""{"count":0,"items":[]}""");
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
