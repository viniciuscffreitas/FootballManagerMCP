using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Matches;

namespace FootballManager.Application.Services;

public interface IMatchesService
{
    Task<Result<SeasonMatches>> GetMatchesAsync(bool includeFriendlies = false, CancellationToken cancellationToken = default);

    Task<Result<MatchDetail>> GetMatchAsync(string date, CancellationToken cancellationToken = default);

    Task<Result<LiveMatch>> GetLiveMatchAsync(CancellationToken cancellationToken = default);

    Task<Result<TeamFixtures>> GetUpcomingAsync(int count = MatchesService.DefaultUpcoming, CancellationToken cancellationToken = default);
}

public class MatchesService(IMatchesGameClient gameClient) : IMatchesService
{
    public const string DateFormat = "yyyy-MM-dd";
    public const int DefaultUpcoming = 20;
    public const int MaxUpcoming = 50;

    public async Task<Result<SeasonMatches>> GetMatchesAsync(
        bool includeFriendlies = false, CancellationToken cancellationToken = default)
    {
        TeamFixtures fixtures;

        try
        {
            fixtures = await gameClient.GetFixturesAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SeasonMatches>(new GameUnavailableError(exception.Message));
        }

        // Friendlies never count towards the record; they're only listed when asked for.
        var competitive = fixtures.Fixtures.Where(f => f.IsPlayed && !f.IsFriendly).ToList();

        return Result.Ok(new SeasonMatches
        {
            TeamName = fixtures.TeamName,
            Overall = MatchRecord.Of(competitive),
            Home = MatchRecord.Of(competitive.Where(f => f.Venue == Venue.Home)),
            Away = MatchRecord.Of(competitive.Where(f => f.Venue == Venue.Away)),
            Played = fixtures.Fixtures.Where(f => f.IsPlayed && (includeFriendlies || !f.IsFriendly)).ToList(),
            Upcoming = fixtures.Fixtures.Where(f => !f.IsPlayed).ToList()
        });
    }

    public async Task<Result<MatchDetail>> GetMatchAsync(string date, CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
        {
            return Result.Fail<MatchDetail>(new ValidationError(nameof(date), [$"Use the match date as {DateFormat}, e.g. 2026-10-11."]));
        }

        MatchDetail? match;

        try
        {
            match = await gameClient.GetMatchAsync(day, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<MatchDetail>(new GameUnavailableError(exception.Message));
        }

        return match is null ? Result.Fail<MatchDetail>(new EntityDoesNotExistError()) : Result.Ok(match);
    }

    public async Task<Result<LiveMatch>> GetLiveMatchAsync(CancellationToken cancellationToken = default)
    {
        LiveMatchReadout? readout;

        try
        {
            readout = await gameClient.GetLiveMatchAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<LiveMatch>(new GameUnavailableError(exception.Message));
        }

        // No match being played is an answer, not an error.
        if (readout is null)
        {
            return Result.Ok(new LiveMatch { InProgress = false, Message = LiveMatch.NoMatchMessage });
        }

        var us = readout.IsHome ? readout.Home : readout.Away;
        var them = readout.IsHome ? readout.Away : readout.Home;
        var substitutions = LiveSubstitutions(readout.Players);

        return Result.Ok(new LiveMatch
        {
            InProgress = true,
            Minute = readout.Minute,
            Clock = readout.Clock,
            Competition = readout.Competition,
            Opponent = them.TeamName,
            Venue = readout.IsHome ? Venue.Home : Venue.Away,
            GoalsFor = us.Goals,
            GoalsAgainst = them.Goals,
            Goals = readout.Goals,
            TeamStats = readout.IsHome ? readout.HomeStats : readout.AwayStats,
            OpponentStats = readout.IsHome ? readout.AwayStats : readout.HomeStats,
            OnPitch = readout.Players.Where(player => player.Slot >= 0).OrderBy(player => player.Slot).ToList(),
            Substitutions = substitutions,
            SubstitutionsRemaining = readout.SubstitutionsRemaining
                ?? (readout.SubstitutionsAllowed is { } allowed ? Math.Max(0, allowed - substitutions.Count) : null),
            Tactic = readout.Tactic
        });
    }

    // The game keeps each player's last change only: when a substitute came on, when a player left the pitch. A
    // substitution is a player coming on paired with one going off in the same minute, in the order they're listed.
    private static List<MatchSubstitution> LiveSubstitutions(List<LivePlayer> players)
    {
        var leaving = players
            .Where(player => player.MinuteOff is not null && player.MinuteOn is null)
            .GroupBy(player => player.MinuteOff!.Value)
            .ToDictionary(group => group.Key, group => new Queue<LivePlayer>(group));

        return players
            .Where(player => player.MinuteOn is not null)
            .OrderBy(player => player.MinuteOn)
            .Select(on =>
            {
                var off = leaving.TryGetValue(on.MinuteOn!.Value, out var queue) && queue.Count > 0 ? queue.Dequeue() : null;

                return new MatchSubstitution
                {
                    Minute = on.MinuteOn,
                    OnPlayerId = on.PlayerId,
                    On = on.Name,
                    OffPlayerId = off?.PlayerId,
                    Off = off?.Name,
                    Injury = off?.WentOffInjured == true
                };
            })
            .ToList();
    }

    public async Task<Result<TeamFixtures>> GetUpcomingAsync(int count = DefaultUpcoming, CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > MaxUpcoming)
        {
            return Result.Fail<TeamFixtures>(new ValidationError(nameof(count), [$"The count runs from 1 to {MaxUpcoming}."]));
        }

        TeamFixtures fixtures;

        try
        {
            fixtures = await gameClient.GetUpcomingFixturesAsync(count, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TeamFixtures>(new GameUnavailableError(exception.Message));
        }

        return Result.Ok(new TeamFixtures
        {
            TeamName = fixtures.TeamName,
            Fixtures = fixtures.Fixtures.Where(f => !f.IsPlayed).Take(count).ToList()
        });
    }
}
