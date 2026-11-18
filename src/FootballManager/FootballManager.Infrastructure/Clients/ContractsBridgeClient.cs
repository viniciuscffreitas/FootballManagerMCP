using FootballManager.Application.Exceptions;
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

        // A list or a row the game sent as null is a read that failed, not an empty squad or a player gone.
        if (squad.Items is null || squad.Items.Any(item => item?.Values is null))
        {
            throw new GameUnavailableException("The game sent an empty row in the squad's contracts: read them again.");
        }

        // A row whose id the game didn't give (not answered, failed, or missing) makes the read unknown: read as 0, the
        // player would look gone from the squad.
        if (squad.Items.FirstOrDefault(item => item.Values.Id is null) is { } unread)
        {
            var reason = unread.Errors?.GetValueOrDefault(GameConstants.UniqueId)
                         ?? (unread.Pending?.Contains(GameConstants.UniqueId) == true ? "not answered in time" : "missing");
            throw new GameUnavailableException($"The game didn't give a player's id in the squad's contracts ({reason}): read them again.");
        }

        return squad.Items.Select(item => item.Values).ToList().Adapt<List<SquadPlayerContract>>();
    }
}
