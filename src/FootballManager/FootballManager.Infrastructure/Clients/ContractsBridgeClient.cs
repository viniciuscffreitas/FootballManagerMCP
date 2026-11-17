using FootballManager.Application.Interfaces;
using FootballManager.Domain.Contracts;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Contracts;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the first team's contracts through the MCPFM plugin, one record per player, with the user's manager and team as
// contexts (the staff's renewal advice needs them).
internal sealed class ContractsBridgeClient(GameBridge bridge) : IContractsGameClient
{
    public async Task<List<SquadPlayerContract>> GetContractsAsync(CancellationToken cancellationToken = default)
    {
        var squad = await bridge.GetSquadAsync<BridgeSquadContractDto>(ContractConstants.Properties, cancellationToken);
        return squad.Items.Select(item => item.Values).ToList().Adapt<List<SquadPlayerContract>>();
    }
}
