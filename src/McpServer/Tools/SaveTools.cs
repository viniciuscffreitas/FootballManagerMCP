using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class SaveTools
{
    [McpServerTool(Name = "list_save_backups")]
    [Description("The copies of the user's career save in the backup folder (SaveGuard BackupFolder), newest first, " +
                 "read from the files' names: no change to anything. Each has name, kind, club, copiedAt (local time, " +
                 "to the minute) and gameDate (the game's date in the save when it was copied). Kinds: BeforeWrite, " +
                 "the copy made before each change a tool applied (only each club's two newest are kept); " +
                 "BeforeRestore, the save as it was before restore_save replaced it; Other, any other .fm file (the " +
                 "copy made before MCPFM, the game's own, the user's), with no club or date. BeforeRestore and Other " +
                 "are never removed. Use it to pick the copy for restore_save.")]
    public static async Task<string> ListSaveBackups(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/saves/backups");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "restore_save")]
    [Description("Put a copy from list_save_backups back over the user's career save (SaveGuard Save), only when the " +
                 "user asks to restore one. The career save must be closed in the game: the game writes an open save " +
                 "back to its file when it saves, and no tool can close a save, so while it's open (or a save is " +
                 "loading) this is refused with that reason; ask the user to close it in FM (back to the start screen) " +
                 "and not to load it until the restore is done. The investigation save may stay open. Before the copy " +
                 "goes over it, the save as it is is kept in the backup folder as <club>-antes-de-restaurar-" +
                 "<yyyyMMdd-HHmm>.fm (keptAs), never removed, so a restore can be undone the same way. By default a " +
                 "dry run: returns the copy it would restore (restored: name, club, gameDate) and keptAs. Show the user " +
                 "the copy's club and game date and ask. To apply, call again with apply=true. Returns the same with " +
                 "applied=true; tell the user the save's name, the copy's club and game date.")]
    public static async Task<string> RestoreSave(
        IHttpClientFactory httpFactory,
        [Description("The copy's file name exactly as list_save_backups shows it, e.g. \"Santos-20261117-1430-jogo-2026-03-10.fm\".")]
        string backup,
        [Description("False (the default) only checks. True replaces the save: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/saves/restore?apply={apply.ToString().ToLowerInvariant()}",
            new { backup });

        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "release_game_version")]
    [Description("Clear the running game's version for changes on the user's career save. After a game update (Game " +
                 "Pass updates on its own) every applied change to the career save is refused until its version is " +
                 "released; the investigation save takes changes on any version. Release only after the short test on " +
                 "the investigation save passed on this version: advance one day, answer one message, change the " +
                 "lineup, and check each worked. This tool runs no test; it only records the release, in " +
                 "released-version.txt in the backup folder. By default a dry run: currentVersion (the running game's " +
                 "build), releasedVersion (the one cleared now) and alreadyReleased. To apply, call again with " +
                 "apply=true; tell the user the version is released.")]
    public static async Task<string> ReleaseGameVersion(
        IHttpClientFactory httpFactory,
        [Description("False (the default) only shows the versions. True releases the running game's version: only after the short test passed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.PostAsync($"/api/v1/saves/release-version?apply={apply.ToString().ToLowerInvariant()}", null);
        return await response.ReadContentOrError();
    }
}
