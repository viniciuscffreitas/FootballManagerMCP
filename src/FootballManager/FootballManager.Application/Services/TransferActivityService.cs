using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Services;

public interface ITransferActivityService
{
    Task<Result<TransferActivity>> GetTransferActivityAsync(CancellationToken cancellationToken = default);
}

public class TransferActivityService(ITransferActivityGameClient gameClient) : ITransferActivityService
{
    public async Task<Result<TransferActivity>> GetTransferActivityAsync(CancellationToken cancellationToken = default)
    {
        TransferActivityReadout readout;

        try
        {
            readout = await gameClient.GetTransferActivityAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TransferActivity>(new GameUnavailableError(exception.Message));
        }

        return Result.Ok(new TransferActivity
        {
            Counts = readout.Counts,
            IncomingValue = readout.IncomingValue,
            OutgoingValue = readout.OutgoingValue,
            Negotiations = readout.Players
                .SelectMany(Negotiations)
                .OrderByDescending(negotiation => negotiation.RequiresResponse)
                .ToList()
        });
    }

    // One per offer; a player the game lists without offers keeps his row, with what the row itself says.
    private static IEnumerable<TransferNegotiation> Negotiations(TransferActivityPlayer player)
    {
        if (player.Offers.Count == 0)
        {
            return [Negotiation(player, new TransferActivityOffer { Status = player.Status })];
        }

        return player.Offers.Select(offer => Negotiation(player, offer));
    }

    private static TransferNegotiation Negotiation(TransferActivityPlayer player, TransferActivityOffer offer)
    {
        return new TransferNegotiation
        {
            PlayerId = player.PlayerId,
            PlayerName = player.Name,
            Direction = player.Direction,
            Club = player.Direction == TransferDirection.In ? offer.SellingClub : offer.BuyingClub,
            Type = offer.Type,
            Fee = offer.Fee,
            Wage = player.Wage,
            Status = offer.Status ?? player.Status,
            RequiresResponse = offer.RequiresResponse
        };
    }
}
