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

public class RegistrationServiceTests
{
    private const string Cup = "Copa Betano do Brasil";
    private const int Neymar = 10;
    private const int Keeper = 20;
    private const int Youngster = 30;

    private readonly IRegistrationGameClient _game = Substitute.For<IRegistrationGameClient>();
    private readonly ISaveGuard _saveGuard = Substitute.For<ISaveGuard>();
    private readonly RegistrationService _registration;

    public RegistrationServiceTests()
    {
        _game.GetRegistrationAsync(Arg.Any<CancellationToken>()).Returns(_ => Registration());
        _game.CanChangeRegistrationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SaveClearance { SaveName = "MCPFM investigation", IsTestSave = true }));
        _registration = new RegistrationService(_game, _saveGuard);
    }

    // The Copa do Brasil as the game showed it (18/11/2026), scaled down: 3 registered of 4 at most, 2 keepers minimum.
    private static SquadRegistration Registration(bool open = true, int[]? registered = null)
    {
        var all = new List<RegistrationPlayer>
        {
            new() { Id = Neymar, Name = "Neymar", SquadNumber = 10 },
            new() { Id = Keeper, Name = "Diógenes", IsGoalkeeper = true },
            new() { Id = 21, Name = "Brazão", IsGoalkeeper = true },
            new() { Id = Youngster, Name = "João Fernandes" },
            new() { Id = 31, Name = "Rodrigo Falcão" }
        };
        var ids = registered ?? [Neymar, Keeper, 21];

        return new SquadRegistration
        {
            Competitions =
            [
                new CompetitionRegistration { Index = 0, Name = "Campeonato Brasileiro Série A Betano", Competition = "Brasileirão" },
                new CompetitionRegistration
                {
                    Index = 1, Name = Cup, Competition = "Copa do Brasil", WindowOpen = open, Window = open ? "Faltam 98 dias" : "Reabre em 77 dias",
                    Rules =
                    [
                        new RegistrationRule { Type = 9, Text = "Tamanho máximo do elenco de 4 jogadores", Value = 3, Display = "3/4 Máx.", Met = true },
                        new RegistrationRule { Type = 87, Text = "Mínimo de 2 goleiros no elenco", Value = 2, Display = "2/2 Mín.", Met = true },
                        new RegistrationRule { Type = 300, Text = "Máximo de 5 estrangeiros", Value = 0, Display = "0/5 Máx.", Met = true }
                    ],
                    Registered = all.Where(p => ids.Contains(p.Id)).ToList(),
                    Unregistered = all.Where(p => !ids.Contains(p.Id)).ToList()
                }
            ]
        };
    }

    [Fact]
    public async Task The_dry_run_checks_the_rules_and_writes_nothing()
    {
        var plan = (await _registration.ChangeRegistrationAsync("copa do brasil", [Youngster], null, apply: false)).Value;

        Assert.False(plan.Applied);
        Assert.Equal(Cup, plan.Competition);
        Assert.Equal("João Fernandes", Assert.Single(plan.Register).Name);
        var size = plan.Rules.Single(rule => rule.Rule.StartsWith("Tamanho"));
        Assert.Equal((3, 4, 4, false), (size.Before, size.After, size.Limit, size.Broken));
        Assert.Null(plan.Rules.Single(rule => rule.Rule.Contains("estrangeiros")).Broken);
        Assert.Empty(plan.Warnings);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
        await _game.DidNotReceiveWithAnyArgs().SetRegisteredAsync(default, default!, default, default);
        await _game.DidNotReceiveWithAnyArgs().ConfirmRegistrationAsync(default, default!);
    }

    [Fact]
    public async Task The_dry_run_warns_of_a_rule_the_change_breaks_and_apply_refuses_it()
    {
        var dryRun = (await _registration.ChangeRegistrationAsync(Cup, null, [Keeper], apply: false)).Value;
        var applied = await _registration.ChangeRegistrationAsync(Cup, null, [Keeper], apply: true);

        Assert.Contains(dryRun.Warnings, warning => warning.Contains("goleiros") && warning.Contains("1 against 2"));
        Assert.True(applied.IsFailed);
        await _game.DidNotReceiveWithAnyArgs().SetRegisteredAsync(default, default!, default, default);
    }

    [Fact]
    public async Task Too_many_players_break_the_squad_size()
    {
        var plan = (await _registration.ChangeRegistrationAsync(Cup, [Youngster, 31], null, apply: false)).Value;

        Assert.True(plan.Rules.Single(rule => rule.Rule.StartsWith("Tamanho")).Broken);
        Assert.Single(plan.Warnings);
    }

    [Fact]
    public async Task Applying_clears_the_save_then_ticks_each_player_confirms_and_reads_it_back()
    {
        _game.GetRegistrationAsync(Arg.Any<CancellationToken>())
            .Returns(Registration(), Registration(registered: [Keeper, 21, Youngster]));

        var plan = (await _registration.ChangeRegistrationAsync(Cup, [Youngster], [Neymar], apply: true)).Value;

        Received.InOrder(() =>
        {
            _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>());
            _game.SetRegisteredAsync(1, Cup, Youngster, true, Arg.Any<CancellationToken>());
            _game.SetRegisteredAsync(1, Cup, Neymar, false, Arg.Any<CancellationToken>());
            _game.ConfirmRegistrationAsync(1, Cup, Arg.Any<CancellationToken>());
        });
        Assert.True(plan.Applied);
        Assert.True(plan.Confirmed);
        Assert.Equal(RegistrationWrite.Written, plan.Writes[Youngster]);
        Assert.Equal(RegistrationWrite.Written, plan.Confirm);
        Assert.Contains(plan.Notes, note => note.Contains("MCPFM investigation"));
    }

    [Fact]
    public async Task A_read_back_that_differs_is_not_confirmed()
    {
        var plan = (await _registration.ChangeRegistrationAsync(Cup, [Youngster], null, apply: true)).Value;

        Assert.True(plan.Applied);
        Assert.False(plan.Confirmed);
        Assert.Contains(plan.Warnings, warning => warning.Contains("João Fernandes"));
    }

    [Fact]
    public async Task A_read_back_the_game_does_not_answer_still_reports_the_apply()
    {
        _game.GetRegistrationAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Registration(), _ => throw new GameUnavailableException("busy"));

        var result = await _registration.ChangeRegistrationAsync(Cup, [Youngster], null, apply: true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Applied);
        Assert.Null(result.Value.Confirmed);
        Assert.Contains(result.Value.Warnings, warning => warning.Contains("couldn't be read back"));
    }

    [Fact]
    public async Task A_write_that_fails_midway_reports_each_player_and_stops()
    {
        _game.SetRegisteredAsync(1, Cup, 31, true, Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("timeout"));

        var plan = (await _registration.ChangeRegistrationAsync(Cup, [Youngster, 31], [Neymar], apply: true)).Value;

        Assert.Equal(RegistrationWrite.Written, plan.Writes[Youngster]);
        Assert.Equal(RegistrationWrite.Unknown, plan.Writes[31]);
        Assert.Equal(RegistrationWrite.NotWritten, plan.Writes[Neymar]);
        Assert.True(plan.Applied);
        Assert.Contains(plan.Warnings, warning => warning.Contains("Rodrigo Falcão"));
        await _game.DidNotReceive().SetRegisteredAsync(1, Cup, Neymar, false, Arg.Any<CancellationToken>());
        await _game.DidNotReceiveWithAnyArgs().ConfirmRegistrationAsync(default, default!);
    }

    [Fact]
    public async Task A_save_the_guard_refuses_gets_nothing_written()
    {
        _saveGuard.PrepareWriteAsync(Arg.Any<CancellationToken>()).Returns(Result.Fail<SaveClearance>(new SaveRefusedError("not the save")));

        var result = await _registration.ChangeRegistrationAsync(Cup, [Youngster], null, apply: true);

        Assert.True(result.IsFailed);
        await _game.DidNotReceiveWithAnyArgs().SetRegisteredAsync(default, default!, default, default);
    }

    [Theory]
    [InlineData(null, new[] { Youngster }, null, "competition")]
    [InlineData(Cup, null, null, "players")]
    [InlineData("Libertadores", new[] { Youngster }, null, "competition")]
    [InlineData(Cup, new[] { Neymar }, null, "register")]
    [InlineData(Cup, new[] { 999 }, null, "register")]
    [InlineData(Cup, null, new[] { Youngster }, "unregister")]
    [InlineData(Cup, new[] { Youngster }, new[] { Youngster }, "players")]
    public async Task A_request_that_does_not_fit_the_registration_is_refused(string? competition, int[]? register, int[]? unregister, string property)
    {
        var result = await _registration.ChangeRegistrationAsync(
            competition, register?.Select(id => (int?)id).ToList(), unregister?.Select(id => (int?)id).ToList(), apply: true);

        Assert.Equal(property, Assert.IsType<ValidationError>(Assert.Single(result.Errors)).PropertyName);
        await _saveGuard.DidNotReceiveWithAnyArgs().PrepareWriteAsync(default);
    }

    [Fact]
    public async Task An_empty_item_in_a_list_is_refused()
    {
        var result = await _registration.ChangeRegistrationAsync(Cup, [Youngster, null], null, apply: false);

        Assert.IsType<ValidationError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task A_closed_window_or_a_player_the_game_locks_is_refused()
    {
        var closed = new RegistrationService(_game, _saveGuard);
        _game.GetRegistrationAsync(Arg.Any<CancellationToken>()).Returns(_ => Registration(open: false));
        var windowShut = await closed.ChangeRegistrationAsync(Cup, [Youngster], null, apply: false);

        _game.GetRegistrationAsync(Arg.Any<CancellationToken>()).Returns(_ => Registration());
        _game.CanChangeRegistrationAsync(1, Cup, Youngster, Arg.Any<CancellationToken>()).Returns(false);
        var locked = await _registration.ChangeRegistrationAsync(Cup, [Youngster], null, apply: false);

        Assert.Contains("closed", Assert.IsType<ValidationError>(Assert.Single(windowShut.Errors)).ErrorMessages.Single());
        Assert.Contains("doesn't let", Assert.IsType<ValidationError>(Assert.Single(locked.Errors)).ErrorMessages.Single());
    }

    [Fact]
    public async Task A_game_that_does_not_answer_is_unavailable()
    {
        _game.GetRegistrationAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("down"));

        var result = await _registration.GetRegistrationAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }
}
