using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Contracts;
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
public class ContractsController(IContractsService contractsService, IContractActionsService contractActions) : ControllerBase
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

    /// <summary>
    /// Lets one of the club's own players go through his menu in the game: released on a free transfer (Free), his
    /// contract ended by mutual consent (Mutual), or marked to leave when it ends (EndOfContract). A dry run shows the
    /// game's confirmation (the cost); it writes only with apply=true, confirmRelease=true and that confirmation sent back
    /// as expectedConfirmation, and answers "Yes" only to the game's dialog that holds it.
    /// </summary>
    [HttpPost("release")]
    public async Task<Results<Ok<ContractChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ReleaseAsync(
        [FromBody] ReleasePlayerRequest request,
        [FromQuery] bool apply = false,
        [FromQuery] bool confirmRelease = false,
        CancellationToken cancellationToken = default)
    {
        var result = await contractActions.ReleaseAsync(
            request.PlayerId, request.How, apply, confirmRelease, request.ExpectedConfirmation, cancellationToken);
        return result.ToGetResult<ContractChange, ContractChangeResponse>(change => change.Adapt<ContractChangeResponse>());
    }

    /// <summary>Changes the squad status (promised playing time) of one of the club's own players; only with apply=true.</summary>
    [HttpPost("squad-status")]
    public async Task<Results<Ok<ContractChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> SetSquadStatusAsync(
        [FromBody] SquadStatusRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await contractActions.SetSquadStatusAsync(request.PlayerId, request.Status, apply, cancellationToken);
        return result.ToGetResult<ContractChange, ContractChangeResponse>(change => change.Adapt<ContractChangeResponse>());
    }

    /// <summary>Hands the renewal of one of the club's own players' contracts to the director of football; only with apply=true.</summary>
    [HttpPost("dof-renewal")]
    public async Task<Results<Ok<ContractChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> HandRenewalToDofAsync(
        [FromBody] DofRenewalRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await contractActions.HandRenewalToDofAsync(request.PlayerId, apply, cancellationToken);
        return result.ToGetResult<ContractChange, ContractChangeResponse>(change => change.Adapt<ContractChangeResponse>());
    }
}
