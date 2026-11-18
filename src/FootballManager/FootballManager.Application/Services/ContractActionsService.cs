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

// Contract changes go through the player's own menu: the game builds it for him and offers only what fits his contract
// now, each entry with the confirmation it would show (for a release, its cost). A dry run reads the menu and says what
// would be picked; apply clears the save, picks the entry as a click does and answers the game's Yes/No dialog.
public class ContractActionsService(IScoutingEditorGameClient editor, IContractsGameClient contracts, ISaveGuard saveGuard)
    : IContractActionsService
{
    private readonly record struct MenuLeaf(List<int> Path, List<string> Labels, MenuEntry Entry, bool IsDisabled);

    // The game shows a contract change in the squad's contracts a moment after it's taken.
    private const int StateChecks = 5;
    private static readonly TimeSpan StatePoll = TimeSpan.FromMilliseconds(400);

    public Task<Result<ContractChange>> ReleaseAsync(
        int? playerId, ReleaseKind? how, bool apply, bool confirmRelease, string? expectedConfirmation = null,
        CancellationToken cancellationToken = default)
    {
        if (playerId is not { } id)
        {
            return Task.FromResult(NoPlayer());
        }

        // Required: the enum's 0 is Free, the release the club pays for, so an omitted kind must not read as it.
        if (how is not { } kind || !Enum.IsDefined(kind))
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError(nameof(how), ["Say how: Free, Mutual or EndOfContract."])));
        }

        if (apply && !confirmRelease)
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError("confirmRelease",
                ["Letting a player go can't be undone: apply needs confirmRelease=true, once the user has agreed to the dry run's terms."])));
        }

        // The terms the user agreed to: the dry run's confirmation, sent back as it was shown.
        if (apply && string.IsNullOrWhiteSpace(expectedConfirmation))
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError(nameof(expectedConfirmation),
                ["Applying a release needs expectedConfirmation: the dry run's confirmation (its cost), as the user agreed to it."])));
        }

        var action = kind switch
        {
            ReleaseKind.Mutual => PlayerMenuAction.MutualTermination,
            ReleaseKind.EndOfContract => PlayerMenuAction.SetForRelease,
            ReleaseKind.Free => PlayerMenuAction.ReleaseOnFreeTransfer,
            _ => throw new ArgumentOutOfRangeException(nameof(how), kind, null)
        };

        return ChangeAsync(id, action, null, apply ? expectedConfirmation : null, apply, cancellationToken);
    }

    public Task<Result<ContractChange>> SetSquadStatusAsync(int? playerId, string? status, bool apply, CancellationToken cancellationToken = default)
    {
        if (playerId is not { } id)
        {
            return Task.FromResult(NoPlayer());
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return Task.FromResult(Result.Fail<ContractChange>(new ValidationError(nameof(status),
                ["Name the squad status as the game's menu labels it, e.g. \"Rotação\"."])));
        }

        return ChangeAsync(id, PlayerMenuAction.SetPlayingTime, status, null, apply, cancellationToken);
    }

    public Task<Result<ContractChange>> HandRenewalToDofAsync(int? playerId, bool apply, CancellationToken cancellationToken = default)
    {
        if (playerId is not { } id)
        {
            return Task.FromResult(NoPlayer());
        }

        return ChangeAsync(id, PlayerMenuAction.DofContractRenewal, null, null, apply, cancellationToken);
    }

    // The entry: the one labelled `wanted` among the menu's statuses, or else the menu's first entry the game lets be
    // picked. `approved`: for a release, the confirmation the user agreed to in the dry run.
    private async Task<Result<ContractChange>> ChangeAsync(
        int playerId, PlayerMenuAction action, string? wanted, string? approved, bool apply, CancellationToken cancellationToken)
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

            var leaves = Leaves(menu, [], [], false).ToList();
            var picked = wanted is null
                ? leaves.Where(leaf => !leaf.IsDisabled).Select(leaf => (MenuLeaf?)leaf).FirstOrDefault()
                : Find(leaves, wanted);

            if (picked is not { IsDisabled: false } leaf)
            {
                var labels = string.Join(", ", leaves.Select(each => each.Entry.Label + Unpickable(each)));
                return Fail(wanted is null ? "game" : "status", wanted is null
                    ? $"The game's menu for {player.Name} has no entry it lets be picked now: {labels}."
                    : $"The game's menu offers {player.Name} none of '{wanted}' it lets be picked now. Its entries: {labels}.");
            }

            var entry = leaf.Entry;
            var current = await ContractOfAsync(playerId, cancellationToken);

            // A status he has already would make the read back look like the change took, even if the game never got it.
            if (action == PlayerMenuAction.SetPlayingTime && current?.AgreedPlayingTime is { } now && Plain(now) == Plain(entry.Label))
            {
                return Fail("status", $"{player.Name}'s contract already promises '{now}': there is nothing to change.");
            }

            // Without the status before, the read back of a pick the game didn't answer can't tell the change from a
            // status he already had: it isn't sent.
            if (apply && action == PlayerMenuAction.SetPlayingTime && current?.AgreedPlayingTime is null)
            {
                return Fail("status", $"The game didn't give {player.Name}'s current squad status (get_contracts), so the change " +
                                      "couldn't be told from it: nothing was sent. Read his contracts again and retry.");
            }
            // A release is sent only on the terms the user agreed to: the confirmation the menu shows now must be the one
            // approved. Without one, its dialog couldn't be told from another one, so it isn't sent at all.
            if (apply && IsRelease(action))
            {
                if (string.IsNullOrWhiteSpace(entry.Confirmation))
                {
                    return Fail("expectedConfirmation", $"The game's menu states no confirmation for {Describe(action)} of {player.Name}, " +
                                                        "so its dialog can't be told from another one: nothing was sent. Ask the user to do it in the game.");
                }

                if (!ConfirmationText.SameAs(entry.Confirmation, approved))
                {
                    return Fail("expectedConfirmation", $"The game's confirmation is now \"{entry.Confirmation}\", not the one agreed to " +
                                                        $"(\"{approved}\"): nothing was sent. Show the user the new terms and run the dry run again.");
                }
            }

            var change = new ContractChange
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                Action = action,
                Choice = string.Join(" > ", leaf.Labels),
                Confirmation = entry.Confirmation,
                ConfirmationIsWarning = entry.ConfirmationIsWarning,
                AgreedPlayingTime = current?.AgreedPlayingTime,
                Choices = action == PlayerMenuAction.SetPlayingTime ? leaves.Select(each => each.Entry.Label).ToList() : []
            };

            if (!apply)
            {
                change.Notes.Add(!string.IsNullOrWhiteSpace(entry.Confirmation)
                    ? IsRelease(action)
                        ? "Show the user the game's confirmation (its cost) first. To apply, send it back unchanged as expectedConfirmation: " +
                          "the release is sent only while the menu still shows it, and \"Yes\" is answered only to a dialog that holds it."
                        : "Show the user the game's confirmation first: applying answers \"Yes\" only to a dialog that holds it."
                    : IsRelease(action)
                        ? "The game's menu states no confirmation (no cost) for it, so its dialog can't be told from another one: " +
                          "the tool can't apply it. Ask the user to do it in the game (get_contracts shows his wage and time left)."
                        : "The game's menu states no confirmation for it: applying picks the entry but doesn't answer its dialog " +
                          "(nothing to tell it from another one by); the user answers it in the game.");
                return Result.Ok(change);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<ContractChange>(clearance.Errors);
            }

            change.Notes.Add(SaveNote(clearance.Value));
            await ApplyAsync(change, leaf, approved ?? entry.Confirmation ?? string.Empty, current is not null, cancellationToken);
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

    // Picks the entry, answers its dialog, and reads the change back. From the pick on, nothing fails the call: each step
    // says what it did, so that the caller doesn't send again what the game may have taken. A refusal before the pick
    // (the menu changed, a dialog already open) throws: nothing was sent.
    private async Task ApplyAsync(ContractChange change, MenuLeaf leaf, string confirmation, bool wasInSquad, CancellationToken cancellationToken)
    {
        try
        {
            // A release is sent only while the entry still states the terms the user approved.
            await editor.ClickMenuEntryAsync(change.PlayerId, change.Action, leaf.Path, leaf.Labels,
                IsRelease(change.Action) ? confirmation : null, cancellationToken);
            change.Menu = MenuWrite.Written;
        }
        catch (GameAnswerLostException exception)
        {
            change.Menu = MenuWrite.Unknown;
            change.Notes.Add($"The game didn't answer the pick ({exception.Message}): the read back decides whether it took.");
        }

        if (change.Menu == MenuWrite.Written)
        {
            // Only the dialog of this action is answered: the one that holds the confirmation (for a release, the one
            // agreed to in the dry run). Without one, the dialog is only read, never answered.
            var dialog = await editor.AnswerDialogAsync(confirmation, cancellationToken);
            change.Dialog = dialog.State;
            change.DialogText = dialog.Text;
        }

        await ReadBackAsync(change, leaf.Entry.Label, wasInSquad, cancellationToken);

        if (change.Menu == MenuWrite.Unknown && change.Confirmed == true)
        {
            change.Menu = MenuWrite.Written;
        }

        change.Applied = change.Confirmed == true || (change.Menu == MenuWrite.Written && change.Dialog switch
        {
            DialogState.Answered => true,
            DialogState.NotShown => string.IsNullOrWhiteSpace(confirmation),
            _ => false
        })
            ? AppliedState.Yes
            : AppliedState.Partial;

        if (change.Applied == AppliedState.Partial)
        {
            change.Notes.Add(change.Dialog switch
            {
                DialogState.NotAnswered when change.DialogText is not null && string.IsNullOrWhiteSpace(confirmation) =>
                    "The game's menu states no confirmation for it, so its dialog wasn't answered: " +
                    $"\"{change.DialogText}\". Ask the user to answer it in the game.",
                DialogState.NotAnswered when change.DialogText is not null =>
                    "The game showed a dialog that doesn't hold the confirmation agreed to, so it wasn't answered: " +
                    $"\"{change.DialogText}\". Ask the user to resolve it in the game.",
                DialogState.NotAnswered =>
                    "The game's dialog couldn't be read, so it wasn't answered. Ask the user to check the game for an open dialog.",
                DialogState.Unknown =>
                    "\"Yes\" was sent to the game's dialog but it didn't close, or the game didn't answer. Ask the user to check the game.",
                DialogState.NotShown =>
                    "The game showed no confirmation dialog though the menu stated one. Ask the user to check the game.",
                _ => "The game didn't answer the pick and the read back doesn't show the change. Check the game before sending it again."
            });
        }
    }

    // What the squad's contracts show once the game has taken the change, compared with what was asked: the status
    // picked, or him gone from the first team. Only a change from the state before counts: a status he already had is
    // refused in the plan, and a release of one the first team didn't list can't be told. A read back that fails leaves
    // it unconfirmed, never an error.
    private async Task ReadBackAsync(ContractChange change, string picked, bool wasInSquad, CancellationToken cancellationToken)
    {
        if (change.Action is not (PlayerMenuAction.SetPlayingTime or PlayerMenuAction.ReleaseOnFreeTransfer))
        {
            change.Notes.Add(change.Action == PlayerMenuAction.MutualTermination
                ? "He answers the offer later: confirmed stays null. Check get_contracts or his inbox after continuing."
                : "The game doesn't show this change in his contract: confirmed stays null. Check his menu before sending it again.");
            return;
        }

        if (change.Action == PlayerMenuAction.ReleaseOnFreeTransfer && !wasInSquad)
        {
            change.Notes.Add("The first team's contracts didn't list him before the release, so the read back can't show it: " +
                             "confirmed stays null. Check his club in the game before sending it again.");
            return;
        }

        try
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
                change.Confirmed = change.Action == PlayerMenuAction.SetPlayingTime
                    ? change.AgreedPlayingTimeAfter is { } after ? Plain(after) == Plain(picked) : null
                    : !change.StillInSquad;

                if (change.Confirmed == true)
                {
                    return;
                }
            }
        }
        catch (GameUnavailableException exception)
        {
            change.Confirmed = null;
            change.Notes.Add($"The change was sent but reading it back failed ({exception.Message}): check get_contracts before sending it again.");
            return;
        }

        change.Notes.Add(change.Action == PlayerMenuAction.SetPlayingTime
            ? change.Confirmed == false
                ? $"His contract shows '{change.AgreedPlayingTimeAfter}', not '{picked}': check get_contracts before sending it again."
                : "The game didn't read back his status: check get_contracts before sending it again."
            : "The first team still lists him: the game may let him go at its next day's processing. Check get_squad after continuing.");
    }

    private async Task<SquadPlayerContract?> ContractOfAsync(int playerId, CancellationToken cancellationToken)
    {
        return (await contracts.GetContractsAsync(cancellationToken)).FirstOrDefault(contract => contract.PlayerId == playerId);
    }

    private static string Unpickable(MenuLeaf leaf)
    {
        return !leaf.IsDisabled ? string.Empty : leaf.Entry.CanRun || leaf.Entry.IsDisabled ? " (greyed out)" : " (can't be picked)";
    }

    // Every entry that does something rather than open a sub-menu, with its path (an index per level, from the top), its
    // label at each level, and whether it or a level above it is greyed out.
    private static IEnumerable<MenuLeaf> Leaves(List<MenuEntry> entries, List<int> above, List<string> labelsAbove, bool disabledAbove)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            List<int> path = [.. above, i];
            List<string> labels = [.. labelsAbove, entries[i].Label];
            var disabled = disabledAbove || entries[i].IsDisabled;

            if (entries[i].Children.Count == 0)
            {
                // An entry that sends no event can't be picked either: the game lets the agreed playing time only go up
                // from the menu, and the statuses below his own come with Event 0 (test save, 18/11/2026).
                yield return new MenuLeaf(path, labels, entries[i], disabled || !entries[i].CanRun);
                continue;
            }

            foreach (var leaf in Leaves(entries[i].Children, path, labels, disabled))
            {
                yield return leaf;
            }
        }
    }

    // The entry labelled so, ignoring case and accents; else the only one whose label holds it.
    private static MenuLeaf? Find(List<MenuLeaf> leaves, string wanted)
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

    // Required: an omitted id would read as 0, a player that doesn't exist.
    private static Result<ContractChange> NoPlayer()
    {
        return Fail("playerId", "Say which player: his FM id, from get_squad or get_contracts.");
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
