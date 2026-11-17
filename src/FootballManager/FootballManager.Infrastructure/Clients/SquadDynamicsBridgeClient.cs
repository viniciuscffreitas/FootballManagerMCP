using FootballManager.Application.Interfaces;
using FootballManager.Domain.DressingRoom;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.DressingRoom;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the dressing room through the MCPFM plugin: the four hierarchy lists, the open happiness issues, the club
// atmosphere, the manager's promises and each first-team player's happiness, all in parallel.
internal sealed class SquadDynamicsBridgeClient(GameBridge bridge) : ISquadDynamicsGameClient
{
    public async Task<SquadDynamics> GetSquadDynamicsAsync(CancellationToken cancellationToken = default)
    {
        var teamLeaders = Members(SquadDynamicsConstants.TeamLeaders, cancellationToken);
        var highlyInfluential = Members(SquadDynamicsConstants.HighlyInfluential, cancellationToken);
        var influential = Members(SquadDynamicsConstants.Influential, cancellationToken);
        var other = Members(SquadDynamicsConstants.Other, cancellationToken);
        var issues = bridge.GetListAsync<BridgeHappinessIssueDto>(
            BridgeQuery.FromHuman().List(SquadDynamicsConstants.Issues).WithHumanAndTeam().Props(SquadDynamicsConstants.IssueProperties),
            cancellationToken);
        var atmosphere = bridge.GetRecordAsync<BridgeAtmosphereDto>(
            BridgeQuery.FromHuman().Base(SquadDynamicsConstants.Atmosphere).WithHumanAndTeam().Props(SquadDynamicsConstants.AtmosphereProperties),
            cancellationToken);
        var promises = bridge.GetListAsync<BridgePromiseDto>(
            BridgeQuery.FromHuman().List(SquadDynamicsConstants.Promises).WithHumanAndTeam().Props(SquadDynamicsConstants.PromiseProperties),
            cancellationToken);
        var players = bridge.GetSquadAsync<BridgePlayerDynamicsDto>(SquadDynamicsConstants.PlayerProperties, cancellationToken);

        await Task.WhenAll(teamLeaders, highlyInfluential, influential, other, issues, atmosphere, promises, players);

        return new SquadDynamics
        {
            Hierarchy = new SquadHierarchy
            {
                TeamLeaders = await teamLeaders,
                HighlyInfluential = await highlyInfluential,
                Influential = await influential,
                Other = await other
            },
            Atmosphere = (await atmosphere).Values.Adapt<DressingRoomAtmosphere>(),
            Issues = (await issues).Items.Select(item => item.Values).ToList().Adapt<List<HappinessIssue>>(),
            Promises = (await promises).Items.Select(item => item.Values).ToList().Adapt<List<PlayerPromise>>(),
            Players = (await players).Items.Select(item => item.Values).ToList().Adapt<List<PlayerDynamics>>()
        };
    }

    private async Task<List<HierarchyMember>> Members(string level, CancellationToken cancellationToken)
    {
        var list = await bridge.GetListAsync<BridgeHierarchyMemberDto>(
            BridgeQuery.FromHuman().List(level).WithHumanAndTeam().Props(SquadDynamicsConstants.MemberProperties),
            cancellationToken);

        return list.Items.Select(item => item.Values).ToList().Adapt<List<HierarchyMember>>();
    }
}
