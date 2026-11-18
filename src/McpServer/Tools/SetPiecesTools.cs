using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class SetPiecesTools
{
    [McpServerTool(Name = "get_set_pieces")]
    [Description("Who wears the armband and who takes each set piece in the user's running Football Manager 27 save, " +
                 "read live from the game. Read only. Returns captain and viceCaptain (FM id and name), captainStandIns " +
                 "(who takes the armband when both are off the pitch, in order) and takerLists: one per list of the " +
                 "game's set piece screen (penalties, corners left and right, free kicks), each with its key (what " +
                 "set_set_piece_takers takes), its label in the game's language, and its takers in the order the game " +
                 "tries them. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetSetPieces(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/setpieces");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_captains")]
    [Description("Make a first-team player captain and/or another vice-captain, as the player's menu does in the game. " +
                 "By default a dry run that writes nothing: it checks the players and returns before (the setup now) " +
                 "and steps (what would change). A player who already holds the role is left as he is. With " +
                 "apply=true the API first checks and copies the save (see the README's Safety), writes, and returns " +
                 "after, read back from the game. Refused when the two ids are the same player, when a player isn't in " +
                 "the first team, and when the game's menu doesn't offer the change.")]
    public static async Task<string> SetCaptains(
        IHttpClientFactory httpFactory,
        [Description("The player to make captain (FM id); leave out to keep the captain.")]
        int? captainId = null,
        [Description("The player to make vice-captain (FM id); leave out to keep the vice.")]
        int? viceCaptainId = null,
        [Description("False (the default) only checks. True writes: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/setpieces/captains?apply={apply.ToString().ToLowerInvariant()}", new { captainId, viceCaptainId });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_set_piece_takers")]
    [Description("Choose who takes a set piece: puts the players at the top of one of the set piece screen's taker " +
                 "lists, in the order given (the first takes it, the next when he's off the pitch); the rest of the " +
                 "list follows them. By default a dry run that writes nothing and returns before and steps. With " +
                 "apply=true the API first checks and copies the save (see the README's Safety), writes inside the " +
                 "screen's edit session, and returns after, read back from the game. Refused for a list the screen " +
                 "doesn't have (the error names the ones it has), a player outside the first team, or a player named twice.")]
    public static async Task<string> SetSetPieceTakers(
        IHttpClientFactory httpFactory,
        [Description("The list's key, as get_set_pieces lists it (e.g. the penalties list).")]
        string list,
        [Description("The takers' FM ids, first to take it first.")]
        int[] playerIds,
        [Description("False (the default) only checks. True writes: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/setpieces/takers?apply={apply.ToString().ToLowerInvariant()}", new { list, playerIds });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "ask_staff_set_piece_takers")]
    [Description("Have the staff pick every set piece taker, as the set piece screen's request to the assistant does. " +
                 "By default a dry run that writes nothing. With apply=true the API first checks and copies the save " +
                 "(see the README's Safety), asks, and returns before and after, read back from the game.")]
    public static async Task<string> AskStaffSetPieceTakers(
        IHttpClientFactory httpFactory,
        [Description("False (the default) only checks. True asks the staff: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsync($"/api/v1/setpieces/takers/staff?apply={apply.ToString().ToLowerInvariant()}", null);
        return await response.ReadContentOrError();
    }
}
