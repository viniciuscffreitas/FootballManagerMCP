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
public class TransfersController(ITransferService transferService, IPlayerExitService exitService) : ControllerBase
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

    /// <summary>
    /// Changes how one of the club's own players may leave through his menu in the game: his transfer status, taking
    /// him off the transfer or loan list, offering him to clubs, recalling or extending a loan. Only with apply=true; a
    /// dry run lists the menu's entries and where he stands.
    /// </summary>
    [HttpPost("exit")]
    public async Task<Results<Ok<PlayerExitResponse>, BadRequest, NotFound, ProblemHttpResult>> ChangeExitAsync(
        [FromBody] PlayerExitRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await exitService.ChangeAsync(request.PlayerId, request.Action, request.Choice, apply, cancellationToken);
        return result.ToGetResult<PlayerExit, PlayerExitResponse>(exit => exit.Adapt<PlayerExitResponse>());
    }
}
