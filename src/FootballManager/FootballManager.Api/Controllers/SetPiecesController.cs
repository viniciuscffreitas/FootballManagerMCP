using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.SetPieces;
using FootballManager.Api.Responses.SetPieces;
using FootballManager.Application.Services;
using FootballManager.Domain.SetPieces;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SetPiecesController(ISetPiecesService setPiecesService) : ControllerBase
{
    /// <summary>The captain, the vice, the armband's stand-ins and every set piece taker list, read live.</summary>
    [HttpGet]
    public async Task<Results<Ok<SetPieceSetupResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await setPiecesService.GetAsync(cancellationToken);
        return result.ToGetResult<SetPieceSetup, SetPieceSetupResponse>(setup => setup.Adapt<SetPieceSetupResponse>());
    }

    /// <summary>Makes a player captain and/or another vice-captain; only with apply=true.</summary>
    [HttpPost("captains")]
    public async Task<Results<Ok<SetPieceEditResponse>, BadRequest, NotFound, ProblemHttpResult>> SetCaptainsAsync(
        [FromBody] SetCaptainsRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await setPiecesService.SetCaptainsAsync(request.CaptainId, request.ViceCaptainId, apply, cancellationToken);
        return result.ToGetResult<SetPieceEdit, SetPieceEditResponse>(edit => edit.Adapt<SetPieceEditResponse>());
    }

    /// <summary>Puts players at the top of one taker list, in order; only with apply=true.</summary>
    [HttpPost("takers")]
    public async Task<Results<Ok<SetPieceEditResponse>, BadRequest, NotFound, ProblemHttpResult>> SetTakersAsync(
        [FromBody] SetTakersRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await setPiecesService.SetTakersAsync(request.List, request.PlayerIds, apply, cancellationToken);
        return result.ToGetResult<SetPieceEdit, SetPieceEditResponse>(edit => edit.Adapt<SetPieceEditResponse>());
    }

    /// <summary>Has the staff pick every set piece taker; only with apply=true.</summary>
    [HttpPost("takers/staff")]
    public async Task<Results<Ok<SetPieceEditResponse>, BadRequest, NotFound, ProblemHttpResult>> AskStaffAsync(
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await setPiecesService.AskStaffAsync(apply, cancellationToken);
        return result.ToGetResult<SetPieceEdit, SetPieceEditResponse>(edit => edit.Adapt<SetPieceEditResponse>());
    }
}
