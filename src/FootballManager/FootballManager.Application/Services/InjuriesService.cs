using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Injuries;

namespace FootballManager.Application.Services;

public interface IInjuriesService
{
    /// <param name="playerId">Only this first-team player's record (FM id); every player's when null.</param>
    Task<Result<MedicalCentre>> GetInjuriesAsync(int? playerId, CancellationToken cancellationToken = default);
}

public class InjuriesService(IInjuriesGameClient gameClient) : IInjuriesService
{
    public async Task<Result<MedicalCentre>> GetInjuriesAsync(int? playerId, CancellationToken cancellationToken = default)
    {
        MedicalCentre centre;

        try
        {
            centre = await gameClient.GetMedicalCentreAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<MedicalCentre>(new GameUnavailableError(exception.Message));
        }

        if (playerId is not { } id)
        {
            return Result.Ok(centre);
        }

        var player = centre.Players.FirstOrDefault(record => record.PlayerId == id);

        if (player is null)
        {
            return Result.Fail<MedicalCentre>(new EntityDoesNotExistError());
        }

        centre.Players = [player];
        return Result.Ok(centre);
    }
}
