using FootballManager.Application.Interfaces;
using FootballManager.Domain.Standings;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Standings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's league table through the MCPFM plugin: the user's team (its league, the league's current stage and
// the press's prediction), then, when the league has a stage, that stage's overall table, one item per team.
internal sealed class LeagueTableBridgeClient(GameBridge bridge) : ILeagueTableGameClient
{
    public async Task<LeagueTableReadout> GetLeagueTableAsync(CancellationToken cancellationToken = default)
    {
        var team = (await bridge.GetRecordAsync<BridgeStandingsTeamDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).WithHumanAndTeam().Props(StandingsConstants.TeamProperties),
            cancellationToken)).Values;

        var readout = new LeagueTableReadout
        {
            League = team.League,
            TeamId = team.Id ?? 0,
            TeamName = team.Name ?? string.Empty,
            MediaExpectedPosition = team.MediaExpectedPosition,
            HasLeagueStage = team.Stage?.Type == StandingsConstants.StageType
        };

        // Between seasons the game has no stage to read a table from.
        if (!readout.HasLeagueStage)
        {
            return readout;
        }

        var table = await bridge.GetListAsync<BridgeTableRowDto>(
            BridgeQuery.FromHuman()
                .List(StandingsConstants.Table)
                .WithHumanAndTeam()
                .Context(StandingsConstants.TableType, StandingsConstants.OverallTable)
                .Props(StandingsConstants.RowProperties),
            cancellationToken);

        readout.Rows = table.Items.Select(item => item.Values).ToList().Adapt<List<LeagueTableRow>>();
        return readout;
    }
}
