using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Transfers;
using FootballManager.Api.Responses.Transfers;
using FootballManager.Application.Services;
using FootballManager.Domain.Transfers;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class TransfersController(ITransferService transferService) : ControllerBase
{
    /// <summary>
    /// Makes a transfer offer for a player through the game's offer panel, its fee moved to exactly the one asked; only
    /// with apply=true. A dry run shows the player, his club and the fee.
    /// </summary>
    [HttpPost("offer")]
    public async Task<Results<Ok<TransferOfferResponse>, BadRequest, NotFound, ProblemHttpResult>> MakeOfferAsync(
        [FromBody] MakeTransferOfferRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await transferService.MakeOfferAsync(request.PlayerId, request.Fee, apply, cancellationToken);
        return result.ToGetResult<TransferOffer, TransferOfferResponse>(offer => offer.Adapt<TransferOfferResponse>());
    }

    /// <summary>
    /// Takes the contract negotiation of a player whose club accepted the offer through the game's contract panel:
    /// promises, then the contract's terms, where it stops; on those terms, offers the deal. Only with apply=true.
    /// </summary>
    [HttpPost("contract")]
    public async Task<Results<Ok<ContractNegotiationResponse>, BadRequest, NotFound, ProblemHttpResult>> NegotiateContractAsync(
        [FromBody] NegotiateContractRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await transferService.NegotiateContractAsync(request.PlayerId, apply, cancellationToken);
        return result.ToGetResult<ContractNegotiation, ContractNegotiationResponse>(
            negotiation => negotiation.Adapt<ContractNegotiationResponse>());
    }
}
