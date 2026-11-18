using FluentResults;
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

        Assert.False(exit.Applied);
        Assert.Equal("Recall from loan", exit.Choice);
        Assert.Equal("Recall him?", exit.Confirmation);
        Assert.True(exit.Status.OutOnLoan);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync();
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
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
            _editor.RunMenuEntryAsync(1, PlayerMenuAction.RemoveFromTransferList,
                Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0 })), Arg.Any<CancellationToken>());
        });
        Assert.True(exit.Applied);
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
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
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
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task A_menu_of_statuses_lists_them_and_applies_the_one_named()
    {
        Player(1);
        Offered(1, PlayerMenuAction.SetTransferStatus);
        Menu(1, PlayerMenuAction.SetTransferStatus, new MenuEntry
        {
            Label = "Transfer status",
            Children =
            [
                new MenuEntry { Label = "Transfer list", CanRun = true },
                new MenuEntry { Label = "Loan list", CanRun = true }
            ]
        });

        var plan = (await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: null, apply: false)).Value;
        var unnamed = await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: null, apply: true);
        await _exits.ChangeAsync(1, PlayerExitAction.SetTransferStatus, choice: "transfer status > loan list", apply: true);

        Assert.Equal(["Transfer status > Transfer list", "Transfer status > Loan list"], plan.Choices);
        Assert.Null(plan.Choice);
        Assert.True(unnamed.IsFailed);
        await _editor.Received(1).RunMenuEntryAsync(1, PlayerMenuAction.SetTransferStatus,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0, 1 })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_entry_that_opens_a_screen_of_its_own_is_not_run()
    {
        Player(1);
        Offered(1, PlayerMenuAction.OfferToClubs);
        Menu(1, PlayerMenuAction.OfferToClubs, new MenuEntry { Label = "Offer to clubs", CanRun = false });

        var result = await _exits.ChangeAsync(1, PlayerExitAction.OfferToClubs, choice: null, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task A_player_no_list_holds_is_not_found()
    {
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<int, ScoutedPlayer>());

        var result = await _exits.ChangeAsync(9, PlayerExitAction.Recall, choice: null, apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    private static HashSet<PlayerMenuAction> Set(params PlayerMenuAction[] actions)
    {
        return actions.ToHashSet();
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
