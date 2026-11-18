using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Services;

// A player leaves the club through his own menu in the game, as the user's right click does: the game offers only
// what fits where he stands (the transfer status entries for any player, taking him off a list he's on, calling him
// back or extending a loan when he's out on one) and each entry sends its own event. Where he stands is read from the
// same menu: "remove from transfer list" is there only while he's on it. A dry run reads the menu and sends nothing.
public class PlayerExitService(IScoutingEditorGameClient editor, ISaveGuard saveGuard) : IPlayerExitService
{
    // The menu shows a change a frame or more after it's sent.
    private const int StateChecks = 5;
    private static readonly TimeSpan StatePoll = TimeSpan.FromMilliseconds(300);

    public async Task<Result<PlayerExit>> ChangeAsync(
        int playerId, PlayerExitAction? requested, string? choice, bool apply, CancellationToken cancellationToken = default)
    {
        // No default action: a request that leaves it out must not fall on the enum's first value (SetTransferStatus).
        if (requested is not { } action)
        {
            return Refuse("action", "Name the action to run, e.g. SetTransferStatus or RemoveFromTransferList. Nothing was sent.");
        }

        try
        {
            var found = await editor.FindPlayersAsync([playerId], cancellationToken);

            if (!found.TryGetValue(playerId, out var player))
            {
                return Result.Fail<PlayerExit>(new EntityDoesNotExistError());
            }

            var menuAction = MenuActionOf(action);
            var offered = await editor.GetOfferedActionsAsync(playerId, cancellationToken);
            var exit = new PlayerExit { PlayerId = player.Id, PlayerName = player.Name, Action = action, Status = StatusOf(offered) };

            var menu = offered.Contains(menuAction) ? await editor.GetMenuAsync(playerId, menuAction, cancellationToken) : null;

            if (menu is not { Count: > 0 })
            {
                return Refuse("action", $"The game's menu doesn't offer {Describe(action)} for {player.Name} now ({Describe(exit.Status)}). Nothing was sent.");
            }

            var entries = Runnable(menu, []).ToList();

            if (entries.Count == 0 && menu.All(entry => entry.IsDisabled))
            {
                return Refuse("action",
                    $"The game greys out {Describe(action)} for {player.Name} now ({string.Join(", ", menu.Select(entry => entry.Label))}): " +
                    "it can't be done yet. Nothing was sent.");
            }

            if (entries.Count == 0)
            {
                return Refuse("action",
                    $"The game's menu for {Describe(action)} has no entry this tool can run for {player.Name} " +
                    $"({string.Join(", ", menu.Select(entry => entry.Label))}): it opens a screen of its own. Nothing was sent.");
            }

            exit.Choices = entries.Select(entry => PlayerMenus.Label(menu, entry)).ToList();

            var picked = Pick(entries, exit.Choices, choice);

            if (picked.IsFailed)
            {
                return Result.Fail<PlayerExit>(picked.Errors);
            }

            if (picked.Value is not { } path)
            {
                exit.Notes.Add($"The game's menu offers {entries.Count} entries: apply with one of them as the choice.");
                return apply
                    ? Refuse("choice", $"Name the choice: one of {string.Join("; ", exit.Choices)}.")
                    : Result.Ok(exit);
            }

            exit.Choice = PlayerMenus.Label(menu, path);
            var confirmation = PlayerMenus.ConfirmationOf(menu, path);
            exit.Confirmation = confirmation.Text;

            if (!apply)
            {
                return Result.Ok(exit);
            }

            if (confirmation.IsWarning)
            {
                return Refuse("action", $"The game warns against it: {confirmation.Text} Nothing was sent.");
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<PlayerExit>(clearance.Errors);
            }

            exit.Notes.Add(SaveNote(clearance.Value));

            var before = exit.Status;
            await editor.SendMenuEntryAsync(playerId, menuAction, path, PlayerMenus.Labels(menu, path), cancellationToken);

            exit.Applied = true;

            // An offer to clubs or a loan extension goes to other clubs, who answer in the inbox later: where he
            // stands doesn't change, so it's read once.
            if (action is PlayerExitAction.OfferToClubs or PlayerExitAction.ExtendLoan)
            {
                exit.Status = StatusOf(await editor.GetOfferedActionsAsync(playerId, cancellationToken));
                exit.Notes.Add("The clubs answer in the inbox, usually within a few days of the game's calendar: get_inbox.");
                return Result.Ok(exit);
            }

            exit.Status = await StatusAfterAsync(playerId, before, cancellationToken);

            if (SameAs(exit.Status, before))
            {
                exit.Notes.Add($"The game's menu still shows {player.Name} as before ({Describe(exit.Status)}) after the entry was sent.");
            }

            return Result.Ok(exit);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<PlayerExit>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Refuse("game", exception.Message);
        }
    }

    // The entry the user named, or the only one when there's one; null when there are several and none was named.
    private static Result<List<int>?> Pick(List<List<int>> entries, List<string> labels, string? choice)
    {
        if (string.IsNullOrWhiteSpace(choice))
        {
            return Result.Ok(entries.Count == 1 ? entries[0] : null);
        }

        var index = labels.FindIndex(label => string.Equals(label.Trim(), choice.Trim(), StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            return Result.Fail<List<int>?>(new ValidationError("choice",
                [$"The game's menu has no entry '{choice}'. It offers: {string.Join("; ", labels)}."]));
        }

        return Result.Ok<List<int>?>(entries[index]);
    }

    // The paths to every entry that does something when picked, from the top of the menu, under no submenu the game
    // greys out (a greyed-out submenu can't be opened in the game).
    private static IEnumerable<List<int>> Runnable(List<MenuEntry> entries, List<int> above)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            List<int> path = [.. above, i];

            if (entries[i].IsDisabled)
            {
                continue;
            }

            if (entries[i].Children.Count > 0)
            {
                foreach (var below in Runnable(entries[i].Children, path))
                {
                    yield return below;
                }
            }
            else if (entries[i].CanRun)
            {
                yield return path;
            }
        }
    }

    // Where he stands once the menu shows the change, or as it last showed him.
    private async Task<PlayerExitStatus> StatusAfterAsync(int playerId, PlayerExitStatus before, CancellationToken cancellationToken)
    {
        var status = before;

        for (var check = 0; check < StateChecks; check++)
        {
            await Task.Delay(StatePoll, cancellationToken);
            status = StatusOf(await editor.GetOfferedActionsAsync(playerId, cancellationToken));

            if (!SameAs(status, before))
            {
                return status;
            }
        }

        return status;
    }

    private static PlayerExitStatus StatusOf(HashSet<PlayerMenuAction> offered)
    {
        return new PlayerExitStatus
        {
            TransferListed = offered.Contains(PlayerMenuAction.RemoveFromTransferList),
            LoanListed = offered.Contains(PlayerMenuAction.RemoveFromLoanList),
            OutOnLoan = offered.Contains(PlayerMenuAction.RecallFromLoan)
        };
    }

    private static bool SameAs(PlayerExitStatus a, PlayerExitStatus b)
    {
        return a.TransferListed == b.TransferListed && a.LoanListed == b.LoanListed && a.OutOnLoan == b.OutOnLoan;
    }

    private static PlayerMenuAction MenuActionOf(PlayerExitAction action)
    {
        return action switch
        {
            PlayerExitAction.SetTransferStatus => PlayerMenuAction.SetTransferStatus,
            PlayerExitAction.RemoveFromTransferList => PlayerMenuAction.RemoveFromTransferList,
            PlayerExitAction.RemoveFromLoanList => PlayerMenuAction.RemoveFromLoanList,
            PlayerExitAction.OfferToClubs => PlayerMenuAction.OfferToClubs,
            PlayerExitAction.Recall => PlayerMenuAction.RecallFromLoan,
            PlayerExitAction.ExtendLoan => PlayerMenuAction.ExtendLoan,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    private static string Describe(PlayerExitAction action)
    {
        return action switch
        {
            PlayerExitAction.SetTransferStatus => "a transfer status",
            PlayerExitAction.RemoveFromTransferList => "taking him off the transfer list",
            PlayerExitAction.RemoveFromLoanList => "taking him off the loan list",
            PlayerExitAction.OfferToClubs => "offering him to clubs",
            PlayerExitAction.Recall => "a recall from loan",
            PlayerExitAction.ExtendLoan => "a loan extension",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    private static string Describe(PlayerExitStatus status)
    {
        var parts = new List<string>();

        if (status.TransferListed)
        {
            parts.Add("on the transfer list");
        }

        if (status.LoanListed)
        {
            parts.Add("on the loan list");
        }

        if (status.OutOnLoan)
        {
            parts.Add("out on loan");
        }

        return parts.Count == 0 ? "on no list, not out on loan" : string.Join(", ", parts);
    }

    private static Result<PlayerExit> Refuse(string field, string message)
    {
        return Result.Fail<PlayerExit>(new ValidationError(field, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
