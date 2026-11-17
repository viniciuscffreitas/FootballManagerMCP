using FootballManager.Application.Interfaces;
using FootballManager.Domain.Youth;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Youth;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the club's youth set-up, its highest youth team and its players out on loan through the MCPFM plugin, the
// three in parallel. Read with the user's manager and team as contexts, like the game's screens.
internal sealed class YouthBridgeClient(GameBridge bridge) : IYouthGameClient
{
    public async Task<YouthAndLoans> GetYouthAndLoansAsync(CancellationToken cancellationToken = default)
    {
        var setup = bridge.GetRecordAsync<BridgeYouthSetupDto>(
            BridgeQuery.FromHuman().Base(YouthConstants.Club).WithHumanAndTeam().Props(YouthConstants.SetupProperties),
            cancellationToken);
        var youthPlayers = bridge.GetListAsync<BridgeYouthPlayerDto>(
            BridgeQuery.FromHuman().List(YouthConstants.YouthPlayers).WithHumanAndTeam().Props(YouthConstants.YouthPlayerProperties),
            cancellationToken);
        var loans = bridge.GetListAsync<BridgeLoanedPlayerDto>(
            BridgeQuery.FromHuman().List(YouthConstants.Loans).WithHumanAndTeam().Props(YouthConstants.LoanProperties),
            cancellationToken);

        await Task.WhenAll(setup, youthPlayers, loans);

        var setupValues = (await setup).Values;

        return new YouthAndLoans
        {
            Youth = setupValues.Adapt<YouthSetup>(),
            YouthPlayers = (await youthPlayers).Items.Select(item => item.Values).ToList().Adapt<List<YouthPlayer>>(),
            Loans = (await loans).Items.Select(item => item.Values).ToList().Adapt<List<LoanedPlayer>>(),
            LoanReport = string.IsNullOrWhiteSpace(setupValues.LoanReport) ? null : setupValues.LoanReport,
            PlayersSuggestedForLoan = setupValues.PlayersSuggestedForLoan
        };
    }
}
