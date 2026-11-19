using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Opposition;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Tactics;
using NSubstitute;

namespace FootballManager.Tests;

public class OppositionInstructionsServiceTests
{
    private const int Eggel = 101;
    private const int Fernandez = 102;

    private readonly IOppositionInstructionsGameClient _game = Substitute.For<IOppositionInstructionsGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly OppositionInstructionsService _service;

    public OppositionInstructionsServiceTests()
    {
        _game.GetAsync(Arg.Any<CancellationToken>()).Returns(_ => Instructions());
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _service = new OppositionInstructionsService(_game, _saveGuard);
    }

    [Fact]
    public async Task The_dry_run_lists_the_instructions_and_the_staff_suggestions_and_writes_nothing()
    {
        var change = (await _service.ChangeAsync(OppositionInstructionsAction.Set, "Marcelo Eggel", Marking("always"), apply: false)).Value;

        Assert.False(change.Applied);
        Assert.Equal(2, change.Current.Players.Count);
        Assert.Equal(2, change.Current.StaffSuggestions.Count);
        var write = Assert.Single(change.Plan);
        Assert.Equal((Eggel, OppositionInstruction.Marking, "none", "always"), (write.PlayerId, write.Instruction, write.From, write.To));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SetAsync(default, default, default!, default);
    }

    [Fact]
    public async Task Applying_clears_the_save_then_writes_and_confirms_by_the_read_back()
    {
        _game.GetAsync(Arg.Any<CancellationToken>()).Returns(Instructions(), Instructions(eggelMarking: "always"));

        var change = (await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", Marking("always"), apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.SetAsync(Eggel, OppositionInstruction.Marking, "always", Arg.Any<CancellationToken>());
        });
        Assert.True(change.Applied);
        Assert.True(Assert.Single(change.Plan).Confirmed);
        Assert.Equal("always", change.After!.Players.Single(row => row.PlayerId == Eggel).Marking);
        Assert.Contains(change.Notes, note => note.Contains("MCPFM investigation"));
    }

    [Fact]
    public async Task A_write_the_read_back_doesnt_show_is_flagged_not_confirmed()
    {
        var change = (await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", Marking("always"), apply: true)).Value;

        Assert.True(change.Applied);
        Assert.False(Assert.Single(change.Plan).Confirmed);
        Assert.Contains(change.Notes, note => note.Contains("read back"));
    }

    [Fact]
    public async Task A_read_back_the_game_doesnt_answer_still_reports_the_apply()
    {
        _game.GetAsync(Arg.Any<CancellationToken>()).Returns(_ => Instructions(), _ => throw new GameUnavailableException("timeout"));

        var result = await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", Marking("always"), apply: true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Applied);
        Assert.Null(result.Value.After);
        Assert.Null(Assert.Single(result.Value.Plan).Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("couldn't be read back"));
    }

    [Fact]
    public async Task A_request_without_an_action_is_refused_and_writes_nothing()
    {
        var result = await _service.ChangeAsync(null, $"{Eggel}", Marking("always"), apply: true);

        Assert.True(result.HasError<ValidationError>());
        await AssertNothingWritten();
    }

    [Fact]
    public async Task A_player_outside_the_expected_lineup_is_refused_and_writes_nothing()
    {
        var result = await _service.ChangeAsync(OppositionInstructionsAction.Set, "Neymar", Marking("always"), apply: true);

        var error = Assert.Single(result.Errors.OfType<ValidationError>());
        Assert.Contains("Marcelo Eggel", string.Join(" ", error.ErrorMessages));
        await AssertNothingWritten();
    }

    [Fact]
    public async Task A_value_the_instruction_doesnt_take_is_refused_and_writes_nothing()
    {
        var result = await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", Marking("hard"), apply: true);

        var error = Assert.Single(result.Errors.OfType<ValidationError>());
        Assert.Contains("always", string.Join(" ", error.ErrorMessages));
        await AssertNothingWritten();
    }

    [Fact]
    public async Task Set_without_any_instruction_is_refused()
    {
        var result = await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", null, apply: true);

        Assert.True(result.HasError<ValidationError>());
        await AssertNothingWritten();
    }

    [Fact]
    public async Task A_player_is_found_by_name_without_accents_or_case()
    {
        var change = (await _service.ChangeAsync(OppositionInstructionsAction.Set, "ronnie fernandez", Marking("always"), apply: false)).Value;

        Assert.Equal(Fernandez, Assert.Single(change.Plan).PlayerId);
    }

    [Fact]
    public async Task Clear_plans_every_instruction_the_player_has_back_to_none()
    {
        _game.GetAsync(Arg.Any<CancellationToken>()).Returns(_ => Instructions(eggelMarking: "always", eggelTackling: "hard"));

        var change = (await _service.ChangeAsync(OppositionInstructionsAction.Clear, $"{Eggel}", null, apply: false)).Value;

        Assert.Equal(
            [(OppositionInstruction.Marking, "none"), (OppositionInstruction.Tackling, "none")],
            change.Plan.Select(write => (write.Instruction, write.To)));
    }

    [Fact]
    public async Task Staff_advice_plans_the_suggestions_the_game_offers_and_lists_the_rest()
    {
        var change = (await _service.ChangeAsync(OppositionInstructionsAction.ApplyStaffAdvice, null, null, apply: false)).Value;

        var write = Assert.Single(change.Plan);
        Assert.Equal((Eggel, OppositionInstruction.Marking, "always"), (write.PlayerId, write.Instruction, write.To));
        Assert.Contains(change.Notes, note => note.Contains("Ronnie Fernández"));
    }

    [Fact]
    public async Task A_save_the_guard_refuses_writes_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>()).Returns(Result.Fail<SaveClearance>(new SaveRefusedError("wrong save")));

        var result = await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", Marking("always"), apply: true);

        Assert.True(result.IsFailed);
        await _game.DidNotReceiveWithAnyArgs().SetAsync(default, default, default!, default);
    }

    [Fact]
    public async Task Without_a_match_ahead_there_is_no_opponent_to_instruct()
    {
        _game.GetAsync(Arg.Any<CancellationToken>()).Returns((OppositionInstructions?)null);

        var result = await _service.ChangeAsync(OppositionInstructionsAction.Set, $"{Eggel}", Marking("always"), apply: false);

        Assert.True(result.HasError<EntityDoesNotExistError>());
    }

    private async Task AssertNothingWritten()
    {
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SetAsync(default, default, default!, default);
    }

    private static Dictionary<OppositionInstruction, string> Marking(string value)
    {
        return new Dictionary<OppositionInstruction, string> { [OppositionInstruction.Marking] = value };
    }

    private static OppositionInstructions Instructions(string eggelMarking = "none", string eggelTackling = "none")
    {
        return new OppositionInstructions
        {
            TeamId = 324,
            TeamName = "Grêmio",
            Players =
            [
                new OppositionPlayerInstructions
                {
                    PlayerId = Eggel, Name = "Marcelo Eggel", Position = new TacticalPosition(PitchPosition.AMC),
                    Marking = eggelMarking, Tackling = eggelTackling
                },
                new OppositionPlayerInstructions { PlayerId = Fernandez, Name = "Ronnie Fernández", Position = new TacticalPosition(PitchPosition.ST) }
            ],
            StaffSuggestions =
            [
                new OppositionStaffSuggestion
                {
                    PlayerId = Eggel, Name = "Marcelo Eggel", Text = "Marcar o Marcelo Eggel de perto",
                    Instruction = OppositionInstruction.Marking, Value = "always"
                },
                new OppositionStaffSuggestion { PlayerId = Fernandez, Name = "Ronnie Fernández", Text = "Algo novo sobre Ronnie Fernández" }
            ]
        };
    }
}
