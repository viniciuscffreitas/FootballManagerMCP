using FootballManager.Application.Interfaces;
using FootballManager.Domain.Finances;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Finances;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's club finance screen through the MCPFM plugin, in one read of the club.
internal sealed class FinancesBridgeClient(GameBridge bridge) : IFinancesGameClient
{
    public async Task<FinanceReadout> GetFinancesAsync(CancellationToken cancellationToken = default)
    {
        var finances = await bridge.GetRecordAsync<BridgeFinancesDto>(
            BridgeQuery.FromHuman().Base(FinancesConstants.Club).WithHumanAndTeam().Props(FinancesConstants.Properties),
            cancellationToken);

        return finances.Values.Adapt<FinanceReadout>();
    }
}
