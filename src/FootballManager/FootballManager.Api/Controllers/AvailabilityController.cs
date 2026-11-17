using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Availability;
using FootballManager.Application.Services;
using FootballManager.Domain.Availability;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class AvailabilityController(IAvailabilityService availabilityService) : ControllerBase
{
    /// <summary>
    /// Who can play the team's next match, read live from the running game: the injured with their time out, the
    /// suspended with the ban's scope, matches left and reason, the next match's squad rules, and every first-team
    /// player's condition and match sharpness.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<AvailabilityResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await availabilityService.GetAvailabilityAsync(cancellationToken);
        return result.ToGetResult<MatchAvailability, AvailabilityResponse>(availability => availability.Adapt<AvailabilityResponse>());
    }
}
