using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.DressingRoom;

namespace FootballManager.Application.Services;

public interface ISquadDynamicsService
{
    Task<Result<SquadDynamics>> GetSquadDynamicsAsync(CancellationToken cancellationToken = default);
}

public class SquadDynamicsService(ISquadDynamicsGameClient gameClient) : ISquadDynamicsService
{
    public async Task<Result<SquadDynamics>> GetSquadDynamicsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetSquadDynamicsAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SquadDynamics>(new GameUnavailableError(exception.Message));
        }
    }
}
