using FootballManager.Domain.Finances;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the user's club finances from the running game. Throws <see cref="Exceptions.GameUnavailableException"/>
/// when it can't.
/// </summary>
public interface IFinancesGameClient
{
    /// <summary>The finance screen: balance, budgets, wages, this month's income and expenditure, history and projection.</summary>
    Task<FinanceReadout> GetFinancesAsync(CancellationToken cancellationToken = default);
}
