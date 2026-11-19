using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Opposition;
using FootballManager.Api.Responses.Opposition;
using FootballManager.Application.Services;
using FootballManager.Domain.Opposition;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class OppositionController(IOppositionService oppositionService, IOppositionInstructionsService instructions) : ControllerBase
{
    /// <summary>
    /// The game's opposition report, read live from the running game: the tactic and lineup it expects, its summary,
    /// the analysis' points, and for the next opponent the threat it poses.
    /// </summary>
    /// <param name="team">
    /// A team of the user's competitions, by name as the game writes it or by FM id; the next opponent when left out.
    /// </param>
    [HttpGet]
    public async Task<Results<Ok<OppositionReportResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] string? team = null,
        CancellationToken cancellationToken = default)
    {
        var result = await oppositionService.GetReportAsync(team, cancellationToken);
        return result.ToGetResult<OppositionReport, OppositionReportResponse>(report => report.Adapt<OppositionReportResponse>());
    }

    /// <summary>
    /// Changes the instructions against the next opponent as the opposition instructions screen does: one player of
    /// the lineup the game expects (Set, Clear) or the staff's suggestions (ApplyStaffAdvice). A dry run that lists the
    /// instructions now, the staff's suggestions and the writes planned, unless apply=true.
    /// </summary>
    [HttpPost("instructions")]
    public async Task<Results<Ok<OppositionInstructionsChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ChangeInstructionsAsync(
        [FromBody] OppositionInstructionsRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await instructions.ChangeAsync(request.Action, request.Player, request.Settings(), apply, cancellationToken);
        return result.ToGetResult<OppositionInstructionsChange, OppositionInstructionsChangeResponse>(
            change => change.Adapt<OppositionInstructionsChangeResponse>());
    }
}
