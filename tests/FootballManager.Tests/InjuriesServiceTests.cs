using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Injuries;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class InjuriesServiceTests
{
    private readonly IInjuriesGameClient _game = Substitute.For<IInjuriesGameClient>();
    private readonly InjuriesService _injuries;

    public InjuriesServiceTests()
    {
        _injuries = new InjuriesService(_game);
    }

    [Fact]
    public async Task Every_players_history_and_risk_come_back_with_the_season_summary()
    {
        MedicalCentre(
            Record(1, "Brazão", new PastInjury
            {
                Date = new DateOnly(2021, 7, 1), Injury = "A torn ACL", Duration = "18 meses", DurationDays = 547,
                Severity = InjurySeverity.Major
            }),
            Record(2, "Rodinei"));

        var centre = (await _injuries.GetInjuriesAsync(playerId: null)).Value;

        Assert.Equal([1, 2], centre.Players.Select(player => player.PlayerId));
        var acl = Assert.Single(centre.Players[0].History);
        Assert.Equal(("A torn ACL", 547, InjurySeverity.Major), (acl.Injury, acl.DurationDays, acl.Severity));
        Assert.Equal("Risk of 2", centre.Players[1].RiskOverview);
        Assert.Equal("Nenhum exausto e 1 lesão recorrente", centre.Season.ExhaustedAndRecurring);
        Assert.Equal("Previsão de retorno de 1 jogador", centre.Season.ExpectedReturnsNextMatch);
    }

    [Fact]
    public async Task One_player_only_keeps_the_season_summary()
    {
        MedicalCentre(Record(1, "Brazão"), Record(2, "Rodinei"));

        var centre = (await _injuries.GetInjuriesAsync(playerId: 2)).Value;

        Assert.Equal("Rodinei", Assert.Single(centre.Players).Name);
        Assert.Equal(21, centre.Season.InjuriesThisSeason);
    }

    [Fact]
    public async Task A_player_outside_the_first_team_isnt_found()
    {
        MedicalCentre(Record(1, "Brazão"));

        var result = await _injuries.GetInjuriesAsync(playerId: 9);

        Assert.IsType<EntityDoesNotExistError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetMedicalCentreAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _injuries.GetInjuriesAsync(playerId: null);

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void MedicalCentre(params PlayerMedicalRecord[] players)
    {
        _game.GetMedicalCentreAsync(Arg.Any<CancellationToken>()).Returns(new MedicalCentre
        {
            Players = [.. players],
            Season = new SeasonInjurySummary
            {
                InjuriesThisSeason = 21,
                ExhaustedAndRecurring = "Nenhum exausto e 1 lesão recorrente",
                ExpectedReturnsNextMatch = "Previsão de retorno de 1 jogador"
            }
        });
    }

    private static PlayerMedicalRecord Record(int id, string name, params PastInjury[] history)
    {
        return new PlayerMedicalRecord
        {
            PlayerId = id, Name = name, InjuriesThisSeason = history.Length, RiskLevel = 100 * id,
            RiskOverview = $"Risk of {id}", History = [.. history]
        };
    }
}
