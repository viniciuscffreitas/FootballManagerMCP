using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Services;

// The pursuit of a player of another club goes through his own menu in the game, as the user's right click does. The
// menu offers only what fits where he stands: "withdraw offer" while an offer the club made for him waits for an
// answer (the same entry withdraws a trial offer), "add as transfer target" until the director of football has him
// among his targets, and the removal once he has. Each entry sends its own event (EventFmxDoAction), and the menu read
// back afterwards says whether the game took it. A dry run reads the menu and sends nothing.
public class TransferTargetService(IScoutingEditorGameClient editor, ISaveGuard saveGuard) : ITransferTargetService
{
    // The menu shows a change a frame or more after it's sent.
    private const int StateChecks = 5;
    private static readonly TimeSpan StatePoll = TimeSpan.FromMilliseconds(300);

    public async Task<Result<TransferTargetChange>> ChangeAsync(
        int? playerId, TransferTargetAction? action, bool apply, CancellationToken cancellationToken = default)
    {
        if (playerId is not { } id)
        {
            return Refuse("playerId", "Name the player: his FM id. Nothing was sent.");
        }

        if (action is not { } wanted || !Enum.IsDefined(wanted))
        {
            return Refuse("action", $"Name the action: one of {string.Join(", ", Enum.GetNames<TransferTargetAction>())}. Nothing was sent.");
        }

        try
        {
            if (!(await editor.FindPlayersAsync([id], cancellationToken)).TryGetValue(id, out var player))
            {
                return Result.Fail<TransferTargetChange>(new EntityDoesNotExistError());
            }

            var menuAction = MenuActionOf(wanted);
            var offered = await editor.GetOfferedActionsAsync(id, cancellationToken);
            var change = new TransferTargetChange { PlayerId = player.Id, PlayerName = player.Name, Club = player.Club, Action = wanted };
            Show(change, offered);

            if (!offered.Contains(menuAction))
            {
                return Refuse("action", $"The game's menu doesn't offer {Describe(wanted)} for {player.Name} now ({Describe(change)}). Nothing was sent.");
            }

            var planned = await PlanAsync(id, menuAction, cancellationToken);

            if (planned.IsFailed)
            {
                return Result.Fail<TransferTargetChange>(planned.Errors);
            }

            var (menu, path) = planned.Value;
            change.Choice = PlayerMenus.Label(menu, path);
            var confirmation = PlayerMenus.ConfirmationOf(menu, path);
            change.Confirmation = confirmation.Text;

            if (wanted == TransferTargetAction.WithdrawOffer)
            {
                change.Notes.Add("The game's entry withdraws the offer waiting for an answer, a trial offer among them: check it's the transfer offer.");
            }

            if (!apply)
            {
                return Result.Ok(change);
            }

            if (confirmation.IsWarning)
            {
                return Refuse("action", $"The game warns against it: {confirmation.Text} Nothing was sent.");
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<TransferTargetChange>(clearance.Errors);
            }

            change.Notes.Add(SaveNote(clearance.Value));

            // The menu again, just before sending: the entry run is the one the plan showed, or nothing is.
            var again = await PlanAsync(id, menuAction, cancellationToken);

            if (again.IsFailed
                || PlayerMenus.Label(again.Value.Menu, again.Value.Path) != change.Choice
                || PlayerMenus.ConfirmationOf(again.Value.Menu, again.Value.Path) != confirmation)
            {
                return Refuse("action", $"The game's menu for {player.Name} changed since it was read ({change.Choice}): nothing was sent.");
            }

            return Result.Ok(await SendAsync(change, menuAction, again.Value.Path, cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TransferTargetChange>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Refuse("game", exception.Message);
        }
    }

    // The menu's only entry that does something. A menu with none (greyed out, or opening a screen of its own) or with
    // several (a choice this tool doesn't make) is refused.
    private async Task<Result<(List<MenuEntry> Menu, List<int> Path)>> PlanAsync(
        int playerId, PlayerMenuAction action, CancellationToken cancellationToken)
    {
        var menu = await editor.GetMenuAsync(playerId, action, cancellationToken);

        if (menu is not { Count: > 0 })
        {
            return Result.Fail(new ValidationError("action", ["The game's menu no longer offers it. Nothing was sent."]));
        }

        var entries = Runnable(menu, []).ToList();

        if (entries.Count != 1)
        {
            var labels = string.Join(", ", menu.Select(entry => entry.Label));
            return Result.Fail(new ValidationError("action", [entries.Count == 0
                ? $"The game greys the entry out, or it opens a screen of its own ({labels}). Nothing was sent."
                : $"The game's menu has {entries.Count} entries for it ({labels}), a choice this tool doesn't make. Nothing was sent."]));
        }

        return Result.Ok((menu, entries[0]));
    }

    // Sends the entry, then reads the menu back. The client reads the player again after the write, so a refusal or no
    // answer may come after the game wrote: the read back decides; one that fails after a write still reports it.
    private async Task<TransferTargetChange> SendAsync(
        TransferTargetChange change, PlayerMenuAction menuAction, List<int> path, CancellationToken cancellationToken)
    {
        try
        {
            await editor.RunMenuEntryAsync(change.PlayerId, menuAction, path, cancellationToken);
            change.Write = WriteState.Written;
        }
        catch (Exception exception) when (exception is GameUnavailableException or GameRejectedException)
        {
            change.Write = WriteState.Unknown;
            change.Notes.Add($"The game didn't take the write cleanly ({exception.Message}): its menu read back decides whether it took.");
        }

        change.Applied = true;

        HashSet<PlayerMenuAction>? offered = null;

        try
        {
            for (var check = 0; check < StateChecks; check++)
            {
                await Task.Delay(StatePoll, cancellationToken);
                offered = await editor.GetOfferedActionsAsync(change.PlayerId, cancellationToken);

                // A player of another club always has some menu (scouting, shortlist): none is a read that failed.
                if (offered.Count == 0)
                {
                    throw new GameUnavailableException("the game listed no menu actions for him");
                }

                if (Reached(change.Action, offered))
                {
                    break;
                }
            }
        }
        catch (Exception exception) when (exception is GameUnavailableException or GameRejectedException)
        {
            change.Notes.Add($"The menu couldn't be read back after the write ({exception.Message}): don't send it again; a dry run shows where he stands.");
            return change;
        }

        Show(change, offered!);
        change.Confirmed = Reached(change.Action, offered!);

        if (change.Confirmed == true)
        {
            change.Write = WriteState.Written;
        }
        else if (change.Write == WriteState.Written)
        {
            change.Notes.Add($"The game's menu still shows {change.PlayerName} as before ({Describe(change)}) after the entry was sent.");
        }
        else
        {
            change.Write = WriteState.NotWritten;
            change.Notes.Add($"The game's menu still shows {change.PlayerName} as before ({Describe(change)}): the write didn't take.");
        }

        return change;
    }

    // Where the menu shows him once the action took: no offer to withdraw, the target's removal offered, or no removal.
    private static bool Reached(TransferTargetAction action, HashSet<PlayerMenuAction> offered)
    {
        return action switch
        {
            TransferTargetAction.WithdrawOffer => !offered.Contains(PlayerMenuAction.WithdrawOffer),
            TransferTargetAction.MarkDofTarget => offered.Contains(PlayerMenuAction.UnmarkDofTarget),
            TransferTargetAction.UnmarkDofTarget => !offered.Contains(PlayerMenuAction.UnmarkDofTarget),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    private static void Show(TransferTargetChange change, HashSet<PlayerMenuAction> offered)
    {
        change.OfferPending = offered.Contains(PlayerMenuAction.WithdrawOffer);
        change.IsDofTarget = offered.Contains(PlayerMenuAction.UnmarkDofTarget);
    }

    // The paths to every entry that does something when picked, from the top of the menu.
    private static IEnumerable<List<int>> Runnable(List<MenuEntry> entries, List<int> above)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            List<int> path = [.. above, i];

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

    private static PlayerMenuAction MenuActionOf(TransferTargetAction action)
    {
        return action switch
        {
            TransferTargetAction.WithdrawOffer => PlayerMenuAction.WithdrawOffer,
            TransferTargetAction.MarkDofTarget => PlayerMenuAction.MarkDofTarget,
            TransferTargetAction.UnmarkDofTarget => PlayerMenuAction.UnmarkDofTarget,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    private static string Describe(TransferTargetAction action)
    {
        return action switch
        {
            TransferTargetAction.WithdrawOffer => "withdrawing an offer",
            TransferTargetAction.MarkDofTarget => "marking him as a transfer target",
            TransferTargetAction.UnmarkDofTarget => "taking him off the transfer targets",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    private static string Describe(TransferTargetChange change)
    {
        return (change.OfferPending ? "an offer waits for an answer" : "no offer waiting")
               + (change.IsDofTarget ? ", a transfer target" : ", not a transfer target");
    }

    private static Result<TransferTargetChange> Refuse(string field, string message)
    {
        return Result.Fail<TransferTargetChange>(new ValidationError(field, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
