using System.Reflection;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Text;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

public interface IScoutingService
{
    /// <summary>The scouting centre's counts, the club's scouts and its recruitment focuses.</summary>
    Task<Result<ScoutingOverview>> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The players of one of the user's scouting lists, as the club's scouting knows them, best graded first. A focus
    /// is named by its id (from the overview). The player search holds hundreds of players or more: only its first
    /// <paramref name="limit"/> are read (<see cref="ScoutingService.DefaultSearchLimit"/> by default), in the search screen's order.
    /// </summary>
    Task<Result<ScoutedPlayerList>> GetPlayersAsync(
        ScoutingSource source = ScoutingSource.Recommendations,
        int? focusId = null,
        FocusPool pool = FocusPool.Recommended,
        bool includeAttributes = false,
        bool includeHidden = false,
        bool includeReport = false,
        int? limit = null,
        bool includeStats = false,
        PerformanceFilter? filter = null,
        int? scan = null,
        CancellationToken cancellationToken = default);
}

public class ScoutingService(IScoutingGameClient gameClient, IPlayerSearchGameClient searchClient) : IScoutingService
{
    public const int DefaultSearchLimit = 50;
    public const int MaxLimit = 200;

    // With a performance filter the search is read whole (up to this many), then cut on the players' numbers.
    public const int MaxScan = 3000;

    // The numbers a filter can bound, by name, case ignored.
    private static readonly Dictionary<string, PropertyInfo> PerformanceFields = typeof(PlayerPerformance)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => Nullable.GetUnderlyingType(p.PropertyType) is { } type && (type == typeof(int) || type == typeof(double)))
        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

    public async Task<Result<ScoutingOverview>> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetOverviewAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ScoutingOverview>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<ScoutedPlayerList>> GetPlayersAsync(
        ScoutingSource source = ScoutingSource.Recommendations,
        int? focusId = null,
        FocusPool pool = FocusPool.Recommended,
        bool includeAttributes = false,
        bool includeHidden = false,
        bool includeReport = false,
        int? limit = null,
        bool includeStats = false,
        PerformanceFilter? filter = null,
        int? scan = null,
        CancellationToken cancellationToken = default)
    {
        if (source == ScoutingSource.Focus && focusId is null)
        {
            return Fail("focusId", "A focus's players need the focus's id (get_scouting lists the focuses).");
        }

        if (source != ScoutingSource.Focus && focusId is not null)
        {
            return Fail("focusId", "A focus id only goes with the focus source.");
        }

        if (limit is < 1 or > MaxLimit)
        {
            return Fail("limit", $"The limit runs 1 to {MaxLimit}.");
        }

        if (scan is < 1 or > MaxScan)
        {
            return Fail("scan", $"The scan runs 1 to {MaxScan}.");
        }

        if (scan is not null && (source != ScoutingSource.Search || filter is null || filter.IsEmpty))
        {
            return Fail("scan", "The scan only goes with the search source and a performance filter.");
        }

        if (filter is not null
            && filter.Minimums.Keys.Concat(filter.Maximums.Keys).FirstOrDefault(name => !PerformanceFields.ContainsKey(name)) is { } unknown)
        {
            var known = string.Join(", ", PerformanceFields.Keys.Select(name => char.ToLowerInvariant(name[0]) + name[1..]));
            return Fail("minimums", $"No number '{unknown}' to cut on. The numbers: {known}.");
        }

        // The cuts need the numbers.
        var filtering = filter is { IsEmpty: false };
        includeStats |= filtering;

        // Hidden values are never fetched unless asked for, so they can't leak into an answer by accident.
        var attributes = new List<AttributeDefinition>();

        if (includeAttributes)
        {
            attributes.AddRange(PlayerAttributes.Visible);
        }

        if (includeHidden)
        {
            attributes.AddRange(PlayerAttributes.Hidden);
        }

        try
        {
            RecruitmentFocus? focus = null;

            if (focusId is { } id)
            {
                var focuses = await gameClient.GetFocusesAsync(cancellationToken);
                focus = focuses.FirstOrDefault(f => f.Id == id);

                if (focus is null)
                {
                    var known = string.Join(", ", focuses.Select(f => $"{f.Name} (id {f.Id})"));
                    return Fail("focusId", $"No recruitment focus with id {id}. The club's focuses: {(known.Length > 0 ? known : "none")}.");
                }

                if (focus.ShowsRemovedFocus)
                {
                    return Fail("focusId", $"The game still shows the removed focus '{focus.Name}' under id {id}, which it has " +
                                           "given a newer focus: its players would be the removed focus's. They can be read " +
                                           "once the game refreshes its focus list.");
                }
            }

            // The search is read a page at a time, in its own order; the other lists are short, sorted whole, then cut.
            var take = source != ScoutingSource.Search ? (int?)null
                : filtering ? scan ?? MaxScan
                : limit ?? DefaultSearchLimit;
            var page = await gameClient.GetPlayersAsync(
                source, focusId, pool, attributes, includeHidden, includeReport, take, includeStats, cancellationToken);

            if (page.Search is not null)
            {
                page.Search.Filters = await searchClient.GetFiltersAsync(cancellationToken);
            }

            // The sort is stable: within a grade, players keep the game's own order.
            var players = page.Players
                .Where(p => !filtering || Passes(p, filter!))
                .OrderByDescending(p => p.Grade ?? -1)
                .Take(limit ?? int.MaxValue)
                .ToList();

            return Result.Ok(new ScoutedPlayerList
            {
                Source = source,
                Focus = focus,
                Pool = focus is null ? null : pool,
                Search = page.Search,
                Total = page.Total,
                Scanned = filtering ? page.Players.Count : null,
                Complete = filtering ? page.Players.Count >= page.Total : null,
                Leagues = filtering
                    ? page.Players.GroupBy(LeagueLabel).OrderByDescending(g => g.Count()).ToDictionary(g => g.Key, g => g.Count())
                    : null,
                UnmatchedLeagues = filtering && filter!.Leagues.Count > 0
                    ? filter.Leagues.Where(league => !page.Players.Any(p => SameLeague(p, league))).ToList()
                    : null,
                Players = players,
                HiddenDataMasked = includeHidden && players.Count > 0 && !players.Any(p => p.HasHiddenValues)
            });
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ScoutedPlayerList>(new GameUnavailableError(exception.Message));
        }
    }

    // A player without a number fails any bound on it. Numbers are compared as the answer shows them, to 2 decimals.
    private static bool Passes(ScoutedPlayer player, PerformanceFilter filter)
    {
        var numbers = player.Performance;

        return (filter.Leagues.Count == 0 || filter.Leagues.Any(league => SameLeague(player, league)))
            && (filter.MinMinutes is not { } minutes || numbers?.Minutes >= minutes)
            && (filter.MinAverageRating is not { } rating || Shown(numbers?.AverageRating) >= rating)
            && filter.Minimums.All(bound => Number(numbers, bound.Key) >= bound.Value)
            && filter.Maximums.All(bound => Number(numbers, bound.Key) <= bound.Value);
    }

    // A league is named alone ("Primera B": any nation's) or with its nation, as the answer's leagues list it ("Primera B (Chile)").
    private static bool SameLeague(ScoutedPlayer player, string league)
    {
        var wanted = GameNames.Normalise(league);
        return player.League is { } name
               && (GameNames.Normalise(name) == wanted || GameNames.Normalise(LeagueLabel(player)) == wanted);
    }

    private static string LeagueLabel(ScoutedPlayer player)
    {
        return player.League is not { } name ? "(no league)"
            : player.LeagueNation is { } nation ? $"{name} ({nation})"
            : name;
    }

    private static double? Number(PlayerPerformance? numbers, string name)
    {
        return numbers is null ? null : Shown(PerformanceFields[name].GetValue(numbers) switch
        {
            int whole => whole,
            double value => value,
            _ => null
        });
    }

    private static double? Shown(double? value)
    {
        return value is { } number ? Math.Round(number, 2) : null;
    }

    private static Result<ScoutedPlayerList> Fail(string property, string message)
    {
        return Result.Fail<ScoutedPlayerList>(new ValidationError(property, [message]));
    }
}
