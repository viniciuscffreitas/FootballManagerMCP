using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Messages;
using FootballManager.Application.Services;
using FootballManager.Domain.Messages;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class NewsController(INewsService newsService) : ControllerBase
{
    /// <summary>The newest news about the user's club and his league, read live from the running game, newest first.</summary>
    /// <param name="limit">At most this many stories of each (1-50, 10 by default).</param>
    /// <param name="includeBodies">Add each story's text.</param>
    [HttpGet]
    public async Task<Results<Ok<NewsResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] int? limit = null,
        [FromQuery] bool includeBodies = false,
        CancellationToken cancellationToken = default)
    {
        var result = await newsService.GetNewsAsync(limit, includeBodies, cancellationToken);
        return result.ToGetResult<News, NewsResponse>(news => news.Adapt<NewsResponse>());
    }
}
