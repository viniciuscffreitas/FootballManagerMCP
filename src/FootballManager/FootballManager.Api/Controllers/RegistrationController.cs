using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Registration;
using FootballManager.Api.Responses.Registration;
using FootballManager.Application.Services;
using FootballManager.Domain.Registration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/squad")]
public class RegistrationController(IRegistrationService registration, ISquadNumbersService numbers) : ControllerBase
{
    /// <summary>
    /// The first team's registration in each competition, read live: window, deadline, rules (the squad's value against
    /// each), who is registered and who the game could register.
    /// </summary>
    [HttpGet("registration")]
    public async Task<Results<Ok<SquadRegistrationResponse>, BadRequest, NotFound, ProblemHttpResult>> GetRegistrationAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await registration.GetRegistrationAsync(cancellationToken);
        return result.ToGetResult<SquadRegistration, SquadRegistrationResponse>(r => r.Adapt<SquadRegistrationResponse>());
    }

    /// <summary>
    /// Registers and unregisters players in one competition. A dry run that checks the players and the rules against
    /// the change unless apply=true.
    /// </summary>
    [HttpPost("registration")]
    public async Task<Results<Ok<RegistrationChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ChangeRegistrationAsync(
        [FromBody] RegistrationChangeRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await registration.ChangeRegistrationAsync(request.Competition, request.Register, request.Unregister, apply, cancellationToken);
        return result.ToGetResult<RegistrationChange, RegistrationChangeResponse>(c => c.Adapt<RegistrationChangeResponse>());
    }

    /// <summary>
    /// Gives a player a squad number, or asks the assistant to number everyone. A dry run unless apply=true.
    /// </summary>
    [HttpPost("numbers")]
    public async Task<Results<Ok<SquadNumberChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ChangeNumberAsync(
        [FromBody] SquadNumberRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await numbers.ChangeNumberAsync(request.PlayerId, request.Number, request.AskAssistant, apply, cancellationToken);
        return result.ToGetResult<SquadNumberChange, SquadNumberChangeResponse>(c => c.Adapt<SquadNumberChangeResponse>());
    }
}
