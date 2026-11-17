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

    /// <summary>The .fm files in the backup folder, newest first, with the club, copy time and game date their names carry.</summary>
    SaveBackupList ListBackups();

    /// <summary>
    /// Puts a copy from the backup folder back over the real save, keeping the save as it was beside the copies first.
    /// Refused while the real save is open (or a save is loading) in the game. A dry run unless <paramref name="apply"/>.
    /// </summary>
    Task<Result<SaveRestore>> RestoreAsync(string backup, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the running game's version for writes on the real save by writing it to the released-version file. A dry
    /// run unless <paramref name="apply"/>: the game's version against the released one.
    /// </summary>
    Task<Result<VersionRelease>> ReleaseVersionAsync(bool apply, CancellationToken cancellationToken = default);
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
        @"^(?<club>.+)-(?<stamp>\d{8}-\d{4})-jogo-(?<date>\d{4}-\d{2}-\d{2})\.fm$", RegexOptions.CultureInvariant);

    // The real save as it was before a restore replaced it: <club>-antes-de-restaurar-<yyyyMMdd>-<HHmm>.fm. Outside
    // the rotation, so it's never removed.
    private static readonly Regex BeforeRestore = new(
        @"^(?<club>.+)-antes-de-restaurar-(?<stamp>\d{8}-\d{4})\.fm$", RegexOptions.CultureInvariant);

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

    public SaveBackupList ListBackups()
    {
        var backups = files.ListFileNames(settings.BackupFolder)
            .Where(name => name.EndsWith(SaveExtension, StringComparison.OrdinalIgnoreCase))
            .Select(Describe)
            .OrderByDescending(backup => backup.CopiedAt.HasValue)
            .ThenByDescending(backup => backup.CopiedAt)
            .ThenBy(backup => backup.Name, StringComparer.Ordinal)
            .ToList();

        return new SaveBackupList { Folder = settings.BackupFolder, Backups = backups };
    }

    // The game holds an open save in memory and writes it back to the file when it saves (the autosave too): a file
    // replaced under an open save would be overwritten by the game's next save. The API can't close a save, so the
    // restore waits for the user to close it. A save loading may be the real one, so that waits as well.
    public async Task<Result<SaveRestore>> RestoreAsync(string backup, bool apply, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.Save) || string.IsNullOrWhiteSpace(settings.Club))
        {
            return Refuse<SaveRestore>("No real save is configured (SaveGuard:Save and SaveGuard:Club), so there's nothing to restore.");
        }

        // Only a save copy directly in the backup folder, by the name the folder lists: never a path elsewhere.
        if (!backup.EndsWith(SaveExtension, StringComparison.OrdinalIgnoreCase)
            || !files.ListFileNames(settings.BackupFolder).Contains(backup, StringComparer.Ordinal))
        {
            return Refuse<SaveRestore>($"There's no copy named '{backup}' in {settings.BackupFolder}; list_save_backups " +
                                       "shows the ones there. Nothing was restored.");
        }

        LoadedSave save;

        try
        {
            save = await game.GetLoadedSaveAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SaveRestore>(new GameUnavailableError(exception.Message));
        }

        if (save.Loading)
        {
            return Refuse<SaveRestore>("A save is loading in the game. Wait until it's open, close it if it's " +
                                       $"'{settings.Save}', then restore. Nothing was restored.");
        }

        if (save.Loaded && string.Equals(save.Name, settings.Save, StringComparison.OrdinalIgnoreCase))
        {
            return Refuse<SaveRestore>($"'{settings.Save}' is open in the game. Close the save in FM first (back to the " +
                                       "start screen), then restore. Nothing was restored.");
        }

        var target = Path.Combine(settings.GamesFolder, settings.Save + SaveExtension);

        if (!files.Exists(target))
        {
            return Refuse<SaveRestore>($"There's no '{settings.Save}{SaveExtension}' in {settings.GamesFolder} to keep " +
                                       "before the restore. Nothing was restored.");
        }

        var restore = new SaveRestore
        {
            Save = settings.Save,
            Restored = Describe(backup),
            KeptAs = $"{FileSafe(settings.Club)}-antes-de-restaurar-{time.GetLocalNow():yyyyMMdd-HHmm}{SaveExtension}"
        };

        if (!apply)
        {
            return Result.Ok(restore);
        }

        // The save as it is goes aside first, so the restore can be undone too; without it nothing is replaced.
        try
        {
            files.Copy(target, Path.Combine(settings.BackupFolder, restore.KeptAs));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Refuse<SaveRestore>($"The save couldn't be kept as {restore.KeptAs} ({exception.Message}). Nothing was restored.");
        }

        try
        {
            files.Replace(Path.Combine(settings.BackupFolder, backup), target);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Refuse<SaveRestore>($"{backup} couldn't be copied over '{settings.Save}{SaveExtension}' " +
                                       $"({exception.Message}). The save as it was is kept as {restore.KeptAs}.");
        }

        restore.Applied = true;
        return Result.Ok(restore);
    }

    // The short test on the test save after a game patch is done by hand; this only records its outcome.
    public async Task<Result<VersionRelease>> ReleaseVersionAsync(bool apply, CancellationToken cancellationToken = default)
    {
        string? version;

        try
        {
            version = await game.GetGameVersionAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<VersionRelease>(new GameUnavailableError(exception.Message));
        }

        if (version is null)
        {
            return Refuse<VersionRelease>("The game's version couldn't be read, so there's nothing to release.");
        }

        var release = new VersionRelease { CurrentVersion = version, ReleasedVersion = ReleasedVersion() };

        if (!apply || release.AlreadyReleased)
        {
            return Result.Ok(release);
        }

        try
        {
            files.WriteText(Path.Combine(settings.BackupFolder, ReleasedVersionFile), version);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Refuse<VersionRelease>($"{ReleasedVersionFile} couldn't be written in {settings.BackupFolder} " +
                                          $"({exception.Message}). The version wasn't released.");
        }

        release.Applied = true;
        return Result.Ok(release);
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

    private static SaveBackup Describe(string name)
    {
        if (OwnBackup.Match(name) is { Success: true } own)
        {
            return new SaveBackup
            {
                Name = name, Kind = SaveBackupKind.BeforeWrite, Club = own.Groups["club"].Value, CopiedAt = Stamp(own),
                GameDate = DateOnly.TryParseExact(own.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date) ? date : null
            };
        }

        if (BeforeRestore.Match(name) is { Success: true } kept)
        {
            return new SaveBackup
            {
                Name = name, Kind = SaveBackupKind.BeforeRestore, Club = kept.Groups["club"].Value, CopiedAt = Stamp(kept)
            };
        }

        return new SaveBackup { Name = name, Kind = SaveBackupKind.Other };
    }

    private static DateTime? Stamp(Match match)
    {
        return DateTime.TryParseExact(match.Groups["stamp"].Value, "yyyyMMdd-HHmm", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var stamp) ? stamp : null;
    }

    private static string FileSafe(string name)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(['\\', '/', ':', '*', '?', '"', '<', '>', '|']).ToHashSet();
        return new string(name.Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }

    private static Result<SaveClearance> Refuse(string reason)
    {
        return Refuse<SaveClearance>(reason);
    }

    private static Result<T> Refuse<T>(string reason)
    {
        return Result.Fail<T>(new SaveRefusedError(reason));
    }
}
