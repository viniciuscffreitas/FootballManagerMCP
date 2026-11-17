using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Standings;

namespace FootballManager.Application.Services;

public interface ILeagueTableService
{
    Task<Result<LeagueTable>> GetLeagueTableAsync(CancellationToken cancellationToken = default);
}

public class LeagueTableService(ILeagueTableGameClient gameClient) : ILeagueTableService
{
    // The game has no current stage for a league between seasons, before its first round.
    private const string NoLeagueStage = "The league isn't in a stage played as a table yet (its season hasn't started).";

    public async Task<Result<LeagueTable>> GetLeagueTableAsync(CancellationToken cancellationToken = default)
    {
        LeagueTableReadout readout;

        try
        {
            readout = await gameClient.GetLeagueTableAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<LeagueTable>(new GameUnavailableError(exception.Message));
        }

        var rows = readout.HasLeagueStage ? readout.Rows.OrderBy(row => row.Position).ToList() : [];

        foreach (var row in rows)
        {
            row.IsUser = row.TeamId == readout.TeamId;
        }

        return Result.Ok(new LeagueTable
        {
            League = readout.League,
            TeamName = readout.TeamName,
            Position = rows.FirstOrDefault(row => row.IsUser)?.Position,
            MediaExpectedPosition = readout.MediaExpectedPosition,
            Rows = rows,
            Reason = rows.Count == 0 ? NoLeagueStage : null
        });
    }
}
