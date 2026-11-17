using FootballManager.Application.Interfaces;
using FootballManager.Domain.Opposition;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Opposition;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the game's opposition report through the MCPFM plugin, as its pre-match analysis does: on the opponent's team,
// with the user's team as the one requesting it. The report names players by their record, which the opponent's squad
// list gives with names and ids. Nothing passes the "team" context, so the user's current team never changes.
internal sealed class OppositionBridgeClient(GameBridge bridge, GameVocabulary vocabulary, TeamsBridgeClient teams)
    : IOppositionGameClient
{
    public async Task<NextOpponent?> GetNextOpponentAsync(CancellationToken cancellationToken = default)
    {
        var next = await bridge.GetRecordAsync<BridgeNextOpponentDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).WithHumanAndTeam().Props(OppositionConstants.NextOpponentProperties),
            cancellationToken);

        // Without a match ahead the game has no opponent to name.
        if (next.Values is not { Id: { } id, Name: { } name })
        {
            return null;
        }

        return new NextOpponent(id, name, string.IsNullOrWhiteSpace(next.Values.ThreatLevel) ? null : next.Values.ThreatLevel);
    }

    public async Task<OppositionReport> GetReportAsync(int? teamId, CancellationToken cancellationToken = default)
    {
        var path = teamId is { } id ? await PathAsync(id, cancellationToken) : OppositionConstants.NextOpponent;

        var keys = vocabulary.KeysAsync(cancellationToken);
        var report = bridge.GetRecordAsync<BridgeOppositionReportDto>(
            BridgeQuery.FromHuman()
                .Base(path)
                .Ref(OppositionConstants.RequestingTeam, TeamConstants.OwnTeam)
                .Context(TacticConstants.InPossession, true)
                .WithHumanAndTeam()
                .Props(OppositionConstants.ReportProperties),
            cancellationToken);
        var squad = bridge.GetListAsync<BridgePlayerDto>(
            BridgeQuery.FromHuman().List($"{path}.{TeamConstants.Players}").Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken,
            gapsExpected: true);

        await Task.WhenAll(keys, report, squad);

        // The report's players are references to their records, which the squad's rows give.
        var players = (await squad).Items
            .Where(player => player.Source?.Index is not null)
            .GroupBy(player => player.Source!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First().Values);

        return (await report).Values
            .BuildAdapter()
            .AddParameters(MappingConfig.TacticKeysParameter, await keys)
            .AddParameters(MappingConfig.PlayersByRecordParameter, players)
            .AdaptToType<OppositionReport>();
    }

    // A team of the user's competitions, listed by get_team's lookup; those are all reached from the user's manager.
    private async Task<string> PathAsync(int teamId, CancellationToken cancellationToken)
    {
        if (await teams.LocateAsync(teamId, cancellationToken) is { Root: GameConstants.HumanRoot } team)
        {
            return team.Path;
        }

        throw new ArgumentException($"Team {teamId} isn't in the user's league or competitions.", nameof(teamId));
    }
}
