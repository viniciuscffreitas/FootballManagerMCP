using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Matches;
using FootballManager.Api.Responses.Matches;
using FootballManager.Application.Services;
using FootballManager.Domain.Matches;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

/// <summary>
/// Changes to the user's match while it's being played. Each is a dry run unless apply=true; a write checks the save
/// first and answers with the match read again.
/// </summary>
[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/matches/live")]
public class LiveMatchController(ILiveMatchControlService control) : ControllerBase
{
    /// <summary>Brings a substitute on for a player on the pitch; the game makes it at the next stoppage.</summary>
    [HttpPost("substitution")]
    public async Task<Results<Ok<LiveMatchActionResponse>, BadRequest, NotFound, ProblemHttpResult>> SubstituteAsync(
        [FromBody] SubstitutionRequest request, [FromQuery] bool apply = false, CancellationToken cancellationToken = default)
    {
        var result = await control.SubstituteAsync(request.PlayerOffId, request.PlayerOnId, apply, cancellationToken);
        return result.ToGetResult<LiveMatchAction, LiveMatchActionResponse>(action => action.Adapt<LiveMatchActionResponse>());
    }

    /// <summary>Changes the mentality of the tactic the match is played with.</summary>
    [HttpPost("mentality")]
    public async Task<Results<Ok<LiveMatchActionResponse>, BadRequest, NotFound, ProblemHttpResult>> SetMentalityAsync(
        [FromBody] LiveMentalityRequest request, [FromQuery] bool apply = false, CancellationToken cancellationToken = default)
    {
        var result = await control.SetMentalityAsync(request.Mentality, apply, cancellationToken);
        return result.ToGetResult<LiveMatchAction, LiveMatchActionResponse>(action => action.Adapt<LiveMatchActionResponse>());
    }

    /// <summary>Changes a player's role on the pitch, with or without the ball.</summary>
    [HttpPost("role")]
    public async Task<Results<Ok<LiveMatchActionResponse>, BadRequest, NotFound, ProblemHttpResult>> SetRoleAsync(
        [FromBody] LiveRoleRequest request, [FromQuery] bool apply = false, CancellationToken cancellationToken = default)
    {
        var result = await control.SetRoleAsync(request.PlayerId, request.InPossession, request.Role, apply, cancellationToken);
        return result.ToGetResult<LiveMatchAction, LiveMatchActionResponse>(action => action.Adapt<LiveMatchActionResponse>());
    }

    /// <summary>Reads the team talk on screen and, with apply=true, says a line of it to the squad.</summary>
    [HttpPost("team-talk")]
    public async Task<Results<Ok<LiveMatchActionResponse>, BadRequest, NotFound, ProblemHttpResult>> TeamTalkAsync(
        [FromBody] TeamTalkRequest request, [FromQuery] bool apply = false, CancellationToken cancellationToken = default)
    {
        var result = await control.TeamTalkAsync(request.Line, apply, cancellationToken);
        return result.ToGetResult<LiveMatchAction, LiveMatchActionResponse>(action => action.Adapt<LiveMatchActionResponse>());
    }
}
