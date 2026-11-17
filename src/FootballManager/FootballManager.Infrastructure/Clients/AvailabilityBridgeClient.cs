using FootballManager.Application.Interfaces;
using FootballManager.Domain.Availability;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Availability;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads who can play the next match through the MCPFM plugin: each first-team player's injury record, the team's
// bans in every competition, and the next match's squad rules, the three in parallel.
internal sealed class AvailabilityBridgeClient(GameBridge bridge) : IAvailabilityGameClient
{
    public async Task<AvailabilityReadout> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var injuries = bridge.GetSquadAsync<BridgeInjuryDto>(AvailabilityConstants.InjuryProperties, cancellationToken);
        var bans = bridge.GetListAsync<BridgeBanDto>(
            BridgeQuery.FromHuman().List(AvailabilityConstants.Bans).WithHumanAndTeam().Props(AvailabilityConstants.BanProperties),
            cancellationToken);
        var nextMatch = bridge.GetRecordAsync<BridgeNextMatchRulesDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).WithHumanAndTeam().Props(AvailabilityConstants.NextMatchProperties),
            cancellationToken);

        await Task.WhenAll(injuries, bans, nextMatch);

        return new AvailabilityReadout
        {
            Injuries = (await injuries).Items.Select(item => item.Values).ToList().Adapt<List<PlayerInjury>>(),
            Bans = (await bans).Items.Select(item => item.Values).ToList().Adapt<List<PlayerBan>>(),
            NextMatch = (await nextMatch).Values.Adapt<NextMatchRules>()
        };
    }
}
