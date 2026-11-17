using FootballManager.Application.Interfaces;
using FootballManager.Domain.Transfers;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Transfers;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the game's transfer activity screen through the MCPFM plugin: its counts and values, the players with offers
// each way, then the offers of each of those players. Read only.
internal sealed class TransferActivityBridgeClient(GameBridge bridge) : ITransferActivityGameClient
{
    public async Task<TransferActivityReadout> GetTransferActivityAsync(CancellationToken cancellationToken = default)
    {
        var summary = bridge.GetRecordAsync<BridgeTransferActivityDto>(
            BridgeQuery.FromHuman().Base(TransferActivityConstants.Activity).WithHumanAndTeam().Props(TransferActivityConstants.SummaryProperties),
            cancellationToken);
        var incoming = PlayersAsync(TransferActivityConstants.PlayersIn, TransferDirection.In, cancellationToken);
        var outgoing = PlayersAsync(TransferActivityConstants.PlayersOut, TransferDirection.Out, cancellationToken);

        await Task.WhenAll(summary, incoming, outgoing);

        var readout = (await summary).Values.Adapt<TransferActivityReadout>();
        readout.Players = [.. await incoming, .. await outgoing];
        return readout;
    }

    private async Task<List<TransferActivityPlayer>> PlayersAsync(string list, TransferDirection direction, CancellationToken cancellationToken)
    {
        var rows = await bridge.GetListAsync<BridgeTransferActivityPlayerDto>(
            BridgeQuery.FromHuman().List(list).WithHumanAndTeam().Props(TransferActivityConstants.PlayerProperties),
            cancellationToken);

        var players = new List<TransferActivityPlayer>();

        for (var index = 0; index < rows.Items.Count; index++)
        {
            var player = rows.Items[index].Values.Adapt<TransferActivityPlayer>();
            player.Direction = direction;
            player.Offers = await OffersAsync(list, index, cancellationToken);
            players.Add(player);
        }

        return players;
    }

    private async Task<List<TransferActivityOffer>> OffersAsync(string list, int index, CancellationToken cancellationToken)
    {
        var offers = await bridge.GetListAsync<BridgeTransferActivityOfferDto>(
            BridgeQuery.FromHuman().List(TransferActivityConstants.Offers(list, index)).WithHumanAndTeam().Props(TransferActivityConstants.OfferProperties),
            cancellationToken);

        return offers.Items.Select(item => item.Values).ToList().Adapt<List<TransferActivityOffer>>();
    }
}
