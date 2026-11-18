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
[Route("api/v{version:apiVersion}/transfers/target")]
public class TransferTargetsController(ITransferTargetService targetService) : ControllerBase
{
    /// <summary>
    /// Withdraws the offer made for a player of another club, or marks or unmarks him as a director of football's
    /// transfer target, through his menu in the game. Only with apply=true; a dry run shows the menu's entry and where
    /// he stands.
    /// </summary>
    [HttpPost]
    public async Task<Results<Ok<TransferTargetResponse>, BadRequest, NotFound, ProblemHttpResult>> ChangeAsync(
        [FromBody] TransferTargetRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await targetService.ChangeAsync(request.PlayerId, request.Action, apply, cancellationToken);
        return result.ToGetResult<TransferTargetChange, TransferTargetResponse>(change => change.Adapt<TransferTargetResponse>());
    }
}
