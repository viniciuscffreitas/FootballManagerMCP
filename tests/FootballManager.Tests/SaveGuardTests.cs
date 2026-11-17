using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Application.Settings;
using FootballManager.Domain.Saves;
using FootballManager.Infrastructure.Files;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

// The guard against real folders in a temporary directory, with the game faked.
public sealed class SaveGuardTests : IDisposable
{
    private const string RealSave = "Vinicius Freitas - Santos";
    private const string Released = "27.0.1.2494769";

    private readonly string _root = Directory.CreateTempSubdirectory("saveguard-").FullName;
    private readonly ISaveSessionGameClient _game = Substitute.For<ISaveSessionGameClient>();
    private readonly SaveGuardSettings _settings;
    private readonly SaveGuard _guard;

    public SaveGuardTests()
    {
        _settings = new SaveGuardSettings
        {
            Save = RealSave,
            Club = "Santos",
            GamesFolder = Path.Combine(_root, "games"),
            BackupFolder = Path.Combine(_root, "fm-backup"),
            ReleasedVersion = Released
        };

        Directory.CreateDirectory(_settings.GamesFolder);
        File.WriteAllText(Path.Combine(_settings.GamesFolder, RealSave + ".fm"), "career");
        Loaded(RealSave, "Santos", new DateOnly(2026, 3, 10));
        Version(Released);

        _guard = new SaveGuard(_game, new LocalSaveFiles(), _settings, new FixedTime(new DateTime(2026, 11, 17, 14, 30, 0)));
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task The_real_save_is_copied_with_the_real_and_the_game_date_in_the_name()
    {
        var clearance = (await _guard.PrepareWriteAsync()).Value;

        Assert.Equal("Santos-20261117-1430-jogo-2026-03-10.fm", Path.GetFileName(clearance.Backup));
        Assert.Equal("career", File.ReadAllText(clearance.Backup!));
        Assert.Equal(new DateOnly(2026, 3, 10), clearance.GameDate);
        Assert.Equal(Released, clearance.GameVersion);
    }

    [Fact]
    public async Task Another_save_loaded_is_refused_without_a_copy()
    {
        Loaded("Someone Else - Palmeiras", "Palmeiras", new DateOnly(2026, 3, 10));

        Assert.Contains("Someone Else - Palmeiras", Refusal(await _guard.PrepareWriteAsync()));
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task The_real_save_running_another_club_is_refused()
    {
        Loaded(RealSave, "Corinthians", new DateOnly(2026, 3, 10));

        Assert.Contains("Corinthians", Refusal(await _guard.PrepareWriteAsync()));
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task Nothing_loaded_is_refused()
    {
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>()).Returns(new LoadedSave { Loaded = false, Loading = true });

        Assert.Contains("loading", Refusal(await _guard.PrepareWriteAsync()));
    }

    [Fact]
    public async Task A_save_without_a_game_date_is_refused()
    {
        Loaded(RealSave, "Santos", null);

        Assert.Contains("date", Refusal(await _guard.PrepareWriteAsync()));
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task A_new_game_version_leaves_the_real_save_read_only()
    {
        Version("27.0.2.2510000");

        var reason = Refusal(await _guard.PrepareWriteAsync());

        Assert.Contains("27.0.2.2510000", reason);
        Assert.Contains(Released, reason);
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task The_released_version_file_wins_over_the_configured_one()
    {
        Directory.CreateDirectory(_settings.BackupFolder);
        File.WriteAllText(Path.Combine(_settings.BackupFolder, SaveGuard.ReleasedVersionFile), "27.0.2.2510000\n");

        Assert.Contains("27.0.2.2510000", Refusal(await _guard.PrepareWriteAsync()));

        Version("27.0.2.2510000");
        Assert.True((await _guard.PrepareWriteAsync()).IsSuccess);
    }

    [Fact]
    public async Task An_unknown_game_version_is_refused()
    {
        Version(null);

        Assert.Contains("version", Refusal(await _guard.PrepareWriteAsync()));
    }

    [Fact]
    public async Task No_real_save_configured_refuses_every_write_outside_the_test_save()
    {
        _settings.Save = null;

        Assert.Contains("SaveGuard", Refusal(await _guard.PrepareWriteAsync()));
    }

    [Fact]
    public async Task A_real_save_missing_from_the_games_folder_is_refused()
    {
        File.Delete(Path.Combine(_settings.GamesFolder, RealSave + ".fm"));

        Assert.Contains(RealSave, Refusal(await _guard.PrepareWriteAsync()));
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task The_test_save_is_cleared_without_a_copy_whatever_the_version()
    {
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>()).Returns(new LoadedSave
        {
            Loaded = true, Name = "MCPFM investigation", IsTestSave = true, Club = "Santos", GameDate = new DateOnly(2026, 3, 10)
        });
        Version("27.0.2.2510000");

        var clearance = (await _guard.PrepareWriteAsync()).Value;

        Assert.True(clearance.IsTestSave);
        Assert.Null(clearance.Backup);
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task The_rotation_keeps_the_two_newest_copies_and_never_touches_other_files()
    {
        string[] others =
        [
            "Santos-20261116-pre-mcpfm.fm", "last save overwrite backup.fm", "Vinicius Freitas - Santos (v02).fm",
            "Santos-notes.fm", SaveGuard.ReleasedVersionFile
        ];
        string[] older = ["Santos-20261110-0900-jogo-2026-02-01.fm", "Santos-20261112-1015-jogo-2026-02-20.fm"];
        const string previous = "Santos-20261116-2200-jogo-2026-03-08.fm";

        Directory.CreateDirectory(_settings.BackupFolder);

        foreach (var name in others.Concat(older).Append(previous))
        {
            File.WriteAllText(Path.Combine(_settings.BackupFolder, name), name == SaveGuard.ReleasedVersionFile ? Released : "old");
        }

        var clearance = (await _guard.PrepareWriteAsync()).Value;

        Assert.Equal(older.Order(), clearance.RemovedBackups.Order());
        Assert.Equal(others.Append(previous).Append("Santos-20261117-1430-jogo-2026-03-10.fm").Order(), Backups().Order());
    }

    [Fact]
    public async Task The_rotation_counts_each_clubs_copies_on_their_own()
    {
        Directory.CreateDirectory(_settings.BackupFolder);
        File.WriteAllText(Path.Combine(_settings.BackupFolder, "Palmeiras-20261101-0900-jogo-2026-01-20.fm"), "other career");
        File.WriteAllText(Path.Combine(_settings.BackupFolder, "Santos-20261116-2200-jogo-2026-03-08.fm"), "old");

        var clearance = (await _guard.PrepareWriteAsync()).Value;

        Assert.Empty(clearance.RemovedBackups);
        Assert.Contains("Palmeiras-20261101-0900-jogo-2026-01-20.fm", Backups());
    }

    [Fact]
    public async Task A_copy_of_the_same_minute_is_kept_rather_than_overwritten()
    {
        Directory.CreateDirectory(_settings.BackupFolder);
        var same = Path.Combine(_settings.BackupFolder, "Santos-20261117-1430-jogo-2026-03-10.fm");
        File.WriteAllText(same, "first");

        var clearance = (await _guard.PrepareWriteAsync()).Value;

        Assert.Equal(same, clearance.Backup);
        Assert.Equal("first", File.ReadAllText(same));
    }

    [Fact]
    public async Task An_unreachable_game_fails_as_unavailable()
    {
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("closed"));

        var result = await _guard.PrepareWriteAsync();

        Assert.True(result.HasError<GameUnavailableError>());
    }

    [Fact]
    public void The_backups_are_listed_newest_first_with_what_their_names_tell()
    {
        Directory.CreateDirectory(_settings.BackupFolder);

        foreach (var name in new[]
                 {
                     "Santos-20261116-pre-mcpfm.fm", "Santos-20261112-1015-jogo-2026-02-20.fm",
                     "Santos-antes-de-restaurar-20261115-0800.fm", "Santos-20261116-2200-jogo-2026-03-08.fm",
                     SaveGuard.ReleasedVersionFile
                 })
        {
            File.WriteAllText(Path.Combine(_settings.BackupFolder, name), "old");
        }

        var list = _guard.ListBackups();

        Assert.Equal(
            ["Santos-20261116-2200-jogo-2026-03-08.fm", "Santos-antes-de-restaurar-20261115-0800.fm",
             "Santos-20261112-1015-jogo-2026-02-20.fm", "Santos-20261116-pre-mcpfm.fm"],
            list.Backups.Select(b => b.Name));

        var newest = list.Backups[0];
        Assert.Equal(SaveBackupKind.BeforeWrite, newest.Kind);
        Assert.Equal("Santos", newest.Club);
        Assert.Equal(new DateOnly(2026, 3, 8), newest.GameDate);
        Assert.Equal(new DateTime(2026, 11, 16, 22, 0, 0), newest.CopiedAt);

        Assert.Equal(SaveBackupKind.BeforeRestore, list.Backups[1].Kind);
        Assert.Equal("Santos", list.Backups[1].Club);
        Assert.Equal(SaveBackupKind.Other, list.Backups[3].Kind);
    }

    [Fact]
    public async Task A_restore_keeps_the_current_save_before_putting_the_copy_back()
    {
        const string copy = "Santos-20261116-2200-jogo-2026-03-08.fm";
        WriteBackup(copy, "two days ago");
        NothingLoaded();

        var restore = (await _guard.RestoreAsync(copy, apply: true)).Value;

        Assert.True(restore.Applied);
        Assert.Equal(RealSave, restore.Save);
        Assert.Equal("Santos-antes-de-restaurar-20261117-1430.fm", restore.KeptAs);
        Assert.Equal("career", File.ReadAllText(Path.Combine(_settings.BackupFolder, restore.KeptAs)));
        Assert.Equal("two days ago", File.ReadAllText(RealSavePath()));
        Assert.Equal(copy, restore.Restored.Name);
        Assert.Equal("Santos", restore.Restored.Club);
        Assert.Equal(new DateOnly(2026, 3, 8), restore.Restored.GameDate);
    }

    [Fact]
    public async Task A_restore_is_a_dry_run_unless_applied()
    {
        const string copy = "Santos-20261116-2200-jogo-2026-03-08.fm";
        WriteBackup(copy, "two days ago");
        NothingLoaded();

        var restore = (await _guard.RestoreAsync(copy, apply: false)).Value;

        Assert.False(restore.Applied);
        Assert.Equal("Santos-antes-de-restaurar-20261117-1430.fm", restore.KeptAs);
        Assert.Equal("career", File.ReadAllText(RealSavePath()));
        Assert.Equal([copy], Backups());
    }

    [Fact]
    public async Task A_restore_is_refused_while_the_real_save_is_open_in_the_game()
    {
        const string copy = "Santos-20261116-2200-jogo-2026-03-08.fm";
        WriteBackup(copy, "two days ago");

        Assert.Contains("Close the save", Refusal(await _guard.RestoreAsync(copy, apply: true)));
        Assert.Equal("career", File.ReadAllText(RealSavePath()));
        Assert.Equal([copy], Backups());
    }

    [Fact]
    public async Task A_restore_is_refused_while_a_save_is_loading()
    {
        const string copy = "Santos-20261116-2200-jogo-2026-03-08.fm";
        WriteBackup(copy, "two days ago");
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>()).Returns(new LoadedSave { Loaded = false, Loading = true });

        Assert.Contains("loading", Refusal(await _guard.RestoreAsync(copy, apply: true)));
        Assert.Equal("career", File.ReadAllText(RealSavePath()));
    }

    [Fact]
    public async Task A_restore_goes_ahead_with_the_test_save_open()
    {
        const string copy = "Santos-20261116-2200-jogo-2026-03-08.fm";
        WriteBackup(copy, "two days ago");
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>()).Returns(new LoadedSave
        {
            Loaded = true, Name = "MCPFM investigation", IsTestSave = true, Club = "Santos", GameDate = new DateOnly(2026, 3, 10)
        });

        Assert.True((await _guard.RestoreAsync(copy, apply: true)).Value.Applied);
        Assert.Equal("two days ago", File.ReadAllText(RealSavePath()));
    }

    [Fact]
    public async Task A_restore_that_cant_keep_the_current_save_leaves_it_untouched()
    {
        const string copy = "Santos-20261116-2200-jogo-2026-03-08.fm";
        WriteBackup(copy, "two days ago");
        WriteBackup("Santos-antes-de-restaurar-20261117-1430.fm", "restored a minute ago");
        NothingLoaded();

        Assert.Contains("Nothing was restored", Refusal(await _guard.RestoreAsync(copy, apply: true)));
        Assert.Equal("career", File.ReadAllText(RealSavePath()));
        Assert.Equal("restored a minute ago",
            File.ReadAllText(Path.Combine(_settings.BackupFolder, "Santos-antes-de-restaurar-20261117-1430.fm")));
    }

    [Theory]
    [InlineData("Santos-20261101-0900-jogo-2026-01-20.fm")]
    [InlineData(@"..\games\Vinicius Freitas - Santos.fm")]
    [InlineData("../games/Vinicius Freitas - Santos.fm")]
    [InlineData(SaveGuard.ReleasedVersionFile)]
    public async Task A_restore_only_takes_a_save_copy_in_the_backup_folder(string backup)
    {
        WriteBackup("Santos-20261116-2200-jogo-2026-03-08.fm", "two days ago");
        WriteBackup(SaveGuard.ReleasedVersionFile, Released);
        NothingLoaded();

        Assert.Contains("list_save_backups", Refusal(await _guard.RestoreAsync(backup, apply: true)));
        Assert.Equal("career", File.ReadAllText(RealSavePath()));
    }

    [Fact]
    public async Task A_restore_without_a_real_save_configured_is_refused()
    {
        WriteBackup("Santos-20261116-2200-jogo-2026-03-08.fm", "two days ago");
        NothingLoaded();
        _settings.Save = null;

        Assert.Contains("SaveGuard", Refusal(await _guard.RestoreAsync("Santos-20261116-2200-jogo-2026-03-08.fm", apply: true)));
    }

    [Fact]
    public async Task The_copy_kept_before_a_restore_is_never_rotated_out()
    {
        WriteBackup("Santos-antes-de-restaurar-20261101-0900.fm", "before an old restore");
        WriteBackup("Santos-20261110-0900-jogo-2026-02-01.fm", "old");
        WriteBackup("Santos-20261112-1015-jogo-2026-02-20.fm", "old");

        var clearance = (await _guard.PrepareWriteAsync()).Value;

        Assert.Equal(["Santos-20261110-0900-jogo-2026-02-01.fm"], clearance.RemovedBackups);
        Assert.Contains("Santos-antes-de-restaurar-20261101-0900.fm", Backups());
    }

    [Fact]
    public async Task A_version_release_dry_run_shows_both_versions_and_writes_nothing()
    {
        Version("27.0.2.2510000");

        var release = (await _guard.ReleaseVersionAsync(apply: false)).Value;

        Assert.False(release.Applied);
        Assert.Equal("27.0.2.2510000", release.CurrentVersion);
        Assert.Equal(Released, release.ReleasedVersion);
        Assert.Empty(Backups());
    }

    [Fact]
    public async Task Releasing_the_new_version_clears_the_real_save_for_writes()
    {
        Version("27.0.2.2510000");
        Assert.Contains("27.0.2.2510000", Refusal(await _guard.PrepareWriteAsync()));

        var release = (await _guard.ReleaseVersionAsync(apply: true)).Value;

        Assert.True(release.Applied);
        Assert.Equal("27.0.2.2510000",
            File.ReadAllText(Path.Combine(_settings.BackupFolder, SaveGuard.ReleasedVersionFile)).Trim());
        Assert.True((await _guard.PrepareWriteAsync()).IsSuccess);
    }

    [Fact]
    public async Task A_version_release_without_the_games_version_is_refused()
    {
        Version(null);

        Assert.Contains("version", Refusal(await _guard.ReleaseVersionAsync(apply: true)));
        Assert.Empty(Backups());
    }

    private void NothingLoaded()
    {
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>()).Returns(new LoadedSave { Loaded = false });
    }

    private void WriteBackup(string name, string content)
    {
        Directory.CreateDirectory(_settings.BackupFolder);
        File.WriteAllText(Path.Combine(_settings.BackupFolder, name), content);
    }

    private string RealSavePath()
    {
        return Path.Combine(_settings.GamesFolder, RealSave + ".fm");
    }

    private void Loaded(string name, string club, DateOnly? date)
    {
        _game.GetLoadedSaveAsync(Arg.Any<CancellationToken>())
            .Returns(new LoadedSave { Loaded = true, Name = name, Club = club, GameDate = date });
    }

    private void Version(string? version)
    {
        _game.GetGameVersionAsync(Arg.Any<CancellationToken>()).Returns(version);
    }

    private List<string> Backups()
    {
        return Directory.Exists(_settings.BackupFolder)
            ? Directory.GetFiles(_settings.BackupFolder).Select(Path.GetFileName).ToList()!
            : [];
    }

    private static string Refusal<T>(Result<T> result)
    {
        Assert.True(result.IsFailed);
        var refusal = Assert.IsType<SaveRefusedError>(Assert.Single(result.Errors));
        return string.Join(" ", refusal.ErrorMessages);
    }

    private sealed class FixedTime(DateTime local) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(local, TimeSpan.Zero);
        }
    }
}
