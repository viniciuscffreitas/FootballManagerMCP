using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.SquadDynamics;
using FootballManager.Application.Services;
using FootballManager.Domain.DressingRoom;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SquadDynamicsController(ISquadDynamicsService squadDynamicsService) : ControllerBase
{
    /// <summary>
    /// The dressing room, read live from the running game: the hierarchy level by level, the club atmosphere and its
    /// reasons, the open happiness issues, the manager's promises to players, and each first-team player's hierarchy
    /// level and reasons to be happy or unhappy.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<SquadDynamicsResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await squadDynamicsService.GetSquadDynamicsAsync(cancellationToken);
        return result.ToGetResult<SquadDynamics, SquadDynamicsResponse>(dynamics => dynamics.Adapt<SquadDynamicsResponse>());
    }
}
