using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Matches;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class LiveMatchServiceTests
{
    private const int Santos = 335;
    private const int Mirassol = 318;

    private readonly IMatchesGameClient _game = Substitute.For<IMatchesGameClient>();
    private readonly MatchesService _matches;

    public LiveMatchServiceTests()
    {
        _matches = new MatchesService(_game);
    }

    [Fact]
    public async Task Out_of_a_match_it_says_no_match_is_in_progress_without_an_error()
    {
        _game.GetLiveMatchAsync(Arg.Any<CancellationToken>()).Returns((LiveMatchReadout?)null);

        var result = await _matches.GetLiveMatchAsync();

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.InProgress);
        Assert.Equal("nenhuma partida em andamento", result.Value.Message);
        Assert.Empty(result.Value.OnPitch);
    }

    [Fact]
    public async Task A_match_in_progress_reads_from_the_users_side()
    {
        _game.GetLiveMatchAsync(Arg.Any<CancellationToken>()).Returns(Readout(home: Mirassol, away: Santos, homeGoals: 1, awayGoals: 2));

        var match = (await _matches.GetLiveMatchAsync()).Value;

        Assert.True(match.InProgress);
        Assert.Null(match.Message);
        Assert.Equal(67, match.Minute);
        Assert.Equal(Venue.Away, match.Venue);
        Assert.Equal("Mirassol", match.Opponent);
        Assert.Equal((2, 1), (match.GoalsFor, match.GoalsAgainst));
        Assert.Equal((1.4, 0.6), (match.TeamStats!.ExpectedGoals, match.OpponentStats!.ExpectedGoals));
        Assert.Equal(("4-2-3-1", "Cautelosa"), (match.Tactic!.Formation, match.Tactic.Mentality));
    }

    [Fact]
    public async Task The_pitch_lists_who_is_on_it_now_in_lineup_order()
    {
        _game.GetLiveMatchAsync(Arg.Any<CancellationToken>()).Returns(Readout(players:
        [
            new LivePlayer { PlayerId = 9, Name = "Striker", Slot = 10, Rating = 7.4, Condition = 71, Goals = 1 },
            new LivePlayer { PlayerId = 1, Name = "Keeper", Slot = 0, Rating = 6.8, Condition = 98 },
            new LivePlayer { PlayerId = 7, Name = "Winger", Slot = -1, Condition = 64, MinuteOff = 60 },
            new LivePlayer { PlayerId = 20, Name = "Sub", Slot = 8, Condition = 100, MinuteOn = 60, YellowCard = true }
        ]));

        var match = (await _matches.GetLiveMatchAsync()).Value;

        Assert.Equal(["Keeper", "Sub", "Striker"], match.OnPitch.Select(player => player.Name));
        Assert.Equal((71, 7.4, 1), (match.OnPitch[2].Condition, match.OnPitch[2].Rating, match.OnPitch[2].Goals));
        Assert.True(match.OnPitch[1].YellowCard);
    }

    [Fact]
    public async Task A_substitution_pairs_who_came_on_with_who_went_off_that_minute()
    {
        _game.GetLiveMatchAsync(Arg.Any<CancellationToken>()).Returns(Readout(players:
        [
            new LivePlayer { PlayerId = 7, Name = "Winger", Slot = -1, MinuteOff = 60 },
            new LivePlayer { PlayerId = 5, Name = "Back", Slot = -1, MinuteOff = 72, WentOffInjured = true },
            new LivePlayer { PlayerId = 20, Name = "Fresh winger", Slot = 8, MinuteOn = 60 },
            new LivePlayer { PlayerId = 21, Name = "Fresh back", Slot = 3, MinuteOn = 72 }
        ], allowed: 5));

        var match = (await _matches.GetLiveMatchAsync()).Value;

        Assert.Equal(
            [(60, "Winger", "Fresh winger", false), (72, "Back", "Fresh back", true)],
            match.Substitutions.Select(sub => (sub.Minute!.Value, sub.Off, sub.On, sub.Injury)));
        Assert.Equal(3, match.SubstitutionsRemaining);
    }

    [Fact]
    public async Task The_games_own_count_of_substitutions_left_wins()
    {
        _game.GetLiveMatchAsync(Arg.Any<CancellationToken>()).Returns(Readout(allowed: 5, remaining: 2));

        var match = (await _matches.GetLiveMatchAsync()).Value;

        Assert.Equal(2, match.SubstitutionsRemaining);
    }

    [Fact]
    public async Task A_closed_game_is_unavailable()
    {
        _game.GetLiveMatchAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("The game isn't running."));

        var result = await _matches.GetLiveMatchAsync();

        Assert.True(result.HasError<GameUnavailableError>());
    }

    private static LiveMatchReadout Readout(
        int home = Santos, int away = Mirassol, int homeGoals = 0, int awayGoals = 0,
        List<LivePlayer>? players = null, int? allowed = null, int? remaining = null)
    {
        var santosStats = new TeamMatchStats { ExpectedGoals = 1.4, Shots = 9, Possession = 55 };
        var mirassolStats = new TeamMatchStats { ExpectedGoals = 0.6, Shots = 4, Possession = 45 };

        return new LiveMatchReadout
        {
            Minute = 67,
            Clock = "67:12",
            Competition = "Brasileirão",
            TeamId = Santos,
            Home = new MatchSide { TeamId = home, TeamName = home == Santos ? "Santos" : "Mirassol", Goals = homeGoals },
            Away = new MatchSide { TeamId = away, TeamName = away == Santos ? "Santos" : "Mirassol", Goals = awayGoals },
            HomeStats = home == Santos ? santosStats : mirassolStats,
            AwayStats = away == Santos ? santosStats : mirassolStats,
            Players = players ?? [],
            Tactic = new LiveTactic { Name = "4-2-3-1 Contra-ataque", Formation = "4-2-3-1", Mentality = "Cautelosa" },
            SubstitutionsAllowed = allowed,
            SubstitutionsRemaining = remaining
        };
    }
}
