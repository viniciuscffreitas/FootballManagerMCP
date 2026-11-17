using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Standings;
using FootballManager.Application.Services;
using FootballManager.Domain.Standings;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class LeagueTableController(ILeagueTableService leagueTableService) : ControllerBase
{
    /// <summary>
    /// The user's league table, read live from the running game: every team's position, games, wins, draws, losses,
    /// goals for and against, goal difference and points, the user's line marked, and where the press expects the user's
    /// team to finish. Empty, with the reason, when the league's season hasn't started.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<LeagueTableResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await leagueTableService.GetLeagueTableAsync(cancellationToken);
        return result.ToGetResult<LeagueTable, LeagueTableResponse>(table => table.Adapt<LeagueTableResponse>());
    }
}
