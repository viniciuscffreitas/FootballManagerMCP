using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Transfers;
using NSubstitute;

namespace FootballManager.Tests;

public class PlayerExitServiceTests
{
    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly PlayerExitService _exits;

    public PlayerExitServiceTests()
    {
        _exits = new PlayerExitService(_editor, _saveGuard);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
    }

    [Fact]
    public async Task The_dry_run_plans_the_menu_entry_and_writes_nothing()
    {
        Player(1);
        Offered(1, PlayerMenuAction.RecallFromLoan);
        Menu(1, PlayerMenuAction.RecallFromLoan, new MenuEntry { Label = "Recall from loan", CanRun = true, Confirmation = "Recall him?" });

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.Recall, choice: null, apply: false)).Value;

        Assert.Equal(PlayerExitWrite.NotWritten, exit.Applied);
        Assert.Null(exit.Confirmed);
        Assert.Equal("Recall from loan", exit.Choice);
        Assert.Equal("Recall him?", exit.Confirmation);
        Assert.True(exit.Status.OutOnLoan);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task Applying_checks_the_save_first_then_sends_the_entry_and_reads_the_new_status()
    {
        Player(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(Set(PlayerMenuAction.RemoveFromTransferList), Set(PlayerMenuAction.SetTransferStatus));
        Menu(1, PlayerMenuAction.RemoveFromTransferList, new MenuEntry { Label = "Remove from transfer list", CanRun = true });

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.RemoveFromTransferList, choice: null, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _editor.SendMenuEntryAsync(1, PlayerMenuAction.RemoveFromTransferList,
                Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0 })),
                Arg.Is<IReadOnlyList<string>>(labels => labels.SequenceEqual(new[] { "Remove from transfer list" })), Arg.Any<CancellationToken>());
        });
        Assert.Equal(PlayerExitWrite.Written, exit.Applied);
        Assert.True(exit.Confirmed);
        Assert.False(exit.Status.TransferListed);
    }

    [Fact]
    public async Task A_save_the_guard_refuses_gets_no_write()
    {
        Player(1);
        Offered(1, PlayerMenuAction.RecallFromLoan);
        Menu(1, PlayerMenuAction.RecallFromLoan, new MenuEntry { Label = "Recall from loan", CanRun = true });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>()).Returns(Result.Fail<SaveClearance>("not the test save"));

        var result = await _exits.ChangeAsync(1, PlayerExitAction.Recall, choice: null, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task An_action_the_menu_doesnt_offer_is_a_clear_error_and_nothing_is_sent()
    {
        Player(1);
        Offered(1, PlayerMenuAction.SetTransferStatus);

        var result = await _exits.ChangeAsync(1, PlayerExitAction.Recall, choice: null, apply: true);

        Assert.True(result.IsFailed);
        Assert.Contains("doesn't offer a recall from loan", Assert.IsType<ValidationError>(result.Errors.Single()).ErrorMessages.Single());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_menu_of_statuses_lists_them_on_the_dry_run()
    {
        StatusMenu(1);

        var plan = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: null, apply: false)).Value;

        Assert.Equal(["Transfer status > Transfer list", "Transfer status > Loan list"], plan.Choices);
        Assert.Null(plan.Choice);
    }

    [Fact]
    public async Task Applying_a_menu_of_statuses_needs_a_choice()
    {
        StatusMenu(1);

        var result = await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: null, apply: true);

        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task Applying_a_menu_of_statuses_sends_the_one_named()
    {
        StatusMenu(1);

        await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "transfer status > loan list", apply: true);

        await _editor.Received(1).SendMenuEntryAsync(1, PlayerMenuAction.SetTransferStatus,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0, 1 })),
            Arg.Is<IReadOnlyList<string>>(labels => labels.SequenceEqual(new[] { "Transfer status", "Loan list" })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_entry_that_opens_a_screen_of_its_own_is_not_run()
    {
        Player(1);
        Offered(1, PlayerMenuAction.OfferToClubs);
        Menu(1, PlayerMenuAction.OfferToClubs, new MenuEntry { Label = "Offer to clubs", CanRun = false });

        var result = await _exits.ChangeAsync(1, PlayerExitAction.OfferToClubs, choice: null, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task An_entry_the_game_greys_out_is_refused_as_not_possible_now()
    {
        Player(1);
        Offered(1, PlayerMenuAction.RecallFromLoan);
        Menu(1, PlayerMenuAction.RecallFromLoan, new MenuEntry { Label = "Trazer do empréstimo", IsDisabled = true });

        var result = await _exits.ChangeAsync(1, PlayerExitAction.Recall, choice: null, apply: true);

        Assert.Contains("greys out", Assert.IsType<ValidationError>(result.Errors.Single()).ErrorMessages.Single());
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_submenu_the_game_greys_out_offers_none_of_its_entries()
    {
        Player(1);
        Offered(1, PlayerMenuAction.SetTransferStatus);
        Menu(1, PlayerMenuAction.SetTransferStatus,
            new MenuEntry { Label = "Transfer status", IsDisabled = true, Children = [new MenuEntry { Label = "Transfer list", CanRun = true }] },
            new MenuEntry { Label = "Loan status", Children = [new MenuEntry { Label = "Loan list", CanRun = true }] });

        var plan = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: null, apply: false)).Value;
        var result = await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "Transfer status > Transfer list", apply: true);

        Assert.Equal(["Loan status > Loan list"], plan.Choices);
        Assert.True(result.IsFailed);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_listing_the_menu_doesnt_show_after_the_send_is_not_confirmed()
    {
        StatusMenu(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(Set(PlayerMenuAction.SetTransferStatus), Set(PlayerMenuAction.SetTransferStatus, PlayerMenuAction.RemoveFromLoanList));

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "Transfer status > Transfer list", apply: true)).Value;

        Assert.Equal(PlayerExitWrite.Written, exit.Applied);
        Assert.False(exit.Confirmed);
        Assert.True(exit.Status.LoanListed);
        Assert.Contains(exit.Notes, note => note.StartsWith("Warning") && note.Contains("transfer list"));
    }

    [Fact]
    public async Task A_listing_is_confirmed_once_the_menu_shows_it()
    {
        StatusMenu(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(Set(PlayerMenuAction.SetTransferStatus), Set(PlayerMenuAction.SetTransferStatus, PlayerMenuAction.RemoveFromTransferList));

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "Transfer status > Transfer list", apply: true)).Value;

        Assert.True(exit.Confirmed);
        Assert.True(exit.Status.TransferListed);
    }

    [Fact]
    public async Task A_loan_extension_the_menu_cant_show_is_neither_confirmed_nor_denied()
    {
        Player(1);
        Offered(1, PlayerMenuAction.RecallFromLoan, PlayerMenuAction.ExtendLoan);
        Menu(1, PlayerMenuAction.ExtendLoan, new MenuEntry { Label = "Extend loan", CanRun = true });

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.ExtendLoan, choice: null, apply: true)).Value;

        Assert.Equal(PlayerExitWrite.Written, exit.Applied);
        Assert.Null(exit.Confirmed);
        Assert.Contains(exit.Notes, note => note.Contains("doesn't show"));
    }

    [Fact]
    public async Task A_lost_answer_is_decided_by_the_read_back()
    {
        Player(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(Set(PlayerMenuAction.RemoveFromTransferList), Set(PlayerMenuAction.SetTransferStatus));
        Menu(1, PlayerMenuAction.RemoveFromTransferList, new MenuEntry { Label = "Remove from transfer list", CanRun = true });
        LostAnswer();

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.RemoveFromTransferList, choice: null, apply: true)).Value;

        Assert.Equal(PlayerExitWrite.Written, exit.Applied);
        Assert.True(exit.Confirmed);
    }

    [Fact]
    public async Task A_lost_answer_with_a_failed_read_back_is_unknown()
    {
        Player(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(_ => Set(PlayerMenuAction.RemoveFromTransferList), _ => throw new GameUnavailableException("timeout"));
        Menu(1, PlayerMenuAction.RemoveFromTransferList, new MenuEntry { Label = "Remove from transfer list", CanRun = true });
        LostAnswer();

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.RemoveFromTransferList, choice: null, apply: true)).Value;

        Assert.Equal(PlayerExitWrite.Unknown, exit.Applied);
        Assert.Null(exit.Confirmed);
        Assert.Contains(exit.Notes, note => note.Contains("get_squad"));
    }

    [Fact]
    public async Task A_sent_entry_whose_read_back_fails_is_still_reported_written()
    {
        Player(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(_ => Set(PlayerMenuAction.RemoveFromTransferList), _ => throw new GameUnavailableException("timeout"));
        Menu(1, PlayerMenuAction.RemoveFromTransferList, new MenuEntry { Label = "Remove from transfer list", CanRun = true });

        var result = await _exits.ChangeAsync(1, PlayerExitAction.RemoveFromTransferList, choice: null, apply: true);

        Assert.Equal(PlayerExitWrite.Written, result.Value.Applied);
        Assert.Null(result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("get_squad"));
    }

    [Fact]
    public async Task A_game_that_fails_before_the_send_is_unavailable_and_nothing_was_written()
    {
        Player(1);
        Offered(1, PlayerMenuAction.RemoveFromTransferList);
        Menu(1, PlayerMenuAction.RemoveFromTransferList, new MenuEntry { Label = "Remove from transfer list", CanRun = true });
        _editor.SendMenuEntryAsync(default, default, default!, default!).ReturnsForAnyArgs(Task.FromException(new GameUnavailableException("down")));

        var result = await _exits.ChangeAsync(1, PlayerExitAction.RemoveFromTransferList, choice: null, apply: true);

        Assert.True(result.HasError<GameUnavailableError>());
    }

    [Fact]
    public async Task A_player_no_list_holds_is_not_found()
    {
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<int, ScoutedPlayer>());

        var result = await _exits.ChangeAsync(9, PlayerExitAction.Recall, choice: null, apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    [Fact]
    public async Task An_apply_without_an_action_is_refused_and_sends_nothing()
    {
        StatusMenu(1);

        var result = await _exits.ChangeAsync(1, null, choice: "Transfer status > Transfer list", apply: true);

        Assert.Contains("action", Assert.IsType<ValidationError>(result.Errors.Single()).ErrorMessages.Single());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_status_he_already_has_is_refused_on_the_plan_and_nothing_is_sent(bool apply)
    {
        StatusMenu(1);
        Offered(1, PlayerMenuAction.SetTransferStatus, PlayerMenuAction.RemoveFromTransferList);

        var result = await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "Transfer status > Transfer list", apply: apply);

        Assert.Contains("already", Assert.IsType<ValidationError>(result.Errors.Single()).ErrorMessages.Single());
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().SendMenuEntryAsync(default, default, default!, default!);
    }

    [Fact]
    public async Task A_lost_answer_the_read_back_doesnt_show_yet_is_unknown_not_unwritten()
    {
        StatusMenu(1);
        LostAnswer();

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "Transfer status > Transfer list", apply: true)).Value;

        Assert.Equal(PlayerExitWrite.Unknown, exit.Applied);
        Assert.False(exit.Confirmed);
        Assert.Contains(exit.Notes, note => note.StartsWith("Warning") && note.Contains("get_squad"));
    }

    [Theory]
    [InlineData("Situação de empréstimo > Sem situação definida")]
    [InlineData("Situação de empréstimo > Indisponível para empréstimo")]
    public async Task A_loan_status_the_menu_cant_tell_apart_is_neither_confirmed_nor_denied(string choice)
    {
        Player(1);
        Offered(1, PlayerMenuAction.SetTransferStatus);
        Menu(1, PlayerMenuAction.SetTransferStatus, new MenuEntry
        {
            Label = "Situação de empréstimo",
            Children =
            [
                new MenuEntry { Label = "Disponível para empréstimo", CanRun = true },
                new MenuEntry { Label = "Sem situação definida", CanRun = true },
                new MenuEntry { Label = "Indisponível para empréstimo", CanRun = true }
            ]
        });

        var exit = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: choice, apply: true)).Value;

        Assert.Equal(PlayerExitWrite.Written, exit.Applied);
        Assert.Null(exit.Confirmed);
        Assert.Contains(exit.Notes, note => note.StartsWith("Warning") && note.Contains("can't tell"));
    }

    [Fact]
    public async Task An_action_outside_the_list_is_refused_before_any_read()
    {
        var result = await _exits.ChangeAsync(1, (PlayerExitAction)999, choice: null, apply: true);

        Assert.Contains("action", Assert.IsType<ValidationError>(result.Errors.Single()).ErrorMessages.Single());
        await _editor.DidNotReceiveWithAnyArgs().FindPlayersAsync(default!);
    }

    [Fact]
    public async Task A_request_without_a_player_is_refused_before_any_read()
    {
        var result = await _exits.ChangeAsync(null, PlayerExitAction.Recall, choice: null, apply: true);

        Assert.Contains("playerId", Assert.IsType<ValidationError>(result.Errors.Single()).ErrorMessages.Single());
        await _editor.DidNotReceiveWithAnyArgs().FindPlayersAsync(default!);
    }

    private void LostAnswer()
    {
        _editor.SendMenuEntryAsync(default, default, default!, default!)
            .ReturnsForAnyArgs(Task.FromException(new GameAnswerLostException("The game didn't answer in time.")));
    }

    private static HashSet<PlayerMenuAction> Set(params PlayerMenuAction[] actions)
    {
        return actions.ToHashSet();
    }

    private void StatusMenu(int playerId)
    {
        Player(playerId);
        Offered(playerId, PlayerMenuAction.SetTransferStatus);
        Menu(playerId, PlayerMenuAction.SetTransferStatus, new MenuEntry
        {
            Label = "Transfer status",
            Children =
            [
                new MenuEntry { Label = "Transfer list", CanRun = true },
                new MenuEntry { Label = "Loan list", CanRun = true }
            ]
        });
    }

    private void Player(int id)
    {
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, ScoutedPlayer> { [id] = new() { Id = id, Name = $"Player {id}" } });
    }

    private void Offered(int playerId, params PlayerMenuAction[] actions)
    {
        _editor.GetOfferedActionsAsync(playerId, Arg.Any<CancellationToken>()).Returns(actions.ToHashSet());
    }

    private void Menu(int playerId, PlayerMenuAction action, params MenuEntry[] entries)
    {
        _editor.GetMenuAsync(playerId, action, Arg.Any<CancellationToken>()).Returns(entries.ToList());
    }
}
