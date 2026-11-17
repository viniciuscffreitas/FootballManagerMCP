using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Contracts;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContractsController(IContractsService contractsService) : ControllerBase
{
    /// <summary>
    /// The first team's contracts, read live from the running game: wage, end date and time left, type, agreed
    /// playing time, release clause, number of clauses, whether a renewal offer is out, and the staff's renewal advice.
    /// </summary>
    /// <param name="expiringBy">yyyy-MM-dd; when set, only contracts ending on or before that day.</param>
    [HttpGet]
    public async Task<Results<Ok<List<SquadPlayerContractResponse>>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] string? expiringBy = null,
        CancellationToken cancellationToken = default)
    {
        var result = await contractsService.GetContractsAsync(expiringBy, cancellationToken);
        return result.ToGetResult<List<SquadPlayerContract>, List<SquadPlayerContractResponse>>(
            contracts => contracts.Adapt<List<SquadPlayerContractResponse>>());
    }
}
