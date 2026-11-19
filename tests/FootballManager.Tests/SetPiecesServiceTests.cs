using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Saves;
using FootballManager.Domain.SetPieces;
using NSubstitute;

namespace FootballManager.Tests;

public class SetPiecesServiceTests
{
    private static readonly SetPiecePlayer Neymar = new() { Id = 10, Name = "Neymar" };
    private static readonly SetPiecePlayer Gabriel = new() { Id = 20, Name = "Gabriel Brazão" };
    private static readonly SetPiecePlayer Rollheiser = new() { Id = 30, Name = "Agustín Rollheiser" };

    private readonly ISetPiecesGameClient _game = Substitute.For<ISetPiecesGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly SetPiecesService _setPieces;

    public SetPiecesServiceTests()
    {
        _setPieces = new SetPiecesService(_game, _saveGuard);
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(_ => Setup());
        _game.GetCaptaincyOfferAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CaptaincyOffer { MakeCaptain = true, MakeViceCaptain = true });
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
    }

    private static SetPieceSetup Setup(SetPiecePlayer? captain = null, SetPiecePlayer? vice = null)
    {
        return new SetPieceSetup
        {
            Captain = captain ?? Neymar,
            ViceCaptain = vice ?? Gabriel,
            TakerLists = [new SetPieceTakerList { Key = "Penalties", Label = "Penalties", Takers = [Neymar] }],
            Squad = [Neymar, Gabriel, Rollheiser]
        };
    }

    [Fact]
    public async Task Reads_the_captaincy_and_the_takers()
    {
        var setup = (await _setPieces.GetAsync()).Value;

        Assert.Equal(Neymar.Id, setup.Captain!.Id);
        Assert.Equal([Neymar.Id], setup.TakerLists.Single().Takers.Select(player => player.Id));
    }

    [Fact]
    public async Task The_game_not_answering_is_a_clear_error()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns<SetPieceSetup>(_ => throw new GameUnavailableException("down"));

        var result = await _setPieces.GetAsync();

        Assert.True(result.HasError<GameUnavailableError>());
    }

    [Fact]
    public async Task A_captain_dry_run_shows_the_change_and_writes_nothing()
    {
        var edit = (await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: false)).Value;

        Assert.Equal(WriteState.NotWritten, edit.Applied);
        Assert.Equal(Neymar.Id, edit.Before.Captain!.Id);
        Assert.Contains(edit.Steps, step => step.Contains("Agustín Rollheiser") && step.Contains("Neymar"));
        await _game.DidNotReceiveWithAnyArgs().MakeCaptainAsync(default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task A_captain_apply_clears_the_save_then_writes_and_reads_back()
    {
        var edit = (await _setPieces.SetCaptainsAsync(Rollheiser.Id, Neymar.Id, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.MakeCaptainAsync(Rollheiser.Id, Arg.Any<CancellationToken>());
            _game.MakeViceCaptainAsync(Neymar.Id, Arg.Any<CancellationToken>());
        });
        Assert.Equal(WriteState.Written, edit.Applied);
        Assert.NotNull(edit.After);
    }

    [Fact]
    public async Task An_apply_the_save_guard_refuses_writes_nothing()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SaveClearance>(new SaveRefusedError("not the test save")));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true);

        Assert.True(result.HasError<SaveRefusedError>());
        await _game.DidNotReceiveWithAnyArgs().MakeCaptainAsync(default);
    }

    [Fact]
    public async Task Naming_the_current_captain_writes_nothing()
    {
        var edit = (await _setPieces.SetCaptainsAsync(Neymar.Id, null, apply: true)).Value;

        Assert.Contains(edit.Notes, note => note.Contains("already"));
        await _game.DidNotReceiveWithAnyArgs().MakeCaptainAsync(default);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task A_player_outside_the_first_team_is_not_found()
    {
        var result = await _setPieces.SetCaptainsAsync(999, null, apply: true);

        Assert.True(result.HasError<EntityDoesNotExistError>());
        await _game.DidNotReceiveWithAnyArgs().MakeCaptainAsync(default);
    }

    [Theory]
    [InlineData(30, 30)]
    [InlineData(null, null)]
    public async Task Captain_and_vice_must_be_two_players(int? captainId, int? viceCaptainId)
    {
        var result = await _setPieces.SetCaptainsAsync(captainId, viceCaptainId, apply: true);

        Assert.True(result.HasError<ValidationError>());
        await _game.DidNotReceiveWithAnyArgs().ReadAsync(default);
    }

    [Fact]
    public async Task A_write_the_game_refuses_is_a_clear_error()
    {
        _game.MakeCaptainAsync(Rollheiser.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameRejectedException("The player's menu doesn't offer Make captain."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true);

        Assert.Contains("Make captain", Messages(result));
    }

    [Fact]
    public async Task A_role_the_players_menu_does_not_offer_is_refused_before_the_save_is_touched()
    {
        _game.GetCaptaincyOfferAsync(Rollheiser.Id, Arg.Any<CancellationToken>()).Returns(new CaptaincyOffer());

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true);

        Assert.Contains("doesn't offer", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().MakeCaptainAsync(default);
    }

    [Fact]
    public async Task The_dry_run_checks_the_players_menu_too()
    {
        _game.GetCaptaincyOfferAsync(Rollheiser.Id, Arg.Any<CancellationToken>()).Returns(new CaptaincyOffer());

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: false);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task An_apply_the_read_back_confirms_says_so()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup(Rollheiser, Neymar));

        var edit = (await _setPieces.SetCaptainsAsync(Rollheiser.Id, Neymar.Id, apply: true)).Value;

        Assert.True(edit.Confirmed);
        Assert.Equal(WriteState.Written, edit.CaptainWritten);
        Assert.Equal(WriteState.Written, edit.ViceCaptainWritten);
    }

    [Fact]
    public async Task A_vice_the_game_refuses_after_the_captain_went_through_reports_what_was_written()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup(Rollheiser, Gabriel));
        _game.MakeViceCaptainAsync(Neymar.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameRejectedException("The player's menu doesn't offer Make vice-captain."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, Neymar.Id, apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(WriteState.Written, result.Value.Applied);
        Assert.Equal(WriteState.Written, result.Value.CaptainWritten);
        Assert.Equal(WriteState.NotWritten, result.Value.ViceCaptainWritten);
        Assert.False(result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("vice-captain") && note.Contains("Make vice-captain"));
        Assert.Equal(Rollheiser.Id, result.Value.After!.Captain!.Id);
    }

    [Fact]
    public async Task A_read_back_that_fails_after_the_writes_still_reports_them_applied()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(
            _ => Setup(),
            _ => throw new GameUnavailableException("down"));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, Neymar.Id, apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(WriteState.Written, result.Value.Applied);
        Assert.Equal(WriteState.Written, result.Value.CaptainWritten);
        Assert.Equal(WriteState.Written, result.Value.ViceCaptainWritten);
        Assert.False(result.Value.Confirmed);
        Assert.Null(result.Value.After);
        Assert.Contains(result.Value.Notes, note => note.Contains("read"));
    }

    [Fact]
    public async Task A_read_back_that_shows_another_captain_is_not_confirmed()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup());

        var edit = (await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true)).Value;

        Assert.Equal(WriteState.Written, edit.Applied);
        Assert.False(edit.Confirmed);
        Assert.Contains(edit.Notes, note => note.Contains("Agustín Rollheiser") && note.Contains("Neymar"));
    }

    [Fact]
    public async Task A_read_back_where_the_role_left_out_changed_is_not_confirmed()
    {
        // Making the vice captain: the game may leave the vice role empty, which the call promised to keep.
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), new SetPieceSetup { Captain = Gabriel, Squad = [Neymar, Gabriel, Rollheiser] });

        var edit = (await _setPieces.SetCaptainsAsync(Gabriel.Id, null, apply: true)).Value;

        Assert.False(edit.Confirmed);
        Assert.Contains(edit.Notes, note => note.Contains("vice-captain") && note.Contains("Gabriel Brazão") && note.Contains("none"));
    }

    [Fact]
    public async Task Making_the_captain_vice_without_a_new_captain_is_refused_before_the_save_is_touched()
    {
        // The game would take the armband off him to make him vice, leaving the club without a captain.
        var result = await _setPieces.SetCaptainsAsync(null, Neymar.Id, apply: true);

        Assert.True(result.HasError<ValidationError>());
        Assert.Contains("captain", Messages(result));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().MakeViceCaptainAsync(default);
    }

    [Fact]
    public async Task A_lost_answer_the_read_back_shows_written_counts_as_written()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup(Rollheiser));
        _game.MakeCaptainAsync(Rollheiser.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameUnavailableException("The game didn't answer in time."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(WriteState.Written, result.Value.Applied);
        Assert.Equal(WriteState.Written, result.Value.CaptainWritten);
        Assert.True(result.Value.Confirmed);
    }

    [Fact]
    public async Task A_lost_answer_the_read_back_shows_unchanged_counts_as_not_written()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup());
        _game.MakeCaptainAsync(Rollheiser.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameUnavailableException("The game didn't answer in time."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(WriteState.NotWritten, result.Value.Applied);
        Assert.Equal(WriteState.NotWritten, result.Value.CaptainWritten);
        Assert.False(result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("captain") && note.Contains("not written"));
    }

    [Fact]
    public async Task A_lost_answer_and_a_failed_read_back_leave_the_write_unknown()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(
            _ => Setup(),
            _ => throw new GameUnavailableException("down"));
        _game.MakeCaptainAsync(Rollheiser.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameUnavailableException("The game didn't answer in time."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, null, apply: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(WriteState.Unknown, result.Value.Applied);
        Assert.Equal(WriteState.Unknown, result.Value.CaptainWritten);
        Assert.False(result.Value.Confirmed);
        Assert.Contains(result.Value.Notes, note => note.Contains("get_set_pieces"));
    }

    [Fact]
    public async Task A_lost_answer_on_the_vice_after_the_captain_is_decided_by_the_read_back()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup(Rollheiser, Gabriel));
        _game.MakeViceCaptainAsync(Neymar.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameUnavailableException("The game didn't answer in time."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, Neymar.Id, apply: true);

        Assert.Equal(WriteState.Written, result.Value.Applied);
        Assert.Equal(WriteState.Written, result.Value.CaptainWritten);
        Assert.Equal(WriteState.NotWritten, result.Value.ViceCaptainWritten);
        Assert.False(result.Value.Confirmed);
    }

    [Fact]
    public async Task No_write_is_sent_after_one_whose_answer_was_lost()
    {
        _game.ReadAsync(Arg.Any<CancellationToken>()).Returns(Setup(), Setup(Rollheiser));
        _game.MakeCaptainAsync(Rollheiser.Id, Arg.Any<CancellationToken>())
            .Returns(_ => throw new GameUnavailableException("The game didn't answer in time."));

        var result = await _setPieces.SetCaptainsAsync(Rollheiser.Id, Neymar.Id, apply: true);

        await _game.DidNotReceiveWithAnyArgs().MakeViceCaptainAsync(default);
        Assert.Equal(WriteState.NotWritten, result.Value.ViceCaptainWritten);
        Assert.Contains(result.Value.Notes, note => note.Contains("vice-captain") && note.Contains("not sent"));
    }

    private static string Messages<T>(Result<T> result)
    {
        return string.Join(" ", result.Errors.SelectMany(error =>
            error is ValidationError validation ? validation.ErrorMessages : [error.Message]));
    }
}
