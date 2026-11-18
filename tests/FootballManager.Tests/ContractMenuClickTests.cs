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

// The service picks a contract entry from the menu it read; the client reads the menu again to send it, then answers the
// game's confirmation dialog. The game can change the menu in between (the save guard copies the save first), and the
// dialog that shows may not be the one the entry opens, so the client checks both before it writes.
public class ContractMenuClickTests
{
    private const int Reserve = 7;
    private static readonly string[] Rotation = ["Status no elenco", "Rotação"];
    private static readonly string[] Release = ["Liberar por transferência livre"];

    // The client maps the game's answers with the app's global Mapster config, as the app registers it at start.
    static ContractMenuClickTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task The_entry_picked_is_sent_when_the_menu_still_has_it()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"));

        await Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation);

        Assert.Contains("eventId=44", Uri.UnescapeDataString(Assert.Single(plugin.Writes).Query));
    }

    [Fact]
    public async Task Another_entry_at_the_same_place_is_refused_and_nothing_is_sent()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Reserva"));

        var refusal = await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));

        Assert.Contains("Rotação", refusal.Message);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_the_game_greyed_out_since_the_check_is_refused_and_nothing_is_sent()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação", lastDisabled: true));

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_entry_under_a_submenu_the_game_greyed_out_is_refused_and_nothing_is_sent()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação", topDisabled: true));

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task An_action_the_game_no_longer_offers_is_refused_and_nothing_is_sent()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), actions: """{"values":{"Actions":{}}}""");

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));

        Assert.Empty(plugin.Writes);
    }

    [Theory]
    [InlineData("99")]
    [InlineData("0")]
    [InlineData("null")]
    public async Task An_entry_with_an_event_the_client_doesnt_know_is_refused_and_nothing_is_sent(string eventId)
    {
        var plugin = new ContractPlugin($$"""[{"Text":"Liberar por transferência livre","Event":{{eventId}},"Disabled":false}]""");

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.ReleaseOnFreeTransfer, [0], Release));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task A_release_whose_cost_changed_since_the_dry_run_is_refused_and_nothing_is_sent()
    {
        var plugin = new ContractPlugin(ReleaseMenu("Isso custará R$240 mil ao clube."));

        var refusal = await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.ReleaseOnFreeTransfer, [0], Release,
                "Isso custará R$120 mil ao clube."));

        Assert.Contains("R$240 mil", refusal.Message);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task A_release_whose_cost_is_still_the_approved_one_is_sent()
    {
        var plugin = new ContractPlugin(ReleaseMenu("Isso custará\nR$120 mil ao clube."));

        await Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.ReleaseOnFreeTransfer, [0], Release,
            "isso custará R$120 mil ao clube.");

        Assert.Single(plugin.Writes);
    }

    [Fact]
    public async Task The_menu_is_checked_by_the_read_right_before_the_send()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"));

        await Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation);

        var send = plugin.Requests.FindIndex(request => request.Method == HttpMethod.Post);
        Assert.Contains($"props={ScoutingConstants.MenuItems}", Uri.UnescapeDataString(plugin.Requests[send - 1].RequestUri!.Query));
    }

    [Fact]
    public async Task A_send_the_game_didnt_answer_is_unknown_not_unsent()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação")) { WriteTimesOut = true };

        await Assert.ThrowsAsync<GameAnswerLostException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));
    }

    [Fact]
    public async Task A_send_the_game_answered_unreadably_is_unknown_not_a_crash()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação")) { WriteAnswer = "{not json" };

        await Assert.ThrowsAsync<GameAnswerLostException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));
    }

    [Fact]
    public async Task A_yes_the_game_answered_unreadably_is_unknown_not_a_crash()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), Dialog("Mudar o status de Reserva para Rotação?"))
        {
            WriteAnswer = "{not json",
            StaysOpen = true
        };

        var answer = await Client(plugin).AnswerDialogAsync("Mudar o status de Reserva para Rotação?");

        Assert.Equal(DialogState.Unknown, answer.State);
    }

    [Fact]
    public async Task A_dialog_already_open_is_left_alone_and_nothing_is_sent()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), Dialog("Enviar a numeração atual do elenco?"));

        await Assert.ThrowsAsync<GameRejectedException>(() =>
            Client(plugin).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime, [0, 1], Rotation));

        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task Actions_the_game_didnt_answer_are_unknown_not_none()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), actions: """{"values":{},"pending":["Actions"]}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => Client(plugin).GetOfferedActionsAsync(Reserve));
    }

    [Fact]
    public async Task Actions_the_game_failed_to_read_are_unknown_not_none()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), actions: """{"values":{},"errors":{"Actions":"Reading the value failed"}}""");

        await Assert.ThrowsAsync<GameUnavailableException>(() => Client(plugin).GetOfferedActionsAsync(Reserve));
    }

    [Fact]
    public async Task The_dialog_of_the_action_asked_for_is_answered_yes()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"),
            Dialog("Liberar Reserva por transferência livre? Isso custará R$120 mil ao clube."));

        var answer = await Client(plugin).AnswerDialogAsync("Isso custará R$120 mil ao clube.");

        Assert.Equal(DialogState.Answered, answer.State);
        Assert.Contains("R$120 mil", answer.Text);
        Assert.Contains(DialogConstants.ClickEndpoint, Assert.Single(plugin.Writes).AbsolutePath);
    }

    [Fact]
    public async Task A_dialog_holding_the_confirmation_with_other_case_and_spaces_is_answered()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), Dialog("Liberar Reserva?  ISSO custará R$120 mil ao clube."));

        var answer = await Client(plugin).AnswerDialogAsync("isso custará\nR$120 mil ao clube.");

        Assert.Equal(DialogState.Answered, answer.State);
    }

    [Fact]
    public async Task A_dialog_holding_only_part_of_the_confirmation_is_not_answered()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), Dialog("Liberar Reserva por transferência livre?"));

        var answer = await Client(plugin).AnswerDialogAsync("Liberar Reserva por transferência livre? Isso custará R$120 mil ao clube.");

        Assert.Equal(DialogState.NotAnswered, answer.State);
        Assert.Empty(plugin.Writes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task An_empty_confirmation_never_answers_a_dialog(string confirmation)
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), Dialog("Liberar Reserva por transferência livre?"));

        var answer = await Client(plugin).AnswerDialogAsync(confirmation);

        Assert.Equal(DialogState.NotAnswered, answer.State);
        Assert.Contains("Reserva", answer.Text);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task A_dialog_with_another_cost_than_the_dry_runs_is_not_answered()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"),
            Dialog("Liberar Reserva por transferência livre? Isso custará R$900 mil ao clube."));

        var answer = await Client(plugin).AnswerDialogAsync("Isso custará R$120 mil ao clube.");

        Assert.Equal(DialogState.NotAnswered, answer.State);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task No_dialog_showing_is_said_and_nothing_is_clicked()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"));

        var answer = await Client(plugin).AnswerDialogAsync("Mudar o status de Reserva para Rotação?");

        Assert.Equal(DialogState.NotShown, answer.State);
        Assert.Empty(plugin.Writes);
    }

    [Fact]
    public async Task A_dialog_still_open_after_yes_is_unknown()
    {
        var plugin = new ContractPlugin(StatusMenu("Titular", "Rotação"), Dialog("Mudar o status de Reserva para Rotação?")) { StaysOpen = true };

        var answer = await Client(plugin).AnswerDialogAsync("Mudar o status de Reserva para Rotação?");

        Assert.Equal(DialogState.Unknown, answer.State);
        Assert.Single(plugin.Writes);
    }

    private static ScoutingEditorBridgeClient Client(ContractPlugin plugin)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        var bridge = new GameBridge(factory, NullLogger<GameBridge>.Instance);
        var scouting = Substitute.For<IScoutingGameClient>();
        scouting.GetFocusesAsync(Arg.Any<CancellationToken>()).Returns([]);
        var search = new DatabaseSearchBridgeClient(bridge, new SearchedRecords());

        return new ScoutingEditorBridgeClient(bridge, scouting, new RemovedFocusLog(), new SearchPositions(), search,
            new TeamsBridgeClient(bridge, new GameVocabulary(bridge), search), new CompetitionPlaces(), new GameDialogs(bridge));
    }

    private static string StatusMenu(string first, string second, bool lastDisabled = false, bool topDisabled = false)
    {
        return "[{\"Text\":\"Status no elenco\",\"Event\":0,\"Disabled\":" + Json(topDisabled) + ",\"ChildList\":["
               + "{\"RowData\":{\"Text\":\"" + first + "\",\"Event\":44,\"Disabled\":false}},"
               + "{\"RowData\":{\"Text\":\"" + second + "\",\"Event\":44,\"Disabled\":" + Json(lastDisabled) + "}}]}]";
    }

    private static string ReleaseMenu(string cost)
    {
        return "[{\"Text\":\"Liberar por transferência livre\",\"Event\":44,\"Disabled\":false,"
               + "\"ActionData\":{\"ConfirmationDialogData\":{\"Description\":" + System.Text.Json.JsonSerializer.Serialize(cost) + "}}}]";
    }

    private static string Json(bool value)
    {
        return value ? "true" : "false";
    }

    // The game's Yes/No dialog as /ui lists it.
    private static string Dialog(string description)
    {
        return string.Join('\n',
            "[0] UIDocument on Modal",
            "  0/4 Panel#GenericModalDialog .panel--visible @0,0 800x600",
            "  0/4/1 SIText#dialog-title \"Confirme, por favor\" @10,10 300x20",
            $"  0/4/2 SIText#dialog-description \"{description}\" @10,40 300x20",
            "  0/4/3 SIButton#buttons-button-primary-default \"Sim\" @10,80 100x20");
    }

    // The plugin as the client sees it: the club's own player in the squad, his contract actions offered, the given
    // menu, and the given dialog open in the game's UI until a click on "Yes" closes it (unless it stays open).
    private sealed class ContractPlugin(string items, string? dialog = null, string? actions = null) : HttpMessageHandler
    {
        private bool _clicked;

        public bool StaysOpen { get; init; }
        public bool WriteTimesOut { get; init; }
        public string WriteAnswer { get; init; } = "{}";
        public List<Uri> Writes { get; } = [];
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var url = Uri.UnescapeDataString(request.RequestUri!.PathAndQuery);

            if (request.Method == HttpMethod.Post)
            {
                if (WriteTimesOut)
                {
                    throw new TaskCanceledException("timed out");
                }

                Writes.Add(request.RequestUri!);
                _clicked |= request.RequestUri!.AbsolutePath == DialogConstants.ClickEndpoint;
                return Answer(WriteAnswer);
            }

            if (url.StartsWith("/ui", StringComparison.Ordinal))
            {
                return Answer(dialog is not null && (!_clicked || StaysOpen) ? dialog : "[0] UIDocument on Main");
            }

            if (url.Contains($"props={ScoutingConstants.MenuItems}"))
            {
                return Answer($$$"""{"values":{"Items":{{{items}}}}}""");
            }

            if (url.Contains($"props={ScoutingConstants.Actions}"))
            {
                return Answer(actions ?? "{\"values\":{\"Actions\":{\"#" + ScoutingConstants.SetPlayingTimeAction + "\":{},\"#"
                    + ScoutingConstants.ReleaseOnFreeTransferAction + "\":{}}}}");
            }

            // The lists a player is found in: the club's own squad holds him.
            return Answer("{\"count\":1,\"items\":[{\"values\":{\"UniqueId\":" + Reserve + ",\"Name\":\"Reserva\"}}]}");
        }

        private static Task<HttpResponseMessage> Answer(string body)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, body.StartsWith('{') ? "application/json" : "text/plain")
            });
        }
    }
}
