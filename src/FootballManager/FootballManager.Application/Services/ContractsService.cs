using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Contracts;

namespace FootballManager.Application.Services;

public interface IContractsService
{
    /// <param name="expiringBy">yyyy-MM-dd; when set, only contracts ending on or before that day.</param>
    Task<Result<List<SquadPlayerContract>>> GetContractsAsync(string? expiringBy = null, CancellationToken cancellationToken = default);
}

public class ContractsService(IContractsGameClient gameClient) : IContractsService
{
    public const string DateFormat = "yyyy-MM-dd";

    public async Task<Result<List<SquadPlayerContract>>> GetContractsAsync(
        string? expiringBy = null, CancellationToken cancellationToken = default)
    {
        DateOnly? lastDay = null;

        if (expiringBy is not null)
        {
            if (!DateOnly.TryParseExact(expiringBy, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
            {
                return Result.Fail<List<SquadPlayerContract>>(
                    new ValidationError(nameof(expiringBy), [$"Use the date as {DateFormat}, e.g. 2026-12-31."]));
            }

            lastDay = day;
        }

        List<SquadPlayerContract> contracts;

        try
        {
            contracts = await gameClient.GetContractsAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<List<SquadPlayerContract>>(new GameUnavailableError(exception.Message));
        }

        if (lastDay is null)
        {
            return Result.Ok(contracts);
        }

        // A contract without an end date never expires, so it never falls inside the window.
        return Result.Ok(contracts.Where(contract => contract.EndDate <= lastDay).ToList());
    }
}
