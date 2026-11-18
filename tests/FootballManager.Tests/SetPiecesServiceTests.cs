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
    private const string Penalties = "Penalties";

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
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
    }

    private static SetPieceSetup Setup()
    {
        return new SetPieceSetup
        {
            Captain = Neymar,
            ViceCaptain = Gabriel,
            TakerLists = [new SetPieceTakerList { Key = Penalties, Label = "Pênaltis", Takers = [Neymar] }],
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

        Assert.False(edit.Applied);
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
        Assert.True(edit.Applied);
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
    public async Task A_takers_dry_run_shows_the_order_and_writes_nothing()
    {
        var edit = (await _setPieces.SetTakersAsync(Penalties, [Rollheiser.Id, Neymar.Id], apply: false)).Value;

        Assert.False(edit.Applied);
        Assert.Contains(edit.Steps, step => step.Contains("Agustín Rollheiser, Neymar"));
        await _game.DidNotReceiveWithAnyArgs().SetTakersAsync(default!, default!);
    }

    [Fact]
    public async Task A_takers_apply_clears_the_save_then_writes()
    {
        await _setPieces.SetTakersAsync(Penalties, [Rollheiser.Id], apply: true);

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.SetTakersAsync(Penalties, Arg.Is<IReadOnlyList<int>>(ids => ids.SequenceEqual(new[] { Rollheiser.Id })), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task A_list_the_screen_does_not_have_is_refused_with_the_ones_it_has()
    {
        var result = await _setPieces.SetTakersAsync("Throw-ins", [Rollheiser.Id], apply: true);

        Assert.Contains(Penalties, Messages(result));
        await _game.DidNotReceiveWithAnyArgs().SetTakersAsync(default!, default!);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task Takers_need_first_team_players_named_once()
    {
        var unknown = await _setPieces.SetTakersAsync(Penalties, [999], apply: true);
        var twice = await _setPieces.SetTakersAsync(Penalties, [Neymar.Id, Neymar.Id], apply: true);
        var none = await _setPieces.SetTakersAsync(Penalties, [], apply: true);

        Assert.True(unknown.HasError<EntityDoesNotExistError>());
        Assert.True(twice.HasError<ValidationError>());
        Assert.True(none.HasError<ValidationError>());
        await _game.DidNotReceiveWithAnyArgs().SetTakersAsync(default!, default!);
    }

    [Fact]
    public async Task Asking_the_staff_is_a_dry_run_unless_applied()
    {
        var dry = (await _setPieces.AskStaffAsync(apply: false)).Value;
        await _game.DidNotReceiveWithAnyArgs().AskStaffToAssignTakersAsync(default);

        var applied = (await _setPieces.AskStaffAsync(apply: true)).Value;

        Assert.False(dry.Applied);
        Assert.True(applied.Applied);
        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.AskStaffToAssignTakersAsync(Arg.Any<CancellationToken>());
        });
    }

    private static string Messages<T>(Result<T> result)
    {
        return string.Join(" ", result.Errors.SelectMany(error =>
            error is ValidationError validation ? validation.ErrorMessages : [error.Message]));
    }
}
