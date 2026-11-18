using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class RegistrationTools
{
    [McpServerTool(Name = "get_squad_registration")]
    [Description("Get the first team's registration in each competition that asks for one, read live from the game's " +
                 "registration screen. For each: the registration's name and the competition's, whether the window is " +
                 "open now (registrations change only then) with the game's words for it and the deadline, the maximum " +
                 "squad size, whether the rules are met and the game's summary, each rule (type, the game's words, the " +
                 "squad's value against it, met or not), the registered players and the ones the game could register " +
                 "(FM id, name, squad number, goalkeeper). In some competitions (the Paulistão) the Quick Pick only " +
                 "picks registered players. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetSquadRegistration(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/squad/registration");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "register_players")]
    [Description("Register or unregister first-team players in one competition, as the game's registration screen " +
                 "does. Only while its window is open. By default a dry run: it checks each player (registered or " +
                 "not, and whether the game lets him change) and the competition's rules against the change: squad " +
                 "size and goalkeepers are worked out, with a warning for each rule the change breaks; other rules " +
                 "(foreigners, under-21s) the game checks on confirming, and the plan says so. Show the user the plan " +
                 "and ask. To apply, call again with apply=true: the save is checked (and the real one copied) first; " +
                 "a change that breaks a rule is refused. writes says, per player, Written, NotWritten or Unknown; " +
                 "confirmed says whether the registration read back has each player where asked. Don't run it while a " +
                 "match is being played.")]
    public static async Task<string> RegisterPlayers(
        IHttpClientFactory httpFactory,
        [Description("The registration's name or the competition's, as get_squad_registration lists them, e.g. 'Copa do Brasil'.")]
        string competition,
        [Description("FM ids of players to register, from the competition's unregistered list.")]
        List<int>? register = null,
        [Description("FM ids of players to unregister, from its registered list.")]
        List<int>? unregister = null,
        [Description("False (the default) only checks. True writes it: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/squad/registration?apply={apply.ToString().ToLowerInvariant()}",
            new { competition, register, unregister });
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_squad_number")]
    [Description("Give a first-team player a squad number, or ask the assistant to number everyone without one, as the " +
                 "game's squad numbers screen does. By default a dry run: it checks the player, that the game lets his " +
                 "number change and that the number is free (a taken one fails with who wears it). For the assistant, " +
                 "it says how many players have no number. Show the user the plan and ask. To apply, call again with " +
                 "apply=true: the save is checked (and the real one copied) first; write says Written, NotWritten or " +
                 "Unknown and confirmed whether the number read back is the one asked. Don't run it while a match is " +
                 "being played.")]
    public static async Task<string> SetSquadNumber(
        IHttpClientFactory httpFactory,
        [Description("The player's FM id from get_squad; leave out with askAssistant.")]
        int? playerId = null,
        [Description("The number to give him, 1 to 99; leave out with askAssistant.")]
        int? number = null,
        [Description("True asks the assistant to number everyone without a number, instead of one player.")]
        bool askAssistant = false,
        [Description("False (the default) only checks. True writes it: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/squad/numbers?apply={apply.ToString().ToLowerInvariant()}",
            new { playerId, number, askAssistant });
        return await response.ReadContentOrError();
    }
}
