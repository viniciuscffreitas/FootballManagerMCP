using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Staff;
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
public class StaffController(IStaffService staffService, IStaffEditingService staffEditing) : ControllerBase
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

    /// <summary>
    /// Runs a staff menu action as the game's staff screen offers it: on a first-team staff member (sack, mutual
    /// termination, coaching or leadership course, new contract) or on the club (job adverts). A dry run that lists the
    /// menu and the entry it would pick unless apply=true.
    /// </summary>
    [HttpPost("actions")]
    public async Task<Results<Ok<StaffActionResponse>, BadRequest, NotFound, ProblemHttpResult>> RunActionAsync(
        [FromBody] StaffActionRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await staffEditing.RunActionAsync(request.StaffId, request.Action, request.Entry, apply, cancellationToken);
        return result.ToGetResult<StaffAction, StaffActionResponse>(action => action.Adapt<StaffActionResponse>());
    }
}
