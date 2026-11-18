using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Board;
using FootballManager.Api.Responses.Board;
using FootballManager.Application.Services;
using FootballManager.Domain.Board;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class BoardController(IBoardService boardService, IBoardEditingService boardEditing) : ControllerBase
{
    /// <summary>
    /// Where the user stands with the club, read live from the running game: the board's and the supporters'
    /// confidence (grade and texts, the board's praise and criticism), the club vision's objectives, the supporters'
    /// and the board's culture, requests to the board and promises made to it.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<BoardResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await boardService.GetBoardAsync(cancellationToken);
        return result.ToGetResult<BoardStatus, BoardResponse>(board => board.Adapt<BoardResponse>());
    }

    /// <summary>
    /// Asks the board for something through the club's menu: the board talk (facilities, youth, staff and the other
    /// requests), a bigger wage or transfer budget, a new contract for the user, an affiliate. A dry run that lists what
    /// the menu offers now and the entry it would pick unless apply=true.
    /// </summary>
    [HttpPost("requests")]
    public async Task<Results<Ok<BoardRequestResponse>, BadRequest, NotFound, ProblemHttpResult>> RequestAsync(
        [FromBody] BoardRequestRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await boardEditing.RequestAsync(request.Action, request.Entry, apply, cancellationToken);
        return result.ToGetResult<BoardRequest, BoardRequestResponse>(plan => plan.Adapt<BoardRequestResponse>());
    }

    /// <summary>
    /// Says a line in the talk with the board open on screen (a request's, or the club vision and season expectations
    /// meeting's). A dry run that lists the lines unless apply=true.
    /// </summary>
    [HttpPost("conversation")]
    public async Task<Results<Ok<BoardAnswerResponse>, BadRequest, NotFound, ProblemHttpResult>> AnswerAsync(
        [FromBody] BoardAnswerRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await boardEditing.AnswerAsync(request.Line, apply, cancellationToken);
        return result.ToGetResult<BoardAnswer, BoardAnswerResponse>(answer => answer.Adapt<BoardAnswerResponse>());
    }

    /// <summary>
    /// Moves money between the transfer and the wage budget with the game's budget adjustment panel. A dry run with the
    /// current budgets unless apply=true; apply without steps opens the panel and reads it.
    /// </summary>
    [HttpPost("budget")]
    public async Task<Results<Ok<BudgetAdjustmentResponse>, BadRequest, NotFound, ProblemHttpResult>> AdjustBudgetAsync(
        [FromBody] BudgetAdjustmentRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await boardEditing.AdjustBudgetAsync(request.Steps, apply, cancellationToken);
        return result.ToGetResult<BudgetAdjustment, BudgetAdjustmentResponse>(plan => plan.Adapt<BudgetAdjustmentResponse>());
    }
}
