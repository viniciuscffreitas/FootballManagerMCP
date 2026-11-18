using System.Globalization;
using System.Text;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Contracts;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

public interface IContractActionsService
{
    /// <summary>
    /// Lets one of the club's own players go, through his menu in the game: released on a free transfer (the club pays
    /// off his contract), his contract ended by mutual consent, or marked to leave when it ends. Only when
    /// <paramref name="apply"/> and <paramref name="confirmRelease"/> are both set: a release can't be undone.
    /// </summary>
    Task<Result<ContractChange>> ReleaseAsync(
        int playerId, ReleaseKind how, bool apply, bool confirmRelease, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the squad status (the playing time his contract promises) of one of the club's own players to the
    /// status of his menu labelled <paramref name="status"/>. Only when <paramref name="apply"/> is set.
    /// </summary>
    Task<Result<ContractChange>> SetSquadStatusAsync(int playerId, string status, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hands the renewal of one of the club's own players' contracts to the director of football, through his menu.
    /// Only when <paramref name="apply"/> is set.
    /// </summary>
    Task<Result<ContractChange>> HandRenewalToDofAsync(int playerId, bool apply, CancellationToken cancellationToken = default);
}

// Contract changes go through the player's own menu: the game builds it for him and offers only what fits his contract
// now, each entry with the confirmation it would show (for a release, its cost). A dry run reads the menu and says what
// would be picked; apply clears the save, picks the entry as a click does and answers the game's Yes/No dialog.
public class ContractActionsService(IScoutingEditorGameClient editor, IContractsGameClient contracts, ISaveGuard saveGuard)
    : IContractActionsService
{
    // The game shows a contract change in the squad's contracts a moment after it's taken.
    private const int StateChecks = 5;
    private static readonly TimeSpan StatePoll = TimeSpan.FromMilliseconds(400);

    public Task<Result<ContractChange>> ReleaseAsync(
        int playerId, ReleaseKind how, bool apply, bool confirmRelease, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(how))
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError(nameof(how), ["Say how: Free, Mutual or EndOfContract."])));
        }

        if (apply && !confirmRelease)
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError("confirmRelease",
                ["Letting a player go can't be undone: apply needs confirmRelease=true, once the user has agreed to the dry run's terms."])));
        }

        var action = how switch
        {
            ReleaseKind.Mutual => PlayerMenuAction.MutualTermination,
            ReleaseKind.EndOfContract => PlayerMenuAction.SetForRelease,
            ReleaseKind.Free => PlayerMenuAction.ReleaseOnFreeTransfer,
            _ => throw new ArgumentOutOfRangeException(nameof(how), how, null)
        };

        return ChangeAsync(playerId, action, null, apply, cancellationToken);
    }

    public Task<Result<ContractChange>> SetSquadStatusAsync(int playerId, string status, bool apply, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError(nameof(status),
                ["Name the squad status as the game's menu labels it, e.g. \"Rotação\"."])));
        }

        return ChangeAsync(playerId, PlayerMenuAction.SetPlayingTime, status, apply, cancellationToken);
    }

    public Task<Result<ContractChange>> HandRenewalToDofAsync(int playerId, bool apply, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(playerId, PlayerMenuAction.DofContractRenewal, null, apply, cancellationToken);
    }

    // The entry: the one labelled `wanted` among the menu's statuses, or else the menu's first entry the game lets be
    // picked.
    private async Task<Result<ContractChange>> ChangeAsync(
        int playerId, PlayerMenuAction action, string? wanted, bool apply, CancellationToken cancellationToken)
    {
        try
        {
            if ((await editor.FindPlayersAsync([playerId], cancellationToken)).GetValueOrDefault(playerId) is not { } player)
            {
                return Result.Fail<ContractChange>(new EntityDoesNotExistError());
            }

            if (!(await editor.GetOfferedActionsAsync(playerId, cancellationToken)).Contains(action)
                || await editor.GetMenuAsync(playerId, action, cancellationToken) is not { Count: > 0 } menu)
            {
                return Fail("game", $"The game doesn't offer {Describe(action)} for {player.Name} now (his menu lacks it: " +
                                    "he may not be one of the club's own players, or his contract doesn't allow it).");
            }

            var leaves = Leaves(menu, []).ToList();
            var picked = wanted is null
                ? leaves.Where(leaf => !leaf.Entry.IsDisabled).Select(leaf => ((List<int> Path, MenuEntry Entry)?)leaf).FirstOrDefault()
                : Find(leaves, wanted);

            if (picked is null || picked.Value.Entry.IsDisabled)
            {
                var labels = string.Join(", ", leaves.Select(leaf => leaf.Entry.Label + (leaf.Entry.IsDisabled ? " (greyed out)" : "")));
                return Fail(wanted is null ? "game" : "status", wanted is null
                    ? $"The game's menu for {player.Name} has no entry it lets be picked now: {labels}."
                    : $"The game's menu offers {player.Name} none of '{wanted}' it lets be picked now. Its entries: {labels}.");
            }

            var (path, entry) = picked.Value;
            var current = await ContractOfAsync(playerId, cancellationToken);
            var change = new ContractChange
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                Action = action,
                Choice = PlayerMenus.Label(menu, path),
                Confirmation = entry.Confirmation,
                ConfirmationIsWarning = entry.ConfirmationIsWarning,
                AgreedPlayingTime = current?.AgreedPlayingTime,
                Choices = action == PlayerMenuAction.SetPlayingTime ? leaves.Select(leaf => leaf.Entry.Label).ToList() : []
            };

            if (!apply)
            {
                change.Notes.Add(entry.Confirmation is not null
                    ? "Show the user the game's confirmation first: applying answers \"Yes\" to it."
                    : IsRelease(action)
                        ? "The game's menu doesn't state what letting him go costs: tell the user the cost isn't known before applying " +
                          "(get_contracts shows his wage and time left). Applying answers \"Yes\" to the game's dialog and returns its text."
                        : "The game's menu states no confirmation for it: applying answers \"Yes\" to whatever dialog the game shows, and returns its text.");
                return Result.Ok(change);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<ContractChange>(clearance.Errors);
            }

            change.Notes.Add(SaveNote(clearance.Value));
            change.DialogAnswered = await editor.ClickMenuEntryAsync(playerId, action, path, cancellationToken);
            change.Applied = true;

            await ReadBackAsync(change, cancellationToken);
            return Result.Ok(change);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ContractChange>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Fail("game", exception.Message);
        }
    }

    // What the squad's contracts show once the game has taken the change: the new status, or whether he's still listed.
    private async Task ReadBackAsync(ContractChange change, CancellationToken cancellationToken)
    {
        for (var check = 0; check < StateChecks; check++)
        {
            if (check > 0)
            {
                await Task.Delay(StatePoll, cancellationToken);
            }

            var contract = await ContractOfAsync(change.PlayerId, cancellationToken);
            change.StillInSquad = contract is not null;
            change.AgreedPlayingTimeAfter = contract?.AgreedPlayingTime;

            if (Reached(change))
            {
                return;
            }
        }

        change.Notes.Add(change.Action switch
        {
            PlayerMenuAction.SetPlayingTime => "The game doesn't show the new status in his contract yet: check get_contracts before sending it again.",
            PlayerMenuAction.ReleaseOnFreeTransfer => "The first team still lists him: the game may let him go at its next day's processing. Check get_squad after continuing.",
            _ => "The change was sent: check his menu or get_contracts before sending it again."
        });
    }

    private static bool Reached(ContractChange change)
    {
        return change.Action switch
        {
            PlayerMenuAction.SetPlayingTime => change.AgreedPlayingTimeAfter != change.AgreedPlayingTime,
            PlayerMenuAction.ReleaseOnFreeTransfer => change.StillInSquad == false,

            // A mutual termination waits for his answer; the other changes don't show in the contract's fields.
            _ => true
        };
    }

    private async Task<SquadPlayerContract?> ContractOfAsync(int playerId, CancellationToken cancellationToken)
    {
        return (await contracts.GetContractsAsync(cancellationToken)).FirstOrDefault(contract => contract.PlayerId == playerId);
    }

    // Every entry that does something rather than open a sub-menu, with its path (an index per level, from the top).
    private static IEnumerable<(List<int> Path, MenuEntry Entry)> Leaves(List<MenuEntry> entries, List<int> above)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            List<int> path = [.. above, i];

            if (entries[i].Children.Count == 0)
            {
                yield return (path, entries[i]);
                continue;
            }

            foreach (var leaf in Leaves(entries[i].Children, path))
            {
                yield return leaf;
            }
        }
    }

    // The entry labelled so, ignoring case and accents; else the only one whose label holds it.
    private static (List<int> Path, MenuEntry Entry)? Find(List<(List<int> Path, MenuEntry Entry)> leaves, string wanted)
    {
        var key = Plain(wanted);
        var exact = leaves.Where(leaf => Plain(leaf.Entry.Label) == key).ToList();

        if (exact.Count > 0)
        {
            return exact[0];
        }

        var partial = leaves.Where(leaf => Plain(leaf.Entry.Label).Contains(key, StringComparison.Ordinal)).ToList();
        return partial.Count == 1 ? partial[0] : null;
    }

    private static string Plain(string text)
    {
        var letters = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(letters.ToArray());
    }

    private static bool IsRelease(PlayerMenuAction action)
    {
        return action is PlayerMenuAction.ReleaseOnFreeTransfer or PlayerMenuAction.MutualTermination or PlayerMenuAction.SetForRelease;
    }

    private static string Describe(PlayerMenuAction action)
    {
        return action switch
        {
            PlayerMenuAction.ReleaseOnFreeTransfer => "a release on a free transfer",
            PlayerMenuAction.MutualTermination => "ending the contract by mutual consent",
            PlayerMenuAction.SetForRelease => "marking him to leave when his contract ends",
            PlayerMenuAction.SetPlayingTime => "a change of squad status",
            PlayerMenuAction.DofContractRenewal => "handing his renewal to the director of football",
            _ => action.ToString()
        };
    }

    private static Result<ContractChange> Fail(string field, string message)
    {
        return Result.Fail<ContractChange>(new ValidationError(field, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
