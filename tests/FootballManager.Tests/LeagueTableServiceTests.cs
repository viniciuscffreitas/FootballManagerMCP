using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Standings;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class LeagueTableServiceTests
{
    private const int Santos = 335;

    private readonly ILeagueTableGameClient _game = Substitute.For<ILeagueTableGameClient>();
    private readonly LeagueTableService _service;

    public LeagueTableServiceTests()
    {
        _service = new LeagueTableService(_game);
    }

    [Fact]
    public async Task The_whole_table_comes_back_with_the_users_line_marked()
    {
        Readout(hasStage: true, Row(329, "Palmeiras", 1, 3, 4, 1), Row(Santos, "Santos", 2, 3, 3, 0), Row(300, "Bahia", 3, 0, 0, 3));

        var table = (await _service.GetLeagueTableAsync()).Value;

        Assert.Equal("Brasileirão", table.League);
        Assert.Equal("Santos", table.TeamName);
        Assert.Equal(2, table.Position);
        Assert.Equal(10, table.MediaExpectedPosition);
        Assert.Null(table.Reason);
        Assert.Equal(["Palmeiras", "Santos", "Bahia"], table.Rows.Select(row => row.TeamName));
        Assert.Equal([Santos], table.Rows.Where(row => row.IsUser).Select(row => row.TeamId));

        var santos = table.Rows[1];
        Assert.Equal((1, 1, 0, 0, 3, 0, 3, 3), (santos.Played, santos.Won, santos.Drawn, santos.Lost, santos.GoalsFor, santos.GoalsAgainst, santos.GoalDifference, santos.Points));
    }

    [Fact]
    public async Task Rows_come_back_top_of_the_table_first()
    {
        Readout(hasStage: true, Row(300, "Bahia", 3, 0, 0, 3), Row(329, "Palmeiras", 1, 3, 4, 1), Row(Santos, "Santos", 2, 3, 3, 0));

        var table = (await _service.GetLeagueTableAsync()).Value;

        Assert.Equal([1, 2, 3], table.Rows.Select(row => row.Position));
    }

    [Fact]
    public async Task A_league_without_a_table_stage_answers_empty_with_the_reason()
    {
        Readout(hasStage: false);

        var result = await _service.GetLeagueTableAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Rows);
        Assert.Null(result.Value.Position);
        Assert.Equal(10, result.Value.MediaExpectedPosition);
        Assert.NotNull(result.Value.Reason);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetLeagueTableAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _service.GetLeagueTableAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Readout(bool hasStage, params LeagueTableRow[] rows)
    {
        _game.GetLeagueTableAsync(Arg.Any<CancellationToken>()).Returns(new LeagueTableReadout
        {
            League = "Brasileirão",
            TeamId = Santos,
            TeamName = "Santos",
            MediaExpectedPosition = 10,
            HasLeagueStage = hasStage,
            Rows = [.. rows]
        });
    }

    private static LeagueTableRow Row(int id, string name, int position, int points, int goalsFor, int goalsAgainst)
    {
        var won = points / 3;
        return new LeagueTableRow
        {
            TeamId = id, TeamName = name, Position = position, Played = 1, Won = won, Lost = 1 - won, Points = points,
            GoalsFor = goalsFor, GoalsAgainst = goalsAgainst, GoalDifference = goalsFor - goalsAgainst
        };
    }
}
