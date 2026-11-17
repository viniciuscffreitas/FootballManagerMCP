using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Staff;
using FootballManager.Application.Services;
using FootballManager.Domain.Staff;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class StaffController(IStaffService staffService) : ControllerBase
{
    /// <summary>
    /// The first team's staff, read live from the running game: each member's age, job, coaching style, coaching and
    /// medical attributes, wage and contract end, with the club's staff counts by department and its open jobs.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<StaffResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await staffService.GetStaffAsync(cancellationToken);
        return result.ToGetResult<TeamStaff, StaffResponse>(staff => staff.Adapt<StaffResponse>());
    }
}
