using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Opposition;

namespace FootballManager.Application.Services;

public interface IOppositionService
{
    /// <summary>
    /// The game's report on an opponent: the user's next one when <paramref name="team"/> is null, else a team of the
    /// user's competitions named in the game's language or by its FM id.
    /// </summary>
    Task<Result<OppositionReport>> GetReportAsync(string? team, CancellationToken cancellationToken = default);
}

// The game's pre-match analysis of an opponent. The game assesses the threat only of the user's next opponent, so a
// report on another team has none.
public class OppositionService(ITeamsGameClient teamsClient, IOppositionGameClient gameClient) : IOppositionService
{
    public async Task<Result<OppositionReport>> GetReportAsync(string? team, CancellationToken cancellationToken = default)
    {
        try
        {
            var next = await gameClient.GetNextOpponentAsync(cancellationToken);
            int? teamId = null;

            if (!string.IsNullOrWhiteSpace(team))
            {
                var found = TeamsService.Find(team, await teamsClient.GetTeamsAsync(cancellationToken));

                if (found.IsFailed)
                {
                    return found.ToResult<OppositionReport>();
                }

                teamId = found.Value.Id;
            }
            else if (next is null)
            {
                return Result.Fail<OppositionReport>(new EntityDoesNotExistError());
            }

            var isNext = next is not null && (teamId is null || teamId == next.TeamId);
            var report = await gameClient.GetReportAsync(isNext ? null : teamId, cancellationToken);

            report.IsNextOpponent = isNext;
            report.ThreatLevel = isNext ? next!.ThreatLevel : null;

            return Result.Ok(report);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<OppositionReport>(new GameUnavailableError(exception.Message));
        }
    }
}
