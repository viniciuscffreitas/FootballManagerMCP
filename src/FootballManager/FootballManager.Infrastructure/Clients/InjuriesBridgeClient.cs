using FootballManager.Application.Interfaces;
using FootballManager.Domain.Injuries;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Injuries;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the medical centre through the MCPFM plugin: each first-team player's injury history and risk, and the
// team's season summary, the two in parallel.
internal sealed class InjuriesBridgeClient(GameBridge bridge) : IInjuriesGameClient
{
    public async Task<MedicalCentre> GetMedicalCentreAsync(CancellationToken cancellationToken = default)
    {
        var players = bridge.GetListAsync<BridgeMedicalRecordDto>(
            BridgeQuery.FromHuman().List(InjuryConstants.Players).WithHumanAndTeam().Props(InjuryConstants.PlayerProperties),
            cancellationToken);
        var season = bridge.GetRecordAsync<BridgeSeasonInjuriesDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).WithHumanAndTeam().Props(InjuryConstants.SeasonProperties),
            cancellationToken);

        await Task.WhenAll(players, season);

        return new MedicalCentre
        {
            Players = (await players).Items.Select(item => item.Values).ToList().Adapt<List<PlayerMedicalRecord>>(),
            Season = (await season).Values.Adapt<SeasonInjurySummary>()
        };
    }
}
