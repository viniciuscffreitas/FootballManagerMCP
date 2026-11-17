using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
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

    [Fact]
    public async Task A_filtered_search_reads_the_whole_search_and_says_it_is_complete()
    {
        Returns(Player(1, "Série A", 1800, 7.1), Player(2, "Série A", 300, 7.5));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter { MinMinutes = 1000 });

        await _game.Received().GetPlayersAsync(
            ScoutingSource.Search, null, Arg.Any<FocusPool>(), Arg.Any<IReadOnlyCollection<AttributeDefinition>>(),
            Arg.Any<bool>(), Arg.Any<bool>(), ScoutingService.MaxScan, true, Arg.Any<CancellationToken>());
        Assert.True(result.Value.Complete);
    }

    [Fact]
    public async Task A_scan_shorter_than_the_search_says_it_is_not_complete()
    {
        Returns(total: 1420, Player(1, "Série A", 1800, 7.1));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter { MinMinutes = 1000 }, scan: 1);

        Assert.False(result.Value.Complete);
        Assert.Equal(1, result.Value.Scanned);
    }

    [Fact]
    public async Task The_leagues_read_are_counted_and_a_league_no_one_plays_in_is_named()
    {
        Returns(
            Player(1, "Liga Profesional", 1800, 7.1),
            Player(2, "Liga Profesional", 1900, 7.0),
            Player(3, "1ª División", 1500, 7.2),
            Player(4, null, null, null));

        var result = await _service.GetPlayersAsync(
            ScoutingSource.Search, filter: new PerformanceFilter { Leagues = ["liga profesional", "Primera División"] });

        Assert.Equal(new Dictionary<string, int> { ["Liga Profesional"] = 2, ["1ª División"] = 1, ["(no league)"] = 1 }, result.Value.Leagues);
        Assert.Equal(["Primera División"], result.Value.UnmatchedLeagues);
        Assert.Equal([1, 2], result.Value.Players.Select(p => p.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task A_league_named_with_its_nation_keeps_only_that_nations_league()
    {
        Returns(
            Player(1, "Primera B", 1800, 7.1, nation: "Chile"),
            Player(2, "Primera B", 1800, 7.1, nation: "Colombia"));

        var result = await _service.GetPlayersAsync(
            ScoutingSource.Search, filter: new PerformanceFilter { Leagues = ["primera b (chile)"] });

        Assert.Equal(1, Assert.Single(result.Value.Players).Id);
        Assert.Equal(new Dictionary<string, int> { ["Primera B (Chile)"] = 1, ["Primera B (Colombia)"] = 1 }, result.Value.Leagues);
        Assert.Empty(result.Value.UnmatchedLeagues!);
    }

    [Fact]
    public async Task Without_a_league_filter_no_league_is_unmatched()
    {
        Returns(Player(1, "Série A", 1800, 7.1));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter { MinMinutes = 1000 });

        Assert.Null(result.Value.UnmatchedLeagues);
    }

    [Fact]
    public async Task Minimums_and_maximums_cut_on_any_number_by_its_name()
    {
        Returns(
            Player(1, "Série A", 1800, 7.1, p => { p.GoalsPer90 = 0.5; p.MistakesLeadingToGoal = 0; }),
            Player(2, "Série A", 1800, 7.1, p => { p.GoalsPer90 = 0.2; p.MistakesLeadingToGoal = 0; }),
            Player(3, "Série A", 1800, 7.1, p => { p.GoalsPer90 = 0.6; p.MistakesLeadingToGoal = 2; }),
            Player(4, "Série A", 1800, 7.1, p => p.MistakesLeadingToGoal = 0));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter
        {
            Minimums = new() { ["goalsPer90"] = 0.33 },
            Maximums = new() { ["MISTAKESLEADINGTOGOAL"] = 0 }
        });

        Assert.Equal(1, Assert.Single(result.Value.Players).Id);
    }

    [Fact]
    public async Task A_bound_compares_the_number_as_the_answer_shows_it()
    {
        Returns(Player(1, "Série A", 1800, 6.8951, p => p.GoalsPer90 = 0.3296));

        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter
        {
            MinAverageRating = 6.9,
            Minimums = new() { ["goalsPer90"] = 0.33 }
        });

        Assert.Single(result.Value.Players);
    }

    [Fact]
    public async Task An_unknown_number_is_refused_with_the_names()
    {
        var result = await _service.GetPlayersAsync(ScoutingSource.Search, filter: new PerformanceFilter
        {
            Minimums = new() { ["golsPor90"] = 0.33 }
        });

        Assert.True(result.IsFailed);
        var error = Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        Assert.Contains("goalsPer90", Assert.Single(error.ErrorMessages));
        await _game.DidNotReceiveWithAnyArgs().GetPlayersAsync(default, default, default, default!, default, default);
    }

    private void Returns(params ScoutedPlayer[] players)
    {
        Returns(players.Length, players);
    }

    private void Returns(int total, params ScoutedPlayer[] players)
    {
        _game.GetPlayersAsync(
                Arg.Any<ScoutingSource>(), Arg.Any<int?>(), Arg.Any<FocusPool>(), Arg.Any<IReadOnlyCollection<AttributeDefinition>>(),
                Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new ScoutedPlayerPage { Players = players.ToList(), Total = total, Search = new PlayerSearchStatus() });
    }

    private static ScoutedPlayer Player(int id, string? league, int? minutes, double? rating, Action<PlayerPerformance>? numbers = null, string? nation = null)
    {
        var performance = minutes is null ? null : new PlayerPerformance { Minutes = minutes, AverageRating = rating };

        if (performance is not null)
        {
            numbers?.Invoke(performance);
        }

        return new ScoutedPlayer
        {
            Id = id,
            Name = $"Player {id}",
            League = league,
            LeagueNation = nation,
            Performance = performance
        };
    }
}
