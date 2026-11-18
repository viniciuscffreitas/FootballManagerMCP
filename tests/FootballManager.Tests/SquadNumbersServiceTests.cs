using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Registration;
using FootballManager.Domain.Saves;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class SquadNumbersServiceTests
{
    private const int Reserve = 2000125580;

    private readonly IRegistrationGameClient _game = Substitute.For<IRegistrationGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly SquadNumbersService _numbers;

    public SquadNumbersServiceTests()
    {
        _game.GetSquadNumberAsync(Reserve, Arg.Any<CancellationToken>()).Returns(_ => Player(null));
        _game.CountPlayersNeedingNumbersAsync(Arg.Any<CancellationToken>()).Returns(13, 0);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _numbers = new SquadNumbersService(_game, _saveGuard);
    }

    // João Fernandes as the squad numbers screen showed him (18/11/2026): no number, 2 and 13 free, 10 Neymar's.
    private static PlayerSquadNumber Player(int? number)
    {
        return new PlayerSquadNumber
        {
            Id = Reserve, Name = "João Fernandes", Number = number, CanChange = true, Suggested = 58,
            Options =
            [
                new SquadNumberOption { Number = 2, Label = "2" },
                new SquadNumberOption { Number = 10, Label = "10. Neymar", Taken = true },
                new SquadNumberOption { Number = 13, Label = "13" }
            ]
        };
    }

    [Fact]
    public async Task The_dry_run_plans_the_number_and_writes_nothing()
    {
        var plan = (await _numbers.ChangeNumberAsync(Reserve, 13, askAssistant: false, apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal(("João Fernandes", (int?)null, (int?)13), (plan.PlayerName, plan.From, plan.To));
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SetSquadNumberAsync(default, default);
    }

    [Fact]
    public async Task Applying_clears_the_save_then_sets_the_number_and_reads_it_back()
    {
        _game.GetSquadNumberAsync(Reserve, Arg.Any<CancellationToken>()).Returns(Player(null), Player(13));

        var plan = (await _numbers.ChangeNumberAsync(Reserve, 13, askAssistant: false, apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.SetSquadNumberAsync(Reserve, 13, Arg.Any<CancellationToken>());
        });
        Assert.True(plan.Applied);
        Assert.True(plan.Confirmed);
        Assert.Equal(RegistrationWrite.Written, plan.Write);
    }

    [Fact]
    public async Task A_number_the_read_back_does_not_show_is_not_confirmed()
    {
        var plan = (await _numbers.ChangeNumberAsync(Reserve, 13, askAssistant: false, apply: true)).Value;

        Assert.False(plan.Confirmed);
        Assert.NotEmpty(plan.Warnings);
    }

    [Fact]
    public async Task A_send_the_game_does_not_answer_is_unknown_and_the_read_back_decides()
    {
        _game.GetSquadNumberAsync(Reserve, Arg.Any<CancellationToken>()).Returns(Player(null), Player(13));
        _game.SetSquadNumberAsync(Reserve, 13, Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("timeout"));

        var plan = (await _numbers.ChangeNumberAsync(Reserve, 13, askAssistant: false, apply: true)).Value;

        Assert.Equal(RegistrationWrite.Unknown, plan.Write);
        Assert.True(plan.Confirmed);
    }

    [Fact]
    public async Task A_number_the_game_refuses_is_not_written()
    {
        _game.SetSquadNumberAsync(Reserve, 13, Arg.Any<CancellationToken>()).ThrowsAsync(new GameRejectedException("no"));

        var plan = (await _numbers.ChangeNumberAsync(Reserve, 13, askAssistant: false, apply: true)).Value;

        Assert.False(plan.Applied);
        Assert.Equal(RegistrationWrite.NotWritten, plan.Write);
    }

    [Fact]
    public async Task The_assistant_numbers_everyone_after_the_save_check()
    {
        var dryRun = (await new SquadNumbersService(_game, _saveGuard).ChangeNumberAsync(null, null, askAssistant: true, apply: false)).Value;
        await _game.DidNotReceiveWithAnyArgs().AskAssistantToNumberAsync(default);

        _game.CountPlayersNeedingNumbersAsync(Arg.Any<CancellationToken>()).Returns(13, 0);
        var plan = (await _numbers.ChangeNumberAsync(null, null, askAssistant: true, apply: true)).Value;

        Assert.Equal(13, dryRun.NeedingNumbersBefore);
        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.AskAssistantToNumberAsync(Arg.Any<CancellationToken>());
        });
        Assert.Equal((13, 0, true), (plan.NeedingNumbersBefore, plan.NeedingNumbersAfter, plan.Confirmed));
    }

    [Theory]
    [InlineData(null, null, false, "number")]
    [InlineData(Reserve, null, false, "number")]
    [InlineData(Reserve, 13, true, "askAssistant")]
    [InlineData(1, 13, false, "playerId")]
    [InlineData(Reserve, 10, false, "number")]
    [InlineData(Reserve, 100, false, "number")]
    public async Task A_request_that_does_not_fit_is_refused(int? playerId, int? number, bool askAssistant, string property)
    {
        var result = await _numbers.ChangeNumberAsync(playerId, number, askAssistant, apply: true);

        Assert.Equal(property, Assert.IsType<ValidationError>(Assert.Single(result.Errors)).PropertyName);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task The_number_he_already_wears_is_refused()
    {
        _game.GetSquadNumberAsync(Reserve, Arg.Any<CancellationToken>()).Returns(Player(13));

        var result = await _numbers.ChangeNumberAsync(Reserve, 13, askAssistant: false, apply: false);

        Assert.Contains("already", Assert.IsType<ValidationError>(Assert.Single(result.Errors)).ErrorMessages.Single());
    }
}
