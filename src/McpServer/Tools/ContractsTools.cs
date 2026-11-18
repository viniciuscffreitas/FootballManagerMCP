using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class ContractsTools
{
    [McpServerTool(Name = "get_contracts")]
    [Description("The contracts of the user's first team in their running Football Manager 27 save, read live from the " +
                 "game. Read only. Returns one entry per player: FM id, name, wage as the game writes it with its " +
                 "period (e.g. \"R$80 mil p/s\"), endDate (yyyy-MM-dd, null without one), timeRemaining (e.g. \"2 anos, " +
                 "291 dias\"), type, agreedPlayingTime (the playing time promised, e.g. \"Goleiro titular\"), " +
                 "releaseClause (the minimum fee release clause as written, null when there is none), clausesCount, " +
                 "hasRenewalOffer, and staffAdvice (the staff's advice on renewing, e.g. \"Não oferecer contrato\"). " +
                 "Money is in the save's currency and texts in the game's display language. Fails with a 503 when the " +
                 "game isn't running or no save is loaded.")]
    public static async Task<string> GetContracts(
        IHttpClientFactory httpFactory,
        [Description("Optional, yyyy-MM-dd: only contracts ending on or before this day, e.g. the season's end. " +
                     "Contracts without an end date are left out.")]
        string? expiringBy = null)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var path = expiringBy is null ? "/api/v1/contracts" : $"/api/v1/contracts?expiringBy={Uri.EscapeDataString(expiringBy)}";
        var response = await http.GetAsync(path);
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "release_player")]
    [Description("Let one of the user's own players go, through his menu in the running game. how: \"Free\" releases " +
                 "him on a free transfer at once (the club pays off what's left of his contract), \"Mutual\" offers to " +
                 "end his contract by mutual consent (he accepts or refuses), \"EndOfContract\" marks him to leave when " +
                 "his contract ends. Player by FM id from get_squad or get_contracts. By default a dry run: it reads " +
                 "his menu and returns the entry it would pick and the game's confirmation in the game's language (for " +
                 "a release, its cost), writing nothing. A release CAN'T BE UNDONE: show the user the player, how and " +
                 "the cost, and apply only once the user has explicitly agreed, with apply=true AND confirmRelease=true. " +
                 "Applying copies the real save first, picks the entry as a click does and answers \"Yes\" to the " +
                 "game's confirmation dialog (dialogAnswered has its text); refused while another dialog is open. " +
                 "stillInSquad says whether the first team still lists him afterwards. Fails with a clear error when " +
                 "the game's menu doesn't offer that for him now. Refused while a match is being played.")]
    public static async Task<string> ReleasePlayer(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id.")] int playerId,
        [Description("Free, Mutual or EndOfContract.")] string how,
        [Description("False (the default) only checks. True lets him go: only once the user has agreed.")] bool apply = false,
        [Description("Needed with apply=true: true once the user has explicitly agreed to let him go at the dry run's terms.")]
        bool confirmRelease = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/contracts/release?apply={Lower(apply)}&confirmRelease={Lower(confirmRelease)}", new { playerId, how });
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_squad_status")]
    [Description("Change the squad status (the playing time his contract promises, get_contracts' agreedPlayingTime) of " +
                 "one of the user's own players, through his menu in the running game. status is the status as the " +
                 "game's menu labels it, in the game's language (e.g. \"Rotação\"); accents and case don't matter. By " +
                 "default a dry run: it returns his current status, every status the menu offers him (choices; his own " +
                 "is greyed out) and the game's confirmation, writing nothing. Lowering a promised status can upset him: " +
                 "show the user first. Apply with apply=true: it copies the real save first, picks the status as a click " +
                 "does and answers the game's confirmation dialog; agreedPlayingTimeAfter is read back from his " +
                 "contract. Fails with the menu's statuses when none matches. Refused while a match is being played.")]
    public static async Task<string> SetSquadStatus(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id.")] int playerId,
        [Description("The new status, as the game's menu labels it.")] string status,
        [Description("False (the default) only checks. True changes it: only once the user has agreed.")] bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync($"/api/v1/contracts/squad-status?apply={Lower(apply)}", new { playerId, status });
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "hand_renewal_to_dof")]
    [Description("Hand the renewal of one of the user's own players' contracts to the director of football, through his " +
                 "menu in the running game: the DoF then negotiates it. For the user to negotiate it himself, use " +
                 "negotiate_contract instead. By default a dry run: it returns the menu entry it would pick and the " +
                 "game's confirmation, writing nothing. Apply with apply=true once the user agrees: it copies the real " +
                 "save first, picks the entry as a click does and answers the game's confirmation dialog. Fails with a " +
                 "clear error when the game's menu doesn't offer it for him now (e.g. already handed over). Refused " +
                 "while a match is being played.")]
    public static async Task<string> HandRenewalToDof(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id.")] int playerId,
        [Description("False (the default) only checks. True hands it over: only once the user has agreed.")] bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync($"/api/v1/contracts/dof-renewal?apply={Lower(apply)}", new { playerId });
        return await response.ReadContentOrError();
    }

    private static string Lower(bool value)
    {
        return value.ToString().ToLowerInvariant();
    }
}
