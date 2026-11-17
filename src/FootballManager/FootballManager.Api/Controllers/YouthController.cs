using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Youth;
using FootballManager.Application.Services;
using FootballManager.Domain.Youth;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class YouthController(IYouthService youthService) : ControllerBase
{
    /// <summary>
    /// The club's youth set-up and highest youth team, and its players out on loan, read live from the running game:
    /// the next intake, the youth ratings, the youth players (those on loan marked) and each loan's report.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<YouthAndLoansResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await youthService.GetYouthAndLoansAsync(cancellationToken);
        return result.ToGetResult<YouthAndLoans, YouthAndLoansResponse>(youth => youth.Adapt<YouthAndLoansResponse>());
    }
}
