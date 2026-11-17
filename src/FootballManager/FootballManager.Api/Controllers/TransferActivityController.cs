using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Transfers;
using FootballManager.Application.Services;
using FootballManager.Domain.Transfers;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/transfer-activity")]
public class TransferActivityController(ITransferActivityService transferActivityService) : ControllerBase
{
    /// <summary>
    /// The club's transfers under way and the offers it has received, read live from the game's transfer activity
    /// screen: its counts, the deals' values, and each offer's player, other club, type, fee and status.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<TransferActivityResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await transferActivityService.GetTransferActivityAsync(cancellationToken);
        return result.ToGetResult<TransferActivity, TransferActivityResponse>(activity => activity.Adapt<TransferActivityResponse>());
    }
}
