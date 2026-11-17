using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Matches;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class FixturesServiceTests
{
    private const int Santos = 335;

    private readonly IMatchesGameClient _game = Substitute.For<IMatchesGameClient>();
    private readonly MatchesService _matches;

    public FixturesServiceTests()
    {
        _matches = new MatchesService(_game);
    }

    [Fact]
    public async Task The_next_matches_come_in_date_order_with_competition_venue_and_opponent()
    {
        Calendar(
            Fixture("2026-04-05", "Brasileirão", 314, "Atlético Mineiro", Santos, "Santos"),
            Fixture("2026-04-12", "Brasileirão", Santos, "Santos", 329, "Palmeiras"));

        var fixtures = (await _matches.GetUpcomingAsync()).Value;

        Assert.Equal("Santos", fixtures.TeamName);
        Assert.Equal(
            [(new DateOnly(2026, 4, 5), "Brasileirão", Venue.Away, "Atlético Mineiro"), (new DateOnly(2026, 4, 12), "Brasileirão", Venue.Home, "Palmeiras")],
            fixtures.Fixtures.Select(f => (f.Date, f.Competition, f.Venue, f.Them.TeamName)));
    }

    [Fact]
    public async Task Twenty_matches_are_asked_for_unless_told_otherwise()
    {
        Calendar();

        await _matches.GetUpcomingAsync();
        await _matches.GetUpcomingAsync(50);

        await _game.Received(1).GetUpcomingFixturesAsync(20, Arg.Any<CancellationToken>());
        await _game.Received(1).GetUpcomingFixturesAsync(50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Only_matches_still_to_play_are_listed_and_no_more_than_asked()
    {
        Calendar(
            Fixture("2026-03-29", "Brasileirão", Santos, "Santos", 322, "Flamengo", played: true),
            Fixture("2026-04-05", "Brasileirão", 314, "Atlético Mineiro", Santos, "Santos"),
            Fixture("2026-04-12", "Brasileirão", Santos, "Santos", 329, "Palmeiras"));

        var fixtures = (await _matches.GetUpcomingAsync(1)).Value;

        Assert.Equal("Atlético Mineiro", Assert.Single(fixtures.Fixtures).Them.TeamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task A_count_outside_one_to_fifty_is_refused_without_asking_the_game(int count)
    {
        var result = await _matches.GetUpcomingAsync(count);

        Assert.True(result.HasError<ValidationError>());
        await _game.DidNotReceiveWithAnyArgs().GetUpcomingFixturesAsync(default, default);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetUpcomingFixturesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _matches.GetUpcomingAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Calendar(params MatchSummary[] fixtures)
    {
        _game.GetUpcomingFixturesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TeamFixtures { TeamName = "Santos", Fixtures = [.. fixtures] });
    }

    private static MatchSummary Fixture(
        string date, string competition, int homeId, string home, int awayId, string away, bool played = false)
    {
        return new MatchSummary
        {
            Date = DateOnly.Parse(date),
            Competition = competition,
            IsPlayed = played,
            TeamId = Santos,
            Home = new MatchSide { TeamId = homeId, TeamName = home, Goals = played ? 1 : null },
            Away = new MatchSide { TeamId = awayId, TeamName = away, Goals = played ? 0 : null }
        };
    }
}
