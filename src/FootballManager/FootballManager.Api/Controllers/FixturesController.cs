using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Matches;
using FootballManager.Application.Services;
using FootballManager.Domain.Matches;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class FixturesController(IMatchesService matchesService) : ControllerBase
{
    /// <summary>The user's calendar, read live from the running game: the next matches in every competition.</summary>
    /// <param name="count">How many matches, 1 to 50.</param>
    [HttpGet]
    public async Task<Results<Ok<TeamFixturesResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] int count = MatchesService.DefaultUpcoming,
        CancellationToken cancellationToken = default)
    {
        var result = await matchesService.GetUpcomingAsync(count, cancellationToken);
        return result.ToGetResult<TeamFixtures, TeamFixturesResponse>(f => f.Adapt<TeamFixturesResponse>());
    }
}
