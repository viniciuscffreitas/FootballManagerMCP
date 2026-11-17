using System.Globalization;
using System.Text.Json;
using FootballManager.Domain.Matches;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Matches;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// The match being played, read the way its screen does: first whether the game is in a match at all, then the match,
// both sides' numbers, the tactic and the squad's names together. Every read is a light /get; nothing touches the
// screen's UI tree, which stalls the live match.
internal sealed partial class MatchesBridgeClient
{
    public async Task<LiveMatchReadout?> GetLiveMatchAsync(CancellationToken cancellationToken = default)
    {
        var state = bridge.GetRecordAsync<BridgeLiveStateDto>(BridgeQuery.FromGame().Props(LiveMatchConstants.State), cancellationToken);
        var teams = bridge.GetRecordAsync<BridgeLiveTeamsDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props(LiveMatchConstants.Teams), cancellationToken);

        await Task.WhenAll(state, teams);

        var game = (await state).Values;
        var user = (await teams).Values;

        // Between matches the next managed fixture is a later one; on match day, before kick-off and after the final
        // whistle, the game isn't in a match.
        if (game.IsDuringMatch != true || game.CurrentDate is null || user.MatchDate != game.CurrentDate)
        {
            return null;
        }

        var match = bridge.GetRecordAsync<BridgeLiveMatchDto>(
            Live().Context(MatchConstants.ShowAllEventTypes, true).Props(LiveMatchConstants.Summary), cancellationToken);
        var home = LiveSideAsync(isHome: true, cancellationToken);
        var away = LiveSideAsync(isHome: false, cancellationToken);
        var tactic = bridge.GetRecordAsync<BridgeLiveTacticDto>(
            BridgeQuery.OnActiveTactic().Context(TacticConstants.InPossession, true).Props(LiveMatchConstants.Tactic), cancellationToken);
        var squad = bridge.GetSquadAsync<BridgeLiveSquadPlayerDto>(
            [GameConstants.UniqueId, GameConstants.Name], cancellationToken);

        await Task.WhenAll(match, home, away, tactic, squad);

        var live = (await match).Values;
        var teamId = user.TeamId ?? 0;
        var isHome = live.HomeTeamId == teamId;
        var (homeSide, homePlayers, homeCondition) = await home;
        var (awaySide, awayPlayers, awayCondition) = await away;
        var (ourSide, ourPlayers, ourCondition) = isHome ? (homeSide, homePlayers, homeCondition) : (awaySide, awayPlayers, awayCondition);
        var played = (await tactic).Values;

        // The squad by database index, which is how a match refers to its players.
        var members = (await squad).Items
            .Where(player => player.Source?.Index is not null)
            .GroupBy(player => player.Source!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First().Values);
        var names = members.ToDictionary(
            member => member.Key, member => new SquadMember(-1, member.Value.Id, member.Value.Name));

        return new LiveMatchReadout
        {
            Minute = Number(live.Minute) ?? ClockMinute(live.Clock),
            Clock = string.IsNullOrEmpty(live.Clock) ? null : live.Clock,
            Competition = live.Competition,
            TeamId = teamId,
            Home = new MatchSide { TeamId = live.HomeTeamId ?? 0, TeamName = live.HomeTeamName ?? string.Empty, Goals = Score(live.LiveHomeScore, live.HomeScore) },
            Away = new MatchSide { TeamId = live.AwayTeamId ?? 0, TeamName = live.AwayTeamName ?? string.Empty, Goals = Score(live.LiveAwayScore, live.AwayScore) },
            HomeStats = homeSide.Stats.Adapt<TeamMatchStats?>(),
            AwayStats = awaySide.Stats.Adapt<TeamMatchStats?>(),
            Goals = Goals(new BridgeMatchEventsDto { Incidents = live.Incidents }, isHome, names, live: true),
            Players = ourPlayers.Select(line => LivePlayer(line, members, ourCondition)).ToList(),
            Tactic = new LiveTactic { Name = played.Name, Formation = played.Formation, Mentality = played.Mentality },
            SubstitutionsRemaining = Number(ourSide.SubstitutionsRemaining),
            SubstitutionsAllowed = Number(ourSide.SubstitutionsAllowed)
        };
    }

    // The match root, not the fixture: on the fixture the live values stay pending or empty during the match.
    private static BridgeQuery Live()
    {
        return BridgeQuery.FromMatch().WithHumanAndTeam();
    }

    // A side's numbers, its players' match lines and their condition now. The condition is the match's: a player's own
    // (on /squad) holds the kick-off value until the match ends (seen 17/11/2026: 93 there, 82 in the match at 45').
    private async Task<(BridgeLiveSideDto Side, List<BridgePlayerMatchStatsDto> Players, Dictionary<int, int> Condition)> LiveSideAsync(
        bool isHome, CancellationToken cancellationToken)
    {
        var side = bridge.GetRecordAsync<BridgeLiveSideDto>(
            Live().Context(MatchConstants.IsHomeTeam, isHome).Props(LiveMatchConstants.Side), cancellationToken);
        var players = bridge.GetListAsync<BridgePlayerMatchStatsDto>(
            Live().List(MatchConstants.PlayerStats).Context(MatchConstants.IsHomeTeam, isHome).Props(LiveMatchConstants.PlayerLine),
            cancellationToken);
        var condition = bridge.GetListAsync<BridgeLiveConditionDto>(
            Live().List(LiveMatchConstants.MatchPlayers).Context(MatchConstants.IsHomeTeam, isHome)
                .Context(LiveMatchConstants.ShowingSubs, false).Props(LiveMatchConstants.MatchPlayerCondition),
            cancellationToken);

        await Task.WhenAll(side, players, condition);

        var conditionById = (await condition).Items
            .Select(item => item.Values)
            .Where(player => player.Id is not null && player.Condition >= 0)
            .GroupBy(player => player.Id!.Value)
            .ToDictionary(group => group.Key, group => group.First().Condition!.Value);

        return ((await side).Values, (await players).Items.Select(item => item.Values).ToList(), conditionById);
    }

    // The minute being played from the match clock (minutes and seconds gone): "18:45" is the 19th minute, and a clock
    // stopped on the minute ("45:00" at half-time) is that minute.
    private static int? ClockMinute(string? clock)
    {
        var parts = clock?.Split(':');

        if (parts is not [var minutes, var seconds]
            || !int.TryParse(minutes, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gone)
            || !int.TryParse(seconds, NumberStyles.Integer, CultureInfo.InvariantCulture, out var secondsGone))
        {
            return null;
        }

        return gone > 0 && secondsGone == 0 ? gone : gone + 1;
    }

    // A player's match line keeps his last change only: came on (with the minute), or left the pitch (his slot is then
    // -1, with the minute he left). Its incident only tells an injury apart for a substitute who came on and went off
    // injured; a starter going off is the same incident, injured or not.
    private static LivePlayer LivePlayer(
        BridgePlayerMatchStatsDto line, IReadOnlyDictionary<int, BridgeLiveSquadPlayerDto> squad, IReadOnlyDictionary<int, int> condition)
    {
        var member = line.Player?.Index is { } index ? squad.GetValueOrDefault(index) : null;
        var leftThePitch = line.PlayerTeamSelectionIndex is < 0;

        return new LivePlayer
        {
            PlayerId = member?.Id,
            Name = member?.Name,
            Slot = line.PlayerTeamSelectionIndex,
            Rating = line.TotalRating > 0 ? line.TotalRating : null,
            Condition = member?.Id is { } id && condition.TryGetValue(id, out var now) ? now : null,
            YellowCard = line.YellowCardedInMatch == true,
            RedCard = line.RedCardedInMatch == true,
            Goals = line.Goals,
            MinuteOn = !leftThePitch && line.IncidentType == MatchConstants.CameOnIncident ? line.MatchTime : null,
            MinuteOff = leftThePitch ? line.MatchTime : null,
            WentOffInjured = leftThePitch && line.IncidentType == MatchConstants.InjuredOffIncident
        };
    }

    // The live score when the game sends it, else the fixture's once written (-1 before).
    private static int? Score(JsonElement? live, int? written)
    {
        return Number(live) ?? (written >= 0 ? written : null);
    }

    // A number sent as a number or as text.
    private static int? Number(JsonElement? value)
    {
        return value switch
        {
            { ValueKind: JsonValueKind.Number } number when number.TryGetDouble(out var real) => (int)real,
            { ValueKind: JsonValueKind.String } text when int.TryParse(text.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }
}
