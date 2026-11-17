using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Finances;
using FootballManager.Application.Services;
using FootballManager.Domain.Finances;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class FinancesController(IFinancesService financesService) : ControllerBase
{
    /// <summary>
    /// The user's club finances, read live from the running game, in the currency its screens show: balance and the
    /// board's view of it, transfer and wage budgets, the wage bill by kind of player, this month's income and
    /// expenditure line by line, the main sources of income, the balance month by month and its projection.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<FinancesResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await financesService.GetFinancesAsync(cancellationToken);
        return result.ToGetResult<ClubFinances, FinancesResponse>(finances => finances.Adapt<FinancesResponse>());
    }
}
