using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Opposition;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Teams;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class OppositionServiceTests
{
    private const string Threat = "Deve ser um jogo equilibrado entre dois times de nível muito parecido.";

    private readonly ITeamsGameClient _teams = Substitute.For<ITeamsGameClient>();
    private readonly IOppositionGameClient _game = Substitute.For<IOppositionGameClient>();
    private readonly OppositionService _opposition;

    public OppositionServiceTests()
    {
        _opposition = new OppositionService(_teams, _game);
        _teams.GetTeamsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new KnownTeam(314, "Atlético Mineiro", "Brasileirão"),
            new KnownTeam(320, "Palmeiras", "Brasileirão")
        ]);
        _game.GetNextOpponentAsync(Arg.Any<CancellationToken>()).Returns(new NextOpponent(314, "Atlético Mineiro", Threat));
        _game.GetReportAsync(null, Arg.Any<CancellationToken>()).Returns(_ => Report(314, "Atlético Mineiro"));
        _game.GetReportAsync(320, Arg.Any<CancellationToken>()).Returns(_ => Report(320, "Palmeiras"));
    }

    [Fact]
    public async Task Without_a_team_it_reports_on_the_next_opponent_with_the_threat_level()
    {
        var report = (await _opposition.GetReportAsync(null)).Value;

        Assert.Equal(("Atlético Mineiro", true, Threat), (report.TeamName, report.IsNextOpponent, report.ThreatLevel));
        Assert.Equal("4-2-3-1 VOL MEA Com Pontas", report.Formation);
        var keeper = Assert.Single(report.ExpectedLineup);
        Assert.Equal(("Everson", "GK", "BallPlayingGoalkeeper", "Support"), (keeper.Name, keeper.Position?.Code, keeper.RoleKey, keeper.Duty));
    }

    [Fact]
    public async Task Another_team_of_the_users_competitions_gets_its_report_without_a_threat_level()
    {
        var report = (await _opposition.GetReportAsync("palmeiras")).Value;

        Assert.Equal((320, "Palmeiras", false, (string?)null), (report.TeamId, report.TeamName, report.IsNextOpponent, report.ThreatLevel));
    }

    [Fact]
    public async Task Naming_the_next_opponent_keeps_the_threat_level()
    {
        var report = (await _opposition.GetReportAsync("Atletico Mineiro")).Value;

        Assert.Equal(("Atlético Mineiro", true, Threat), (report.TeamName, report.IsNextOpponent, report.ThreatLevel));
    }

    [Fact]
    public async Task An_unknown_team_fails_with_the_close_ones()
    {
        var result = await _opposition.GetReportAsync("Flamengo");

        Assert.IsType<ValidationError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task No_match_ahead_and_no_team_named_has_nothing_to_report()
    {
        _game.GetNextOpponentAsync(Arg.Any<CancellationToken>()).Returns((NextOpponent?)null);

        var result = await _opposition.GetReportAsync(null);

        Assert.IsType<EntityDoesNotExistError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetNextOpponentAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _opposition.GetReportAsync(null);

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private static OppositionReport Report(int id, string name)
    {
        return new OppositionReport
        {
            TeamId = id,
            TeamName = name,
            Formation = "4-2-3-1 VOL MEA Com Pontas",
            TacticSummary = "Tendem a jogar num estilo equilibrado parecido, seja em casa ou fora.",
            ExpectedLineup =
            [
                new ExpectedPlayer
                {
                    PlayerId = 19045164, Name = "Everson", Position = new TacticalPosition(PitchPosition.GK),
                    RoleKey = "BallPlayingGoalkeeper", Duty = "Support"
                }
            ]
        };
    }
}
