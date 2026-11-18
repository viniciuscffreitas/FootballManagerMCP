using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class BoardTools
{
    [McpServerTool(Name = "get_board")]
    [Description("Where the user stands with the club in their running Football Manager 27 save, read live from the " +
                 "game's club vision screen. Returns: board (confidence grade, e.g. 6 for \"contente\", the overall " +
                 "summary and the texts on matches, squad support, tactics, the club vision and transfers, plus the " +
                 "board's highlights, i.e. praise, and criticism); supporters (confidence grade, e.g. 3, and the " +
                 "texts on the overall picture, matches and the vision); thisYearObjective and finalObjective (the " +
                 "club vision's objective for the current season and for its final year: name, year as the game " +
                 "words it, and the game's progress and status numbers, whose scales aren't confirmed); " +
                 "supporterCulture and boardCulture (what each wants from the club, one item per line); " +
                 "boardRequests and activeBoardRequests (counts of requests made to the board); and promises (made " +
                 "to the board, empty when none). Read only. Texts are in the game's display language. Fails with a " +
                 "503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetBoard(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/board");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "request_from_board")]
    [Description("Asks the board for something in the user's running Football Manager 27 save, through the club's " +
                 "menu as the board screen does. action: Request (\"Pedido à Diretoria\": opens the talk with the " +
                 "board, whose lines are the requests: facilities, youth set-up, staff and the rest), IncreaseBudgets " +
                 "(entries \"Aumentar Orçamento de Salários\" and \"Aumentar orçamento de transferências\"), " +
                 "ManagerNewContract (a new contract for the user), AffiliateLink or SeniorAffiliateLink. The game " +
                 "offers only what the board will hear now: an action it doesn't offer fails with the ones it does " +
                 "(offered lists them; one missing usually means a request is open or was refused recently). By " +
                 "default a dry run that returns the menu's entries in the game's language, the entry it would pick " +
                 "and the game's notice (\"A diretoria entrará em contato...\"). When the menu has several entries, " +
                 "name one by its label in entry. Show the user the plan and ask. To apply, call again with " +
                 "apply=true: the save is checked (and the real one copied) first; card then holds the talk the entry " +
                 "opened, if any: go on with answer_board. The board answers later in the inbox; get_board counts the " +
                 "requests. Don't run it while a match is being played.")]
    public static async Task<string> RequestFromBoard(
        IHttpClientFactory httpFactory,
        [Description("Request, IncreaseBudgets, ManagerNewContract, AffiliateLink or SeniorAffiliateLink.")]
        string action,
        [Description("The menu entry to pick, by its label as the dry run lists it.")]
        string? entry = null,
        [Description("False (the default) only checks. True sends it: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync($"/api/v1/board/requests?apply={apply.ToString().ToLowerInvariant()}", new { action, entry });
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "answer_board")]
    [Description("Says a line in the talk with the board open in the user's running Football Manager 27 save: a " +
                 "request opened by request_from_board, or the club vision and season expectations meeting " +
                 "(\"Reunião de visão e expectativas do clube\"), one line at a time. By default a dry run that " +
                 "returns the lines on screen (card.lines) and the one it would say; fails when no talk is open. " +
                 "Name the line by its number from 0 or a piece of its text. Show the user the lines and ask: a " +
                 "promise or an expectation agreed here binds the season. To say it, call again with apply=true: the " +
                 "save is checked (and the real one copied) first; after holds the next lines, null once the talk " +
                 "closed. Don't run it while a match is being played.")]
    public static async Task<string> AnswerBoard(
        IHttpClientFactory httpFactory,
        [Description("The line to say: its number from 0 or a piece of its text.")]
        string? line = null,
        [Description("False (the default) only checks. True says it: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync($"/api/v1/board/conversation?apply={apply.ToString().ToLowerInvariant()}", new { line });
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "adjust_budget")]
    [Description("Moves money between the transfer and the wage budget in the user's running Football Manager 27 save, " +
                 "with the game's budget adjustment panel (\"Ajustar Orçamento de Transferências e Salários\"). By " +
                 "default a dry run that returns both budgets as the club's menu sums them up (\"R$30 MI / R$52.5 mil " +
                 "p/s\"); fails when the menu doesn't offer the adjustment now. apply=true without steps opens the " +
                 "panel and returns it (before: slider position, transfer and wage budget, whether the board lets them " +
                 "move; applied stays false). apply=true with steps (1 to 20, either sign) moves the split by that many of the game's own increments (positive: " +
                 "transfers to wages, negative: back) and confirms; after holds the panel then. The save is checked " +
                 "(and the real one copied) before any apply. Show the user the plan and ask. Don't run it while a " +
                 "match is being played.")]
    public static async Task<string> AdjustBudget(
        IHttpClientFactory httpFactory,
        [Description("Steps of the game's own increment; positive moves money from the transfer to the wage budget.")]
        int? steps = null,
        [Description("False (the default) only checks. True opens the panel and, with steps, moves the split: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync($"/api/v1/board/budget?apply={apply.ToString().ToLowerInvariant()}", new { steps });
        return await response.ReadContentOrError();
    }
}
