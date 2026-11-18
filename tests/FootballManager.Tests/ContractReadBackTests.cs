using System.Net;
using System.Text;
using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.Services;
using FootballManager.Domain.Contracts;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Mappings;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

// The read back of a contract change goes through the plugin's squad answer, as the app reads it: an answer it can't
// read after the write was sent must leave the change applied and unconfirmed, never fail the call.
public class ContractReadBackTests
{
    private const int Reserve = 7;

    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();

    static ContractReadBackTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public ContractReadBackTests()
    {
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, ScoutedPlayer> { [Reserve] = new() { Id = Reserve, Name = "Reserva" } });
        _editor.GetOfferedActionsAsync(Reserve, Arg.Any<CancellationToken>()).Returns([PlayerMenuAction.SetPlayingTime]);
        _editor.GetMenuAsync(Reserve, PlayerMenuAction.SetPlayingTime, Arg.Any<CancellationToken>()).Returns(
        [
            new MenuEntry
            {
                Label = "Status no elenco",
                Children = [new MenuEntry { Label = "Titular" }, new MenuEntry { Label = "Rotação" }]
            }
        ]);
        _editor.AnswerDialogAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new DialogAnswer { State = DialogState.Answered, Text = "Mudar o status de Reserva?" });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
    }

    [Fact]
    public async Task A_read_back_answered_with_something_unreadable_leaves_the_change_applied_and_unconfirmed()
    {
        var service = Service(Squad("Reserva"), "{not json");

        var result = await service.SetSquadStatusAsync(Reserve, "Rotação", apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AppliedState.Yes, result.Value.Applied);
        Assert.Null(result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("reading it back failed"));
    }

    // An answer without the list would read as an empty squad, him gone: the release isn't confirmed by it.
    [Fact]
    public async Task A_release_read_back_without_the_list_is_not_confirmed()
    {
        const string cost = "Isso custará R$120 mil ao clube.";
        _editor.GetOfferedActionsAsync(Reserve, Arg.Any<CancellationToken>()).Returns([PlayerMenuAction.ReleaseOnFreeTransfer]);
        _editor.GetMenuAsync(Reserve, PlayerMenuAction.ReleaseOnFreeTransfer, Arg.Any<CancellationToken>())
            .Returns([new MenuEntry { Label = "Liberar por transferência livre", Confirmation = cost }]);
        var service = Service(Squad("Reserva"), """{"count":0}""");

        var result = await service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: cost);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(true, result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("reading it back failed"));
    }

    private ContractActionsService Service(params string[] squadAnswers)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        var plugin = new SquadPlugin(squadAnswers);
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(plugin) { BaseAddress = new Uri("http://localhost:5121") });
        var contracts = new ContractsBridgeClient(new GameBridge(factory, NullLogger<GameBridge>.Instance));
        return new ContractActionsService(_editor, contracts, _saveGuard);
    }

    private static string Squad(string playingTime)
    {
        return "{\"count\":1,\"items\":[{\"values\":{\"UniqueId\":" + Reserve + ",\"Name\":\"Reserva\","
               + "\"FullContract.AgreedPlayingTime\":{\"DisplayValue\":\"" + playingTime + "\"}}}]}";
    }

    // The plugin's squad answers in turn; the last one repeats.
    private sealed class SquadPlugin(string[] answers) : HttpMessageHandler
    {
        private int _next;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = answers[Math.Min(_next++, answers.Length - 1)];
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
