using Asp.Versioning;
using FootballManager.Api.Extensions;
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
public class BoardController(IBoardService boardService) : ControllerBase
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
}
