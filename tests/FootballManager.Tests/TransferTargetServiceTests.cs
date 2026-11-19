using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Transfers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class TransferTargetServiceTests
{
    private const int Walter = 7;

    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly TransferTargetService _targets;

    public TransferTargetServiceTests()
    {
        _targets = new TransferTargetService(_editor, _saveGuard);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation" }));
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, ScoutedPlayer> { [Walter] = new() { Id = Walter, Name = "Walter", Club = "Mirassol" } });
    }

    [Fact]
    public async Task A_missing_action_is_refused_before_the_save_is_touched()
    {
        var result = await _targets.ChangeAsync(Walter, null, apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task The_dry_run_shows_the_entry_and_where_he_stands_without_writing()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        Menu(PlayerMenuAction.WithdrawOffer, "Retirar proposta");

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: false)).Value;

        Assert.Equal("Retirar proposta", change.Choice);
        Assert.True(change.OfferPending);
        Assert.False(change.Applied);
        Assert.Equal(WriteState.NotWritten, change.Write);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Applying_goes_through_the_save_guard_then_runs_the_entry_and_confirms_it_by_the_menu()
    {
        Offered(PlayerMenuAction.MakeOffer, PlayerMenuAction.MarkDofTarget);
        Menu(PlayerMenuAction.MarkDofTarget, "Adicionar como alvo de transferência");
        _editor.RunMenuEntryAsync(Walter, PlayerMenuAction.MarkDofTarget, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                Offered(PlayerMenuAction.MakeOffer, PlayerMenuAction.UnmarkDofTarget);
                return (ScoutedPlayer?)null;
            });

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.MarkDofTarget, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _editor.RunMenuEntryAsync(Walter, PlayerMenuAction.MarkDofTarget, Arg.Is<IReadOnlyList<int>>(path => path.SequenceEqual(new[] { 0 })), Arg.Any<CancellationToken>());
        });
        Assert.True(change.Applied);
        Assert.Equal(WriteState.Written, change.Write);
        Assert.True(change.Confirmed);
        Assert.True(change.IsDofTarget);
    }

    [Fact]
    public async Task An_action_his_menu_does_not_offer_is_refused_with_nothing_sent()
    {
        Offered(PlayerMenuAction.MakeOffer);

        var result = await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true);

        Assert.True(result.IsFailed);
        Assert.Contains(Reasons(result), reason => reason.Contains("doesn't offer"));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task An_entry_the_game_greys_out_is_refused()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        _editor.GetMenuAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<CancellationToken>())
            .Returns([new MenuEntry { Label = "Retirar proposta", CanRun = false }]);

        var result = await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Nothing_is_sent_when_the_menu_changed_between_the_plan_and_the_send()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        _editor.GetMenuAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<CancellationToken>())
            .Returns(
                [new MenuEntry { Label = "Retirar proposta", CanRun = true }],
                [new MenuEntry { Label = "Retirar oferta de teste", CanRun = true }]);

        var result = await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task A_warning_from_the_game_is_refused_before_the_save_is_touched()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        _editor.GetMenuAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<CancellationToken>())
            .Returns([new MenuEntry { Label = "Retirar proposta", CanRun = true, Confirmation = "Isso irrita o clube.", ConfirmationIsWarning = true }]);

        var result = await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
    }

    [Fact]
    public async Task A_timeout_after_sending_is_decided_by_the_menu_read_back()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        Menu(PlayerMenuAction.WithdrawOffer, "Retirar proposta");
        _editor.RunMenuEntryAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns<ScoutedPlayer?>(_ =>
            {
                Offered(PlayerMenuAction.MakeOffer);
                throw new GameUnavailableException("timed out");
            });

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true)).Value;

        Assert.True(change.Applied);
        Assert.Equal(WriteState.Written, change.Write);
        Assert.True(change.Confirmed);
        Assert.False(change.OfferPending);
    }

    [Fact]
    public async Task A_read_back_that_fails_after_sending_still_reports_the_write_with_a_warning()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        Menu(PlayerMenuAction.WithdrawOffer, "Retirar proposta");
        _editor.RunMenuEntryAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                _editor.GetOfferedActionsAsync(Walter, Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("gone"));
                return (ScoutedPlayer?)null;
            });

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true)).Value;

        Assert.True(change.Applied);
        Assert.Equal(WriteState.Written, change.Write);
        Assert.Null(change.Confirmed);
        Assert.Contains(change.Notes, note => note.Contains("couldn't be read back"));
    }

    [Fact]
    public async Task A_menu_that_still_shows_him_as_before_is_not_confirmed()
    {
        Offered(PlayerMenuAction.UnmarkDofTarget);
        Menu(PlayerMenuAction.UnmarkDofTarget, "Remover dos alvos de transferência");

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.UnmarkDofTarget, apply: true)).Value;

        Assert.True(change.Applied);
        Assert.False(change.Confirmed);
        Assert.True(change.IsDofTarget);
        Assert.Contains(change.Notes, note => note.Contains("still shows"));
    }

    [Fact]
    public async Task A_refusal_on_sending_that_left_the_menu_as_it_was_is_not_a_write()
    {
        Offered(PlayerMenuAction.MarkDofTarget);
        Menu(PlayerMenuAction.MarkDofTarget, "Adicionar como alvo de transferência");
        _editor.RunMenuEntryAsync(Walter, PlayerMenuAction.MarkDofTarget, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new GameRejectedException("no"));

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.MarkDofTarget, apply: true)).Value;

        Assert.Equal(WriteState.NotWritten, change.Write);
        Assert.False(change.Confirmed);
        Assert.False(change.IsDofTarget);
    }

    [Fact]
    public async Task An_empty_menu_read_back_is_no_confirmation()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        Menu(PlayerMenuAction.WithdrawOffer, "Retirar proposta");
        _editor.RunMenuEntryAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                Offered();
                return (ScoutedPlayer?)null;
            });

        var change = (await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true)).Value;

        Assert.True(change.Applied);
        Assert.Null(change.Confirmed);
        Assert.True(change.OfferPending);
    }

    [Fact]
    public async Task Nothing_is_sent_when_the_game_turned_its_confirmation_into_a_warning_since_the_plan()
    {
        Offered(PlayerMenuAction.WithdrawOffer);
        _editor.GetMenuAsync(Walter, PlayerMenuAction.WithdrawOffer, Arg.Any<CancellationToken>())
            .Returns(
                [new MenuEntry { Label = "Retirar proposta", CanRun = true }],
                [new MenuEntry { Label = "Retirar proposta", CanRun = true, Confirmation = "Isso irrita o clube.", ConfirmationIsWarning = true }]);

        var result = await _targets.ChangeAsync(Walter, TransferTargetAction.WithdrawOffer, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task A_missing_player_is_refused_before_the_game_is_read()
    {
        var result = await _targets.ChangeAsync(null, TransferTargetAction.MarkDofTarget, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().FindPlayersAsync(default!);
    }

    [Fact]
    public async Task An_unknown_player_is_not_found()
    {
        var result = await _targets.ChangeAsync(99, TransferTargetAction.MarkDofTarget, apply: false);

        Assert.True(result.IsFailed);
        Assert.IsType<FootballManager.Application.ResultErrors.EntityDoesNotExistError>(result.Errors[0]);
    }

    private void Offered(params PlayerMenuAction[] actions)
    {
        _editor.GetOfferedActionsAsync(Walter, Arg.Any<CancellationToken>()).Returns(actions.ToHashSet());
    }

    private void Menu(PlayerMenuAction action, string label)
    {
        _editor.GetMenuAsync(Walter, action, Arg.Any<CancellationToken>())
            .Returns([new MenuEntry { Label = label, CanRun = true }]);
    }

    private static IEnumerable<string> Reasons<T>(Result<T> result)
    {
        return result.Errors.OfType<FootballManager.Application.ResultErrors.ValidationError>().SelectMany(error => error.ErrorMessages);
    }
}
