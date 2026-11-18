using FluentResults;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Contracts;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using NSubstitute;

namespace FootballManager.Tests;

public class ContractActionsServiceTests
{
    private const int Reserve = 7;

    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly IContractsGameClient _contracts = Substitute.For<IContractsGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly ContractActionsService _service;

    public ContractActionsServiceTests()
    {
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, ScoutedPlayer> { [Reserve] = new() { Id = Reserve, Name = "Reserva" } });
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>())
            .Returns([new SquadPlayerContract { PlayerId = Reserve, Name = "Reserva", AgreedPlayingTime = "Reserva" }]);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _service = new ContractActionsService(_editor, _contracts, _saveGuard);
    }

    [Fact]
    public async Task The_release_dry_run_shows_the_games_cost_and_writes_nothing()
    {
        Offered(PlayerMenuAction.ReleaseOnFreeTransfer);
        Menu(PlayerMenuAction.ReleaseOnFreeTransfer,
            new MenuEntry { Label = "Liberar por transferência livre", Confirmation = "Isso custará R$120 mil ao clube." });

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: false, confirmRelease: false)).Value;

        Assert.False(change.Applied);
        Assert.Equal("Liberar por transferência livre", change.Choice);
        Assert.Contains("R$120 mil", change.Confirmation);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Applying_a_release_needs_its_explicit_confirmation()
    {
        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: false);

        Assert.True(result.HasError<ValidationError>());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Applying_a_release_clears_the_save_then_clicks_and_answers_the_dialog()
    {
        Offered(PlayerMenuAction.MutualTermination);
        Menu(PlayerMenuAction.MutualTermination, new MenuEntry { Label = "Rescindir contrato" });
        _editor.ClickMenuEntryAsync(Reserve, PlayerMenuAction.MutualTermination, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns("Tem certeza?");
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns(
            [new SquadPlayerContract { PlayerId = Reserve, Name = "Reserva" }],
            new List<SquadPlayerContract>());

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Mutual, apply: true, confirmRelease: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _editor.ClickMenuEntryAsync(Reserve, PlayerMenuAction.MutualTermination,
                Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0 })), Arg.Any<CancellationToken>());
        });
        Assert.True(change.Applied);
        Assert.Equal("Tem certeza?", change.DialogAnswered);
        Assert.False(change.StillInSquad);
    }

    [Fact]
    public async Task A_refused_save_writes_nothing()
    {
        Offered(PlayerMenuAction.DofContractRenewal);
        Menu(PlayerMenuAction.DofContractRenewal, new MenuEntry { Label = "Pedir ao DoF para renovar" });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>()).Returns(Result.Fail<SaveClearance>(new SaveRefusedError("real save")));

        var result = await _service.HandRenewalToDofAsync(Reserve, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task An_action_the_menu_doesnt_offer_is_a_clear_error_and_nothing_is_sent()
    {
        Offered(PlayerMenuAction.SetPlayingTime);

        var result = await _service.HandRenewalToDofAsync(Reserve, apply: true);

        Assert.Contains("doesn't offer", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task A_player_outside_the_squads_lists_is_not_found()
    {
        var result = await _service.HandRenewalToDofAsync(99, apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    [Fact]
    public async Task The_squad_status_is_picked_by_its_label_and_read_back()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns(
            [new SquadPlayerContract { PlayerId = Reserve, Name = "Reserva", AgreedPlayingTime = "Reserva" }],
            [new SquadPlayerContract { PlayerId = Reserve, Name = "Reserva", AgreedPlayingTime = "Rotação" }]);

        var change = (await _service.SetSquadStatusAsync(Reserve, "rotação", apply: true)).Value;

        await _editor.Received(1).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0, 1 })), Arg.Any<CancellationToken>());
        Assert.Equal("Reserva", change.AgreedPlayingTime);
        Assert.Equal("Rotação", change.AgreedPlayingTimeAfter);
        Assert.Equal(["Titular", "Rotação", "Reserva"], change.Choices);
    }

    [Fact]
    public async Task A_status_the_menu_doesnt_have_is_refused_with_the_ones_it_has()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();

        var result = await _service.SetSquadStatusAsync(Reserve, "Inexistente", apply: false);

        Assert.Contains("Titular, Rotação, Reserva", Messages(result));
    }

    [Fact]
    public async Task A_greyed_out_entry_is_not_picked()
    {
        Offered(PlayerMenuAction.SetForRelease);
        Menu(PlayerMenuAction.SetForRelease, new MenuEntry { Label = "Liberar no fim do contrato", IsDisabled = true });

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.EndOfContract, apply: false, confirmRelease: false);

        Assert.Contains("Liberar no fim do contrato", Messages(result));
    }

    private void Offered(params PlayerMenuAction[] actions)
    {
        _editor.GetOfferedActionsAsync(Reserve, Arg.Any<CancellationToken>()).Returns(actions.ToHashSet());
    }

    private void Menu(PlayerMenuAction action, params MenuEntry[] entries)
    {
        _editor.GetMenuAsync(Reserve, action, Arg.Any<CancellationToken>()).Returns(entries.ToList());
    }

    // The status menu: one entry, with the statuses below it; his own status greyed out.
    private void StatusMenu()
    {
        Menu(PlayerMenuAction.SetPlayingTime, new MenuEntry
        {
            Label = "Status no elenco",
            Children =
            [
                new MenuEntry { Label = "Titular" },
                new MenuEntry { Label = "Rotação" },
                new MenuEntry { Label = "Reserva", IsDisabled = true }
            ]
        });
    }

    private static string Messages<T>(Result<T> result)
    {
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
