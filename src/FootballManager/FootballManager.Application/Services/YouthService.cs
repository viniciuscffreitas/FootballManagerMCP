using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Youth;

namespace FootballManager.Application.Services;

public interface IYouthService
{
    Task<Result<YouthAndLoans>> GetYouthAndLoansAsync(CancellationToken cancellationToken = default);
}

public class YouthService(IYouthGameClient gameClient) : IYouthService
{
    public async Task<Result<YouthAndLoans>> GetYouthAndLoansAsync(CancellationToken cancellationToken = default)
    {
        YouthAndLoans read;

        try
        {
            read = await gameClient.GetYouthAndLoansAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<YouthAndLoans>(new GameUnavailableError(exception.Message));
        }

        // The game keeps a youth player out on loan in his youth team's list; the loans list says where he plays.
        var loanClubs = read.Loans.ToDictionary(loan => loan.Id, loan => loan.Club);

        foreach (var player in read.YouthPlayers)
        {
            player.OnLoan = loanClubs.TryGetValue(player.Id, out var club);
            player.LoanClub = club;
        }

        return Result.Ok(read);
    }
}
