namespace FootballManager.Infrastructure.Constants;

// The finance screen, read on the user's club. Plain numbers (balance, budgets, totals, history, projection) are in the
// game's unit, pounds sterling; the wages data and the breakdowns also carry the texts the screens show.
// FinanceTotals* never answer on the club.
internal static class FinancesConstants
{
    public const string Club = "Club";

    public const string Balance = "OverallBalance";
    public const string FinancialStatus = "FinancialStatus.DisplayValue";
    public const string TransferBudget = "TransferBudget";
    public const string TransferBudgetNextSeason = "TransferBudgetNextSeason";
    public const string TransferSpendingThisSeason = "TransferExpenditureThisSeason";
    public const string IncomeThisMonth = "TotalIncomeThisMonth";
    public const string ExpenditureThisMonth = "TotalExpenditureThisMonth";
    public const string Wages = "WagesData";
    public const string IncomeLines = "IncomeBreakdownThisMonth";
    public const string ExpenditureLines = "ExpenditureBreakdownThisMonth";
    public const string NotableIncome = "NotableIncomeBreakdown";
    public const string BalanceHistory = "FinanceBalanceHistory";
    public const string BalanceProjection = "ProjectionBalance";

    public static readonly string[] Properties =
    [
        Balance, FinancialStatus, TransferBudget, TransferBudgetNextSeason, TransferSpendingThisSeason, IncomeThisMonth,
        ExpenditureThisMonth, Wages, IncomeLines, ExpenditureLines, NotableIncome, BalanceHistory, BalanceProjection
    ];
}
