using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Finances;

namespace FootballManager.Application.Services;

public interface IFinancesService
{
    Task<Result<ClubFinances>> GetFinancesAsync(CancellationToken cancellationToken = default);
}

public class FinancesService(IFinancesGameClient gameClient) : IFinancesService
{
    public async Task<Result<ClubFinances>> GetFinancesAsync(CancellationToken cancellationToken = default)
    {
        FinanceReadout readout;

        try
        {
            readout = await gameClient.GetFinancesAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ClubFinances>(new GameUnavailableError(exception.Message));
        }

        var currency = ScreenCurrency(readout.WageGroups);

        return Result.Ok(new ClubFinances
        {
            Balance = Format(currency, readout.Balance),
            FinancialStatus = readout.FinancialStatus,
            TransferBudget = Format(currency, readout.TransferBudget),
            TransferBudgetNextSeason = Format(currency, readout.TransferBudgetNextSeason),
            TransferSpendingThisSeason = Format(currency, readout.TransferSpendingThisSeason),
            IncomeThisMonth = Format(currency, readout.IncomeThisMonth),
            ExpenditureThisMonth = Format(currency, readout.ExpenditureThisMonth),
            Wages = readout.Wages,
            // The game lists every kind of player, the ones the squad has none of included.
            WageGroups = readout.WageGroups.Where(group => group.Players > 0).ToList(),
            IncomeLines = readout.IncomeLines,
            ExpenditureLines = readout.ExpenditureLines,
            NotableIncome = readout.NotableIncome,
            BalanceHistory = Entries(currency, readout.BalanceHistory),
            BalanceProjection = Entries(currency, readout.BalanceProjection),
            ConversionRate = currency == DisplayCurrency.Pounds ? null : Math.Round(currency.Rate, 4)
        });
    }

    // The wage table writes each group's total in full and sends it in pounds too; the biggest total gives the most
    // precise rate. The game has no other way to say what a pound is worth on its screens.
    private static DisplayCurrency ScreenCurrency(List<WageGroup> groups)
    {
        return groups
            .OrderByDescending(group => group.TotalWagesInGameUnit)
            .Select(group => DisplayCurrency.From(group.TotalWages, group.TotalWagesInGameUnit))
            .FirstOrDefault(currency => currency is not null) ?? DisplayCurrency.Pounds;
    }

    private static string? Format(DisplayCurrency currency, long? pounds)
    {
        return pounds is { } amount ? currency.Format(amount) : null;
    }

    private static List<BalanceEntry> Entries(DisplayCurrency currency, List<BalanceAt> points)
    {
        return points.Select(point => new BalanceEntry { When = point.When, Balance = currency.Format(point.Amount) }).ToList();
    }
}
