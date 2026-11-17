using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Tactics.Editing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class TacticCreationTests
{
    private readonly ITacticEditorGameClient _game = Substitute.For<ITacticEditorGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly TacticEditingService _editing;

    // Slot 1 holds the tactic the team plays; slots 2 and 3 are empty.
    private List<int> _slots = [1];

    public TacticCreationTests()
    {
        _game.GetOptionsAsync(Arg.Any<CancellationToken>()).Returns(new TacticOptions
        {
            ActiveSlot = 1,
            Styles = [new TacticOption("Gegenpress", "Gegenpress"), new TacticOption("WingPlay", "Jogo pelas Pontas")],
            Formations = new ByPhase<List<TacticOption>>
            {
                InPossession = [new TacticOption("433DMWide", "4-3-3 VOL Com Pontas")],
                OutOfPossession = [new TacticOption("4141DM", "4-1-4-1 VOL")]
            }
        });
        _game.GetTacticAsync(Arg.Any<CancellationToken>()).Returns(_ => new EditableTactic { ActiveSlot = 1, Slots = _slots });
        _game.SetCreatorStyleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => new WriteOutcome(null, call.Arg<string>()));
        _game.SetCreatorFormationAsync(Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => new WriteOutcome(null, call.Arg<string>()));
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", Club = "Santos" }));
        _editing = new TacticEditingService(_game, _saveGuard);
    }

    [Fact]
    public async Task A_slot_holding_a_tactic_is_refused()
    {
        var result = await _editing.CreateTacticAsync(Tactic(slot: 1), apply: true);

        Assert.Contains("Slot 1 already holds a tactic", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SetCreatorStyleAsync(default!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task A_slot_is_one_to_three(int slot)
    {
        Assert.Contains("Must be 1 to 3", Messages(await _editing.CreateTacticAsync(Tactic(slot), apply: false)));
    }

    [Fact]
    public async Task Style_and_formations_must_be_ones_the_game_offers()
    {
        var tactic = Tactic();
        tactic.Style = "Tiki";
        tactic.Formation.OutOfPossession = "433DMWide";

        var messages = Messages(await _editing.CreateTacticAsync(tactic, apply: false));

        Assert.Contains("Unknown key 'Tiki'", messages);
        Assert.Contains("Unknown key '433DMWide'. Use one of: 4141DM", messages);
    }

    [Fact]
    public async Task The_creator_needs_a_style_and_both_formations()
    {
        var messages = Messages(await _editing.CreateTacticAsync(new NewTactic { Slot = 2 }, apply: false));

        Assert.Contains("Give the tactical style", messages);
        Assert.Contains("Give the formation in possession", messages);
        Assert.Contains("Give the formation out of possession", messages);
    }

    [Fact]
    public async Task A_dry_run_plans_the_creators_steps_and_writes_nothing()
    {
        var edit = (await _editing.CreateTacticAsync(Tactic(), apply: false)).Value;

        Assert.False(edit.Applied);
        Assert.Equal(["Open creator", "Style", "Formation", "Formation", "Create"], edit.Steps.Select(step => step.Action));
        Assert.All(edit.Steps, step => Assert.Equal(EditStepStatus.Planned, step.Status));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().StartCreatorAsync();
        await _game.DidNotReceiveWithAnyArgs().SetCreatorStyleAsync(default!);
        await _game.DidNotReceiveWithAnyArgs().ConfirmCreatorAsync();
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_sends_nothing_to_the_game()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("Another save is loaded.")));

        var result = await _editing.CreateTacticAsync(Tactic(), apply: true);

        Assert.Contains("Another save is loaded.", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().StartCreatorAsync();
        await _game.DidNotReceiveWithAnyArgs().SetCreatorStyleAsync(default!);
        await _game.DidNotReceiveWithAnyArgs().ConfirmCreatorAsync();
    }

    [Fact]
    public async Task An_apply_walks_the_creator_in_order_and_reads_the_new_slot_back()
    {
        _game.When(game => game.ConfirmCreatorAsync(Arg.Any<CancellationToken>())).Do(_ => _slots = [1, 2]);

        var edit = (await _editing.CreateTacticAsync(Tactic(), apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.StartCreatorAsync(Arg.Any<CancellationToken>());
            _game.SetCreatorStyleAsync("Gegenpress", Arg.Any<CancellationToken>());
            _game.NextCreatorStageAsync(Arg.Any<CancellationToken>());
            _game.SetCreatorFormationAsync(true, "433DMWide", Arg.Any<CancellationToken>());
            _game.NextCreatorStageAsync(Arg.Any<CancellationToken>());
            _game.SetCreatorFormationAsync(false, "4141DM", Arg.Any<CancellationToken>());
            _game.ConfirmCreatorAsync(Arg.Any<CancellationToken>());
        });
        Assert.True(edit.Applied);
        Assert.All(edit.Steps, step => Assert.Equal(EditStepStatus.Done, step.Status));
        await _game.DidNotReceiveWithAnyArgs().SwitchSlotAsync(default);
    }

    // The game won't switch to an empty slot: its creator fills the first empty one.
    [Fact]
    public async Task A_slot_after_an_empty_one_is_refused()
    {
        var result = await _editing.CreateTacticAsync(Tactic(slot: 3), apply: true);

        Assert.Contains("The game creates a new tactic in the first empty slot, slot 2", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().StartCreatorAsync();
    }

    // With no tactic at all, the game shows the creator for slot 1 on its own.
    [Fact]
    public async Task A_team_without_a_tactic_walks_the_creator_it_already_shows()
    {
        _slots = [];
        _game.When(game => game.ConfirmCreatorAsync(Arg.Any<CancellationToken>())).Do(_ => _slots = [1]);

        var edit = (await _editing.CreateTacticAsync(Tactic(slot: 1), apply: true)).Value;

        Assert.Equal(["Style", "Formation", "Formation", "Create"], edit.Steps.Select(step => step.Action));
        Assert.All(edit.Steps, step => Assert.Equal(EditStepStatus.Done, step.Status));
        await _game.DidNotReceiveWithAnyArgs().StartCreatorAsync();
    }

    [Fact]
    public async Task A_slot_still_empty_after_confirming_fails_the_create_step()
    {
        var edit = (await _editing.CreateTacticAsync(Tactic(), apply: true)).Value;

        var create = edit.Steps.Single(step => step.Action == "Create");
        Assert.Equal(EditStepStatus.Failed, create.Status);
        Assert.Contains("slot 2 still holds no tactic", create.Detail);
    }

    [Fact]
    public async Task A_step_the_game_refuses_stops_the_creator()
    {
        _game.NextCreatorStageAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameRejectedException("The creator didn't move on."));

        var edit = (await _editing.CreateTacticAsync(Tactic(), apply: true)).Value;

        Assert.Equal(EditStepStatus.Failed, edit.Steps.Single(step => step.Action == "Style").Status);
        Assert.All(edit.Steps.Where(step => step.Action is "Formation" or "Create"), step => Assert.Equal(EditStepStatus.Skipped, step.Status));
        await _game.DidNotReceiveWithAnyArgs().ConfirmCreatorAsync();
    }

    [Fact]
    public async Task A_creator_left_partway_is_not_written_over()
    {
        _game.GetCreatorStageAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _editing.CreateTacticAsync(Tactic(), apply: true);

        Assert.Contains("on step 2", Messages(result));
        await _game.DidNotReceiveWithAnyArgs().StartCreatorAsync();
        await _game.DidNotReceiveWithAnyArgs().SetCreatorStyleAsync(default!);
    }

    private static NewTactic Tactic(int slot = 2)
    {
        return new NewTactic
        {
            Slot = slot,
            Style = "Gegenpress",
            Formation = new ByPhase<string> { InPossession = "433DMWide", OutOfPossession = "4141DM" }
        };
    }

    private static string Messages(Result<TacticEdit> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }
}
