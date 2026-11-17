using FootballManager.Application.Interfaces;
using FootballManager.Domain.Board;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Board;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the club vision screen through the MCPFM plugin: the team's confidence record, the club's vision and board
// requests, and the manager's promises to the board, the three in parallel.
internal sealed class BoardBridgeClient(GameBridge bridge) : IBoardGameClient
{
    public async Task<BoardStatus> GetBoardAsync(CancellationToken cancellationToken = default)
    {
        var confidence = bridge.GetRecordAsync<BridgeTeamConfidenceDto>(
            BridgeQuery.FromHuman().Base(BoardConstants.TeamConfidence).WithHumanAndTeam().Props(BoardConstants.ConfidenceProperties),
            cancellationToken);
        var vision = bridge.GetRecordAsync<BridgeClubVisionDto>(
            BridgeQuery.FromHuman().Base(BoardConstants.Club).WithHumanAndTeam()
                .Context(BoardConstants.UsingPerformanceData, false).Props(BoardConstants.VisionProperties),
            cancellationToken);
        var promises = bridge.GetRecordAsync<BridgeBoardPromisesDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props([BoardConstants.Promises]), cancellationToken);

        await Task.WhenAll(confidence, vision, promises);

        // The vision read fills the objectives, culture and requests; the confidence record and the promises the rest.
        var status = (await vision).Values.Adapt<BoardStatus>();
        status.Board = (await confidence).Values.Adapt<BoardConfidence>();
        status.Supporters = (await confidence).Values.Adapt<SupporterConfidence>();
        status.Promises = (await promises).Values.Adapt<List<string>>();
        return status;
    }
}
