using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Injuries;
using FootballManager.Application.Services;
using FootballManager.Domain.Injuries;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class InjuriesController(IInjuriesService injuriesService) : ControllerBase
{
    /// <summary>
    /// The medical centre, read live from the running game: each first-team player's injury history, injuries this
    /// season and injury risk, and the team's season summary with who's expected back for the next match.
    /// </summary>
    /// <param name="playerId">Only this first-team player's record (FM id).</param>
    [HttpGet]
    public async Task<Results<Ok<InjuriesResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] int? playerId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await injuriesService.GetInjuriesAsync(playerId, cancellationToken);
        return result.ToGetResult<MedicalCentre, InjuriesResponse>(centre => centre.Adapt<InjuriesResponse>());
    }
}
