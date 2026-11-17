using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Staff;

namespace FootballManager.Application.Services;

public interface IStaffService
{
    Task<Result<TeamStaff>> GetStaffAsync(CancellationToken cancellationToken = default);
}

public class StaffService(IStaffGameClient gameClient) : IStaffService
{
    public async Task<Result<TeamStaff>> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetStaffAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TeamStaff>(new GameUnavailableError(exception.Message));
        }
    }
}
