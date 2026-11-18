using FluentResults;
using FootballManager.Application.Exceptions;
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
    private const string Cost = "Isso custará R$120 mil ao clube.";

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
            new MenuEntry { CanRun = true, Label = "Liberar por transferência livre", Confirmation = Cost });

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: false, confirmRelease: false)).Value;

        Assert.Equal(AppliedState.No, change.Applied);
        Assert.Null(change.Dialog);
        Assert.Equal("Liberar por transferência livre", change.Choice);
        Assert.Contains("R$120 mil", change.Confirmation);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_release_kind_it_doesnt_know_is_refused()
    {
        var result = await _service.ReleaseAsync(Reserve, (ReleaseKind)7, apply: false, confirmRelease: false);

        Assert.True(result.HasError<ValidationError>());
        await _editor.DidNotReceiveWithAnyArgs().GetMenuAsync(default, default);
    }

    [Fact]
    public async Task A_release_without_how_is_refused_before_the_save_is_touched()
    {
        var result = await _service.ReleaseAsync(Reserve, null, apply: true, confirmRelease: true);

        Assert.True(result.HasError<ValidationError>());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().GetMenuAsync(default, default);
    }

    [Fact]
    public async Task A_change_without_the_player_is_refused_before_the_game_is_read()
    {
        var results = new[]
        {
            await _service.ReleaseAsync(null, ReleaseKind.Free, apply: true, confirmRelease: true),
            await _service.SetSquadStatusAsync(null, "Rotação", apply: true),
            await _service.HandRenewalToDofAsync(null, apply: true)
        };

        Assert.All(results, result => Assert.Contains(result.Errors.OfType<ValidationError>(), error => error.PropertyName == "playerId"));
        await _editor.DidNotReceiveWithAnyArgs().FindPlayersAsync(default!);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task Applying_a_release_needs_its_explicit_confirmation()
    {
        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: false);

        Assert.True(result.HasError<ValidationError>());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task Applying_a_release_clears_the_save_then_clicks_and_answers_the_dialog()
    {
        Offered(PlayerMenuAction.MutualTermination);
        Menu(PlayerMenuAction.MutualTermination, new MenuEntry { CanRun = true, Label = "Rescindir contrato", Confirmation = Cost });
        Dialog(DialogState.Answered, "Rescindir o contrato de Reserva?");

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Mutual, apply: true, confirmRelease: true, expectedConfirmation: Cost)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _editor.ClickMenuEntryAsync(Reserve, PlayerMenuAction.MutualTermination,
                Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0 })),
                Arg.Is<IReadOnlyList<string>>(labels => labels.SequenceEqual(new[] { "Rescindir contrato" })), Cost, Arg.Any<CancellationToken>());
        });
        Assert.Equal(AppliedState.Yes, change.Applied);
        Assert.Equal(MenuWrite.Written, change.Menu);
        Assert.Equal(DialogState.Answered, change.Dialog);
        Assert.Equal("Rescindir o contrato de Reserva?", change.DialogText);
        Assert.Null(change.Confirmed);
    }

    [Fact]
    public async Task Only_the_dialog_holding_the_confirmation_approved_in_the_dry_run_is_answered()
    {
        FreeRelease();
        Dialog(DialogState.Answered, "Liberar Reserva? " + Cost);

        await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: "  isso custará   R$120 MIL ao clube. ");

        await _editor.Received(1).AnswerDialogAsync("  isso custará   R$120 MIL ao clube. ", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Applying_a_release_without_the_dry_runs_confirmation_is_refused_before_the_save_is_touched(string? expected)
    {
        FreeRelease();

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: expected);

        Assert.Contains(result.Errors.OfType<ValidationError>(), error => error.PropertyName == "expectedConfirmation");
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_release_whose_confirmation_changed_since_the_dry_run_is_refused_and_nothing_is_sent()
    {
        FreeRelease();

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true,
            expectedConfirmation: "Isso custará R$90 mil ao clube.");

        Assert.Contains(Cost, Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_release_whose_menu_states_no_confirmation_is_refused_and_nothing_is_sent()
    {
        Offered(PlayerMenuAction.SetForRelease);
        Menu(PlayerMenuAction.SetForRelease, new MenuEntry { CanRun = true, Label = "Liberar no fim do contrato" });

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.EndOfContract, apply: true, confirmRelease: true, expectedConfirmation: "Reserva");

        Assert.True(result.HasError<ValidationError>());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_change_whose_menu_states_no_confirmation_never_has_its_dialog_answered()
    {
        Offered(PlayerMenuAction.DofContractRenewal);
        Menu(PlayerMenuAction.DofContractRenewal, new MenuEntry { CanRun = true, Label = "Pedir ao DoF para renovar" });
        Dialog(DialogState.NotAnswered, "Pedir ao DoF para renovar com Reserva?");

        var change = (await _service.HandRenewalToDofAsync(Reserve, apply: true)).Value;

        await _editor.Received(1).AnswerDialogAsync("", Arg.Any<CancellationToken>());
        Assert.Equal(AppliedState.Partial, change.Applied);
        Assert.Contains(change.Notes, note => note.Contains("answer it in the game"));
    }

    [Fact]
    public async Task A_dialog_that_isnt_the_actions_is_left_for_the_user_and_the_pick_is_reported_partial()
    {
        FreeRelease();
        Dialog(DialogState.NotAnswered, "Enviar a numeração atual do elenco?");

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: Cost);

        Assert.True(result.IsSuccess);
        Assert.Equal(AppliedState.Partial, result.Value.Applied);
        Assert.Equal(MenuWrite.Written, result.Value.Menu);
        Assert.Equal(DialogState.NotAnswered, result.Value.Dialog);
        Assert.Contains(result.Value.Notes, note => note.Contains("resolve it in the game"));
    }

    [Fact]
    public async Task A_dialog_whose_answer_the_game_didnt_confirm_is_partial_not_an_error()
    {
        FreeRelease();
        Dialog(DialogState.Unknown, "Liberar Reserva? Isso custará R$120 mil ao clube.");

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: Cost);

        Assert.Equal(AppliedState.Partial, result.Value.Applied);
        Assert.Equal(DialogState.Unknown, result.Value.Dialog);
    }

    [Fact]
    public async Task A_pick_the_game_didnt_answer_is_decided_by_the_read_back()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        _editor.ClickMenuEntryAsync(default, default, default!, default!).ReturnsForAnyArgs(Task.FromException(new GameAnswerLostException("timed out")));
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns(
            [Contract("Reserva")],
            [Contract("Rotação")]);

        var change = (await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true)).Value;

        Assert.Equal(MenuWrite.Written, change.Menu);
        Assert.Equal(AppliedState.Yes, change.Applied);
        Assert.True(change.Confirmed);
        await _editor.DidNotReceiveWithAnyArgs().AnswerDialogAsync(default!);
    }

    [Fact]
    public async Task A_pick_the_game_didnt_answer_and_the_read_back_doesnt_show_stays_unknown()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        _editor.ClickMenuEntryAsync(default, default, default!, default!).ReturnsForAnyArgs(Task.FromException(new GameAnswerLostException("timed out")));

        var result = await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(MenuWrite.Unknown, result.Value.Menu);
        Assert.Equal(AppliedState.Partial, result.Value.Applied);
        Assert.False(result.Value.Confirmed);
        await _editor.DidNotReceiveWithAnyArgs().AnswerDialogAsync(default!);
    }

    [Fact]
    public async Task A_status_he_already_has_is_refused_and_nothing_is_sent()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns([Contract("Rotação")]);

        var result = await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true);

        Assert.Contains("already", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_lost_pick_of_a_release_of_one_the_first_team_didnt_list_stays_unknown()
    {
        FreeRelease();
        _editor.ClickMenuEntryAsync(default, default, default!, default!).ReturnsForAnyArgs(Task.FromException(new GameAnswerLostException("timed out")));
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns(new List<SquadPlayerContract>());

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: Cost)).Value;

        Assert.Equal(MenuWrite.Unknown, change.Menu);
        Assert.Equal(AppliedState.Partial, change.Applied);
        Assert.Null(change.Confirmed);
    }

    [Fact]
    public async Task A_read_back_that_fails_after_the_send_is_unconfirmed_not_an_error()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        Dialog(DialogState.Answered, "Mudar o status de Reserva?");
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns(
            _ => [Contract("Reserva")],
            _ => throw new GameUnavailableException("busy"));

        var result = await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AppliedState.Yes, result.Value.Applied);
        Assert.Null(result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("reading it back failed"));
    }

    [Fact]
    public async Task A_free_release_with_him_still_in_the_squad_is_not_confirmed()
    {
        FreeRelease();
        Dialog(DialogState.Answered, "Liberar Reserva? Isso custará R$120 mil ao clube.");

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: Cost)).Value;

        Assert.True(change.StillInSquad);
        Assert.False(change.Confirmed);
        Assert.Contains(change.Notes, note => note.Contains("still lists him"));
    }

    [Fact]
    public async Task A_free_release_with_him_gone_is_confirmed()
    {
        FreeRelease();
        Dialog(DialogState.Answered, "Liberar Reserva? Isso custará R$120 mil ao clube.");
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns([Contract("Reserva")], new List<SquadPlayerContract>());

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: Cost)).Value;

        Assert.False(change.StillInSquad);
        Assert.True(change.Confirmed);
    }

    [Fact]
    public async Task A_refused_save_writes_nothing()
    {
        Offered(PlayerMenuAction.DofContractRenewal);
        Menu(PlayerMenuAction.DofContractRenewal, new MenuEntry { CanRun = true, Label = "Pedir ao DoF para renovar" });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>()).Returns(Result.Fail<SaveClearance>(new SaveRefusedError("real save")));

        var result = await _service.HandRenewalToDofAsync(Reserve, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task An_action_the_menu_doesnt_offer_is_a_clear_error_and_nothing_is_sent()
    {
        Offered(PlayerMenuAction.SetPlayingTime);

        var result = await _service.HandRenewalToDofAsync(Reserve, apply: true);

        Assert.Contains("doesn't offer", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
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
        Dialog(DialogState.Answered, "Mudar o status de Reserva?");
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns([Contract("Reserva")], [Contract("Rotação")]);

        var change = (await _service.SetSquadStatusAsync(Reserve, "rotação", apply: true)).Value;

        await _editor.Received(1).ClickMenuEntryAsync(Reserve, PlayerMenuAction.SetPlayingTime,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0, 1 })),
            Arg.Is<IReadOnlyList<string>>(labels => labels.SequenceEqual(new[] { "Status no elenco", "Rotação" })), null, Arg.Any<CancellationToken>());
        Assert.Equal("Reserva", change.AgreedPlayingTime);
        Assert.Equal("Rotação", change.AgreedPlayingTimeAfter);
        Assert.True(change.Confirmed);
        Assert.Equal(["Titular", "Rotação", "Reserva"], change.Choices);
    }

    // Without the status before, a lost pick's read back couldn't tell the change from a status he already had.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task A_status_change_on_an_unknown_current_status_is_refused_and_nothing_is_written(bool listed)
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>())
            .Returns(listed ? [new SquadPlayerContract { PlayerId = Reserve, Name = "Reserva" }] : new List<SquadPlayerContract>());

        var result = await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true);

        Assert.Contains(result.Errors.OfType<ValidationError>(), error => error.PropertyName == "status");
        Assert.Contains("current", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_status_read_back_other_than_the_one_asked_for_is_not_confirmed()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        StatusMenu();
        Dialog(DialogState.Answered, "Mudar o status de Reserva?");
        _contracts.GetContractsAsync(Arg.Any<CancellationToken>()).Returns([Contract("Reserva")], [Contract("Titular")]);

        var change = (await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true)).Value;

        Assert.False(change.Confirmed);
        Assert.Contains(change.Notes, note => note.Contains("'Titular', not 'Rotação'"));
    }

    [Fact]
    public async Task A_status_under_a_greyed_out_submenu_is_not_picked()
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        Menu(PlayerMenuAction.SetPlayingTime, new MenuEntry
        {
            Label = "Status no elenco",
            IsDisabled = true,
            Children = [new MenuEntry { CanRun = true, Label = "Titular" }, new MenuEntry { CanRun = true, Label = "Rotação" }]
        });

        var result = await _service.SetSquadStatusAsync(Reserve, "Rotação", apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
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
        Menu(PlayerMenuAction.SetForRelease, new MenuEntry { CanRun = true, Label = "Liberar no fim do contrato", IsDisabled = true });

        var result = await _service.ReleaseAsync(Reserve, ReleaseKind.EndOfContract, apply: false, confirmRelease: false);

        Assert.Contains("Liberar no fim do contrato", Messages(result));
    }

    // The game doesn't offer every status: some send no event (Event 0) and carry no confirmation. Seen on Andrey
    // Quintino in the test save, 18/11/2026: once "Jogador do elenco", "Jogador de rotação" and below came so.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_status_whose_entry_sends_no_event_is_refused_and_nothing_is_sent(bool apply)
    {
        Offered(PlayerMenuAction.SetPlayingTime);
        Menu(PlayerMenuAction.SetPlayingTime, new MenuEntry
        {
            Label = "Tempo de Jogo Acordado",
            Children =
            [
                new MenuEntry { CanRun = true, Label = "Titular Regular", Confirmation = "Alterar?" },
                new MenuEntry { CanRun = false, Label = "Jogador de rotação" }
            ]
        });

        var result = await _service.SetSquadStatusAsync(Reserve, "Jogador de rotação", apply: apply);

        Assert.True(result.IsFailed);
        Assert.Contains("Jogador de rotação (can't be picked)", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _editor.DidNotReceiveWithAnyArgs().ClickMenuEntryAsync(default, default, default!, default!);
    }

    // The game takes the hand-over without showing the dialog its menu states, and drops the entry from his menu
    // (Everton Cebolinha in the test save, 18/11/2026).
    [Fact]
    public async Task A_hand_over_his_menu_no_longer_offers_is_confirmed_without_a_dialog()
    {
        _editor.GetOfferedActionsAsync(Reserve, Arg.Any<CancellationToken>())
            .Returns([PlayerMenuAction.DofContractRenewal], new HashSet<PlayerMenuAction>());
        Menu(PlayerMenuAction.DofContractRenewal, new MenuEntry { CanRun = true, Label = "Dir. de futebol negocia contrato", Confirmation = "Tem certeza?" });
        Dialog(DialogState.NotShown, "");

        var change = (await _service.HandRenewalToDofAsync(Reserve, apply: true)).Value;

        Assert.True(change.Confirmed);
        Assert.Equal(AppliedState.Yes, change.Applied);
    }

    [Fact]
    public async Task A_hand_over_his_menu_still_offers_is_not_confirmed()
    {
        Offered(PlayerMenuAction.DofContractRenewal);
        Menu(PlayerMenuAction.DofContractRenewal, new MenuEntry { CanRun = true, Label = "Dir. de futebol negocia contrato", Confirmation = "Tem certeza?" });
        Dialog(DialogState.NotShown, "");

        var change = (await _service.HandRenewalToDofAsync(Reserve, apply: true)).Value;

        Assert.False(change.Confirmed);
        Assert.Equal(AppliedState.Partial, change.Applied);
    }

    // Andrey Quintino in the test save, 18/11/2026: no confirmation in the menu, no dialog, and the entry still offered.
    [Fact]
    public async Task A_change_the_read_back_denies_is_partial_even_without_a_dialog()
    {
        Offered(PlayerMenuAction.DofContractRenewal);
        Menu(PlayerMenuAction.DofContractRenewal, new MenuEntry { CanRun = true, Label = "Dir. de futebol negocia contrato" });
        Dialog(DialogState.NotShown, "");

        var change = (await _service.HandRenewalToDofAsync(Reserve, apply: true)).Value;

        Assert.False(change.Confirmed);
        Assert.Equal(AppliedState.Partial, change.Applied);
    }

    // The game takes a release without showing its dialog: it drops the release (and most entries) from his menu at once
    // and takes him off the squad only on its next day (Andrey Quintino in the test save, 18/11/2026).
    [Fact]
    public async Task A_release_his_menu_no_longer_offers_is_confirmed_while_he_is_still_listed()
    {
        _editor.GetOfferedActionsAsync(Reserve, Arg.Any<CancellationToken>())
            .Returns([PlayerMenuAction.ReleaseOnFreeTransfer], [PlayerMenuAction.ReleaseOnFreeTransfer], new HashSet<PlayerMenuAction>());
        Menu(PlayerMenuAction.ReleaseOnFreeTransfer,
            new MenuEntry { CanRun = true, Label = "Liberar por transferência livre", Confirmation = Cost });
        Dialog(DialogState.NotShown, "");

        var change = (await _service.ReleaseAsync(Reserve, ReleaseKind.Free, apply: true, confirmRelease: true, expectedConfirmation: Cost)).Value;

        Assert.True(change.StillInSquad);
        Assert.True(change.Confirmed);
        Assert.Equal(AppliedState.Yes, change.Applied);
        Assert.Contains(change.Notes, note => note.Contains("next day"));
    }

    private void FreeRelease()
    {
        Offered(PlayerMenuAction.ReleaseOnFreeTransfer);
        Menu(PlayerMenuAction.ReleaseOnFreeTransfer,
            new MenuEntry { CanRun = true, Label = "Liberar por transferência livre", Confirmation = Cost });
    }

    private void Dialog(DialogState state, string text)
    {
        _editor.AnswerDialogAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new DialogAnswer { State = state, Text = text });
    }

    private static SquadPlayerContract Contract(string playingTime)
    {
        return new SquadPlayerContract { PlayerId = Reserve, Name = "Reserva", AgreedPlayingTime = playingTime };
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
                new MenuEntry { CanRun = true, Label = "Titular" },
                new MenuEntry { CanRun = true, Label = "Rotação" },
                new MenuEntry { CanRun = true, Label = "Reserva", IsDisabled = true }
            ]
        });
    }

    private static string Messages<T>(Result<T> result)
    {
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
