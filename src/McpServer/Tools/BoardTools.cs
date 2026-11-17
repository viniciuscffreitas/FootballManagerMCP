using System.ComponentModel;
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
}
