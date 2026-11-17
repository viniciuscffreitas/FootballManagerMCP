using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Rotation;
using FootballManager.Application.Services;
using FootballManager.Domain.Rotation;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class RotationController(IRotationService rotationService) : ControllerBase
{
    /// <summary>
    /// Rotates the active tactic's eleven for the next match: starters who are injured, banned or tired (condition
    /// under 85) go out for available reserves with condition 92+, match-sharp ones (75+) first; a tired starter only
    /// for a reserve rated at most 2 below him in the slot's role. Each slot keeps its position and role. Picks the
    /// reserves through set_tactic only with apply=true.
    /// </summary>
    [HttpPost]
    public async Task<Results<Ok<RotationPlanResponse>, BadRequest, NotFound, ProblemHttpResult>> RotateAsync(
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await rotationService.SuggestAsync(apply, cancellationToken);
        return result.ToGetResult<RotationPlan, RotationPlanResponse>(plan => plan.Adapt<RotationPlanResponse>());
    }
}
