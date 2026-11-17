using FootballManager.Application.Interfaces;
using FootballManager.Application.Services;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using NSubstitute;

namespace FootballManager.Tests;

public class PerformanceFilterTests
{
    private readonly IScoutingGameClient _game = Substitute.For<IScoutingGameClient>();
    private readonly IPlayerSearchGameClient _search = Substitute.For<IPlayerSearchGameClient>();
    private readonly ScoutingService _service;

    public PerformanceFilterTests()
    {
        _service = new ScoutingService(_game, _search);
        _search.GetFiltersAsync(Arg.Any<CancellationToken>()).Returns(new PlayerSearchFilters());
    }

    [Fact]
    public async Task Only_players_of_the_named_leagues_with_the_minutes_and_rating_are_kept()
    {
        Returns(
            Player(1, "Série A", minutes: 1800, rating: 7.1),
            Player(2, "Liga Profesional", minutes: 2100, rating: 7.0),
            Player(6, "Brasileirão Série B", minutes: 2000, rating: 7.2),
            Player(3, "Série A", minutes: 600, rating: 7.4),
            Player(4, "Série A", minutes: 1500, rating: 6.7),
            Player(5, "Premier League", minutes: 2500, rating: 7.3));

        var result = await _service.GetPlayersAsync(
            ScoutingSource.Search,
            filter: new PerformanceFilter { Leagues = ["serie a", "Liga Profesional"], MinMinutes = 1000, MinAverageRating = 6.9 });

        Assert.Equal([1, 2], result.Value.Players.Select(p => p.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task A_filtered_search_reads_the_scan_with_the_numbers_and_says_how_many_it_read()
    {
        Returns(Player(1, "Série A", 1800, 7.1), Player(2, "Série A", 1900, 7.2), Player(3, "Série A", 300, 7.5));

        var result = await _service.GetPlayersAsync(
            ScoutingSource.Search, limit: 1, filter: new PerformanceFilter { MinMinutes = 1000 }, scan: 500);

        await _game.Received().GetPlayersAsync(
            ScoutingSource.Search, null, Arg.Any<FocusPool>(), Arg.Any<IReadOnlyCollection<AttributeDefinition>>(),
            Arg.Any<bool>(), Arg.Any<bool>(), 500, true, Arg.Any<CancellationToken>());
        Assert.Single(result.Value.Players);
        Assert.Equal(3, result.Value.Scanned);
    }

    [Fact]
    public async Task A_player_without_numbers_comes_back_when_nothing_cuts_on_them()
    {
        Returns(Player(1, null, null, null));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, includeStats: true);

        Assert.Equal(1, Assert.Single(result.Value.Players).Id);
        Assert.Null(result.Value.Scanned);
    }

    [Fact]
    public async Task A_player_without_numbers_is_cut_by_a_minimum()
    {
        Returns(Player(1, null, null, null));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter { MinAverageRating = 6.9 });

        Assert.Empty(result.Value.Players);
    }

    [Fact]
    public async Task A_scan_without_a_filter_is_refused()
    {
        var result = await _service.GetPlayersAsync(ScoutingSource.Search, scan: 500);

        Assert.True(result.IsFailed);
    }

    private void Returns(params ScoutedPlayer[] players)
    {
        _game.GetPlayersAsync(
                Arg.Any<ScoutingSource>(), Arg.Any<int?>(), Arg.Any<FocusPool>(), Arg.Any<IReadOnlyCollection<AttributeDefinition>>(),
                Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new ScoutedPlayerPage { Players = players.ToList(), Total = players.Length, Search = new PlayerSearchStatus() });
    }

    private static ScoutedPlayer Player(int id, string? league, int? minutes, double? rating)
    {
        return new ScoutedPlayer
        {
            Id = id,
            Name = $"Player {id}",
            League = league,
            Performance = minutes is null ? null : new PlayerPerformance { Minutes = minutes, AverageRating = rating }
        };
    }
}
