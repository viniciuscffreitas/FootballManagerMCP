using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Settings;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

public interface ISaveGuard
{
    /// <summary>
    /// Call before every write a tool applies to the game, and write nothing when it fails. Checks that the open save is
    /// the real one configured (name, club, a game date) or the test save, and that the game's version is the released
    /// one; then copies the real save to the backup folder and rotates the older copies. The test save passes without
    /// a copy or a version check. A refusal is a <see cref="SaveRefusedError"/> with the reason for the user.
    /// </summary>
    Task<Result<SaveClearance>> PrepareWriteAsync(CancellationToken cancellationToken = default);
}

// Writes through the bridge change the game in memory; the game puts them in the save file when it next saves. A copy
// of the file before each write is the state to go back to if a write goes wrong. A game patch (Game Pass updates on
// its own) can change what the bridge's writes do, so the real save stays read-only on a version nobody has tried
// on the test save yet.
public class SaveGuard(ISaveSessionGameClient game, ISaveFiles files, SaveGuardSettings settings, TimeProvider time)
    : ISaveGuard
{
    /// <summary>The game build cleared for writes on the real save, beside the copies; it overrides the settings'.</summary>
    public const string ReleasedVersionFile = "released-version.txt";

    /// <summary>How many of its own copies per club the guard keeps.</summary>
    public const int KeptBackups = 2;

    private const string SaveExtension = ".fm";

    // The guard's own copies: <club>-<yyyyMMdd>-<HHmm>-jogo-<game date>.fm. Anything else in the folder (the copy made
    // before MCPFM, the game's own backups, the user's) is never removed.
    private static readonly Regex OwnBackup = new(
        @"^(?<club>.+)-(?<stamp>\d{8}-\d{4})-jogo-\d{4}-\d{2}-\d{2}\.fm$", RegexOptions.CultureInvariant);

    public async Task<Result<SaveClearance>> PrepareWriteAsync(CancellationToken cancellationToken = default)
    {
        LoadedSave save;
        string? version;

        try
        {
            save = await game.GetLoadedSaveAsync(cancellationToken);
            version = save.Loaded && !save.IsTestSave ? await game.GetGameVersionAsync(cancellationToken) : null;
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SaveClearance>(new GameUnavailableError(exception.Message));
        }

        if (!save.Loaded)
        {
            return Refuse(save.Loading
                ? "A save is still loading: wait until it's open, then try again."
                : "No save is loaded.");
        }

        if (save.IsTestSave)
        {
            return Result.Ok(new SaveClearance
            {
                SaveName = save.Name ?? "", IsTestSave = true, Club = save.Club, GameDate = save.GameDate
            });
        }

        if (string.IsNullOrWhiteSpace(settings.Save) || string.IsNullOrWhiteSpace(settings.Club))
        {
            return Refuse("No real save is configured (SaveGuard:Save and SaveGuard:Club): only the test save takes writes.");
        }

        if (!string.Equals(save.Name, settings.Save, StringComparison.OrdinalIgnoreCase))
        {
            return Refuse($"The game has '{save.Name ?? "an unnamed save"}' open, not '{settings.Save}' or the test save. " +
                          "Nothing was written; open the right save first.");
        }

        if (!string.Equals(save.Club, settings.Club, StringComparison.OrdinalIgnoreCase))
        {
            return Refuse($"'{save.Name}' has {save.Club ?? "no club"} as the user's club, not {settings.Club}. Nothing was written.");
        }

        if (save.GameDate is not { } gameDate)
        {
            return Refuse("The game's current date couldn't be read, so the save can't be checked or copied. Nothing was written.");
        }

        var released = ReleasedVersion();

        if (version is null)
        {
            return Refuse("The game's version couldn't be read, so it can't be checked against the released one. Nothing was written.");
        }

        if (!string.Equals(version, released, StringComparison.Ordinal))
        {
            return Refuse($"The game is on version {version}, but the released one is {released ?? "not set"}. The real save " +
                          "stays read-only until the new version passes the short test on the test save and is released.");
        }

        var source = Path.Combine(settings.GamesFolder, settings.Save + SaveExtension);

        if (!files.Exists(source))
        {
            return Refuse($"There's no '{settings.Save}{SaveExtension}' in {settings.GamesFolder} to copy. Nothing was written.");
        }

        var clearance = new SaveClearance
        {
            SaveName = save.Name!, Club = save.Club, GameDate = gameDate, GameVersion = version
        };

        var club = FileSafe(settings.Club);
        var backup = Path.Combine(settings.BackupFolder,
            $"{club}-{time.GetLocalNow():yyyyMMdd-HHmm}-jogo-{gameDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}{SaveExtension}");

        try
        {
            // A copy made earlier in the same minute holds the file as it was then; keep it rather than overwrite it.
            if (!files.Exists(backup))
            {
                files.Copy(source, backup);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Refuse($"The save couldn't be copied to {backup} ({exception.Message}). Nothing was written.");
        }

        clearance.Backup = backup;
        Rotate(club, backup, clearance);
        return Result.Ok(clearance);
    }

    private string? ReleasedVersion()
    {
        var released = files.ReadText(Path.Combine(settings.BackupFolder, ReleasedVersionFile))?.Trim();
        return string.IsNullOrEmpty(released) ? settings.ReleasedVersion : released;
    }

    // Removes this club's older copies of the guard's own, past the newest few. The copy is already made, so a copy
    // that can't be removed only stays behind: it's noted, not a reason to stop the write.
    private void Rotate(string club, string backup, SaveClearance clearance)
    {
        var older = files.ListFileNames(settings.BackupFolder)
            .Select(name => (Name: name, Match: OwnBackup.Match(name)))
            .Where(file => file.Match.Success && file.Match.Groups["club"].Value == club)
            .OrderByDescending(file => file.Match.Groups["stamp"].Value, StringComparer.Ordinal)
            .ThenByDescending(file => file.Name, StringComparer.Ordinal)
            .Skip(KeptBackups)
            .Select(file => Path.Combine(settings.BackupFolder, file.Name))
            .Where(path => path != backup);

        foreach (var path in older)
        {
            try
            {
                files.Delete(path);
                clearance.RemovedBackups.Add(Path.GetFileName(path));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                clearance.Notes.Add($"An old copy, {Path.GetFileName(path)}, couldn't be removed ({exception.Message}).");
            }
        }
    }

    private static string FileSafe(string name)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(['\\', '/', ':', '*', '?', '"', '<', '>', '|']).ToHashSet();
        return new string(name.Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }

    private static Result<SaveClearance> Refuse(string reason)
    {
        return Result.Fail<SaveClearance>(new SaveRefusedError(reason));
    }
}
