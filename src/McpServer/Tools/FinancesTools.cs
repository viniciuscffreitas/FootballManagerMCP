using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class FinancesTools
{
    [McpServerTool(Name = "get_finances")]
    [Description("The user's club finances in their running Football Manager 27 save, read live from the game's finance " +
                 "screen. Every amount is text in the currency the game's screens show (e.g. \"R$8.01 MI\"). Returns: " +
                 "balance and financialStatus (the game's word for it, e.g. \"Instável\"); transferBudget, " +
                 "transferBudgetNextSeason, transferSpendingThisSeason; incomeThisMonth and expenditureThisMonth " +
                 "(totals); wages (committed weekly bill, budget, remaining, next season's budget, % of the budget in " +
                 "use); wageGroups (the bill by squad status: players, total, average, highest, lowest, the most the " +
                 "game advises paying, over wages; \"Geral\" is the whole squad); incomeLines and expenditureLines " +
                 "(this month, line by line, with its % of the month and change from last month in %); notableIncome " +
                 "(month, season, last season); balanceHistory (month by month) and balanceProjection (end of this " +
                 "season and the next two). The game sends balance, budgets, totals, history and projection only in " +
                 "its own unit (pounds): those are converted at conversionRate, read from the game's wage table, and " +
                 "may differ from the screen by a few units; when null they are in pounds (£). Read only. Fails with " +
                 "a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetFinances(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/finances");
        return await response.ReadContentOrError();
    }
}
