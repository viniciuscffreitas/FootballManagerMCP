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
        int? requestedPlayer, PlayerExitAction? requested, string? choice, bool apply, CancellationToken cancellationToken = default)
    {
        // No default action: a request that leaves it out must not fall on the enum's first value (SetTransferStatus).
        if (requested is not { } action)
        {
            return Refuse("action", "Name the action to run, e.g. SetTransferStatus or RemoveFromTransferList. Nothing was sent.");
        }

        // A number outside the list would reach no menu action: refused before anything is read.
        if (!Enum.IsDefined(action))
        {
            return Refuse("action", $"There's no action {(int)action}: use SetTransferStatus, RemoveFromTransferList, RemoveFromLoanList, " +
                                    "OfferToClubs, Recall or ExtendLoan. Nothing was sent.");
        }

        if (requestedPlayer is not { } playerId)
        {
            return Refuse("playerId", "Name the player: playerId, his FM id. Nothing was sent.");
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
            var labels = PlayerMenus.Labels(menu, path);
            var expected = Expected(action, labels);

            // Sending what he already has would prove nothing: the read back would show it whether the game took it or not.
            if (expected is { } target && target.Shows(exit.Status))
            {
                return Refuse("choice", $"{player.Name} is already {target.What}: there's nothing to change. Nothing was sent.");
            }

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

            var written = PlayerExitWrite.Written;

            try
            {
                await editor.SendMenuEntryAsync(playerId, menuAction, path, labels, cancellationToken);
            }
            catch (GameAnswerLostException exception)
            {
                // The entry went out: a lost answer doesn't say the game didn't take it, the read back decides.
                written = PlayerExitWrite.Unknown;
                exit.Notes.Add($"Warning: {exception.Message}; the read back decides whether it was written.");
            }

            // The entry is sent by now: a read back that fails must not report it as failed, or it gets sent again.
            try
            {
                exit.Status = await StatusAfterAsync(playerId, expected?.Shows, cancellationToken);
            }
            catch (Exception exception) when (exception is GameUnavailableException or GameRejectedException)
            {
                exit.Applied = written;
                exit.Notes.Add(written == PlayerExitWrite.Unknown
                    ? $"Warning: the game may or may not have taken it, and his menu couldn't be read back ({exception.Message}): " +
                      "check with get_squad before sending it again."
                    : $"Sent, but his menu couldn't be read back ({exception.Message}): check with get_squad before sending it again.");
                return Result.Ok(exit);
            }

            if (expected is not { } wanted)
            {
                exit.Applied = written;
                exit.Notes.Add(action is PlayerExitAction.OfferToClubs or PlayerExitAction.ExtendLoan
                    ? $"His menu doesn't show whether {Describe(action)} went to the clubs: they answer in the inbox, " +
                      "usually within a few days of the game's calendar (get_inbox)."
                    : CantTellApart(action, labels)
                        ? $"Warning: his menu can't tell '{exit.Choice}' from the other loan statuses off the loan list, so it can't " +
                          "confirm it: check it with get_squad."
                        : $"His menu doesn't show the status '{exit.Choice}': check it with get_squad.");
                return Result.Ok(exit);
            }

            var shown = wanted.Shows(exit.Status);

            // The game takes a recall without bringing him back at once: it greys "Trazer do empréstimo" out and says
            // "<player> vai voltar em breve ao seu clube.", while he stays out on loan for a few days. The entry was
            // runnable before the send, so the entry greyed out now is the game taking the recall.
            if (!shown && action == PlayerExitAction.Recall && await RecallGreyedOutAsync(playerId, menuAction, cancellationToken))
            {
                shown = true;
                exit.Notes.Add($"The game took the recall: it greys the entry out now and {player.Name} comes back to the club soon " +
                               "(a few days of the game's calendar). Until then his menu still shows him out on loan.");
            }

            exit.Confirmed = shown;
            // With the answer lost, only the change showing proves it was taken; not showing it yet doesn't prove it wasn't.
            exit.Applied = written == PlayerExitWrite.Unknown && !shown ? PlayerExitWrite.Unknown : PlayerExitWrite.Written;

            if (!shown)
            {
                exit.Notes.Add($"Warning: after the entry was sent, the game's menu doesn't show {player.Name} {wanted.What}: " +
                               $"it shows him {Describe(exit.Status)}." +
                               (written == PlayerExitWrite.Unknown
                                   ? " The game's answer was lost too, so it may still take it: check with get_squad before sending it again."
                                   : string.Empty));
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

    // True when the menu still lists the recall but greys out every entry of it. A read that fails proves nothing.
    private async Task<bool> RecallGreyedOutAsync(int playerId, PlayerMenuAction menuAction, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await editor.GetMenuAsync(playerId, menuAction, cancellationToken);
            return menu is { Count: > 0 } && menu.All(entry => entry.IsDisabled);
        }
        catch (Exception exception) when (exception is GameUnavailableException or GameRejectedException)
        {
            return false;
        }
    }

    // Where he stands once the menu shows him as the entry puts him, or as it last showed him. An entry whose effect
    // the menu doesn't show is read once.
    private async Task<PlayerExitStatus> StatusAfterAsync(
        int playerId, Func<PlayerExitStatus, bool>? shows, CancellationToken cancellationToken)
    {
        PlayerExitStatus status = new();

        for (var check = 0; check < (shows is null ? 1 : StateChecks); check++)
        {
            await Task.Delay(StatePoll, cancellationToken);
            status = StatusOf(await editor.GetOfferedActionsAsync(playerId, cancellationToken));

            if (shows?.Invoke(status) == true)
            {
                return status;
            }
        }

        return status;
    }

    // Where the entry puts him, as his menu shows it; null when the menu doesn't show it: an offer to clubs or a loan
    // extension (the clubs answer later), or a status other than listed or not listed (the answers to offers, say).
    // A status entry is read by its labels, in the game's display language (Portuguese, or English).
    private static (Func<PlayerExitStatus, bool> Shows, string What)? Expected(PlayerExitAction action, List<string> labels)
    {
        switch (action)
        {
            case PlayerExitAction.RemoveFromTransferList:
                return (status => !status.TransferListed, "off the transfer list");
            case PlayerExitAction.RemoveFromLoanList:
                return (status => !status.LoanListed, "off the loan list");
            case PlayerExitAction.Recall:
                return (status => !status.OutOnLoan, "back from loan");
            case PlayerExitAction.SetTransferStatus when labels.Count == 2:
                break;
            default:
                return null;
        }

        var (top, leaf) = (labels[0].Trim(), labels[1].Trim());

        if (StartsWithAny(top, TransferStatusMenus))
        {
            return StartsWithAny(leaf, Listed) ? (status => status.TransferListed, "on the transfer list")
                : StartsWithAny(leaf, NotListed) ? (status => !status.TransferListed, "off the transfer list")
                : null;
        }

        if (StartsWithAny(top, LoanStatusMenus))
        {
            // Off the loan list are both "no status" and "unavailable": his menu shows them alike, so neither is expected.
            return StartsWithAny(leaf, LoanListed) ? (status => status.LoanListed, "on the loan list") : null;
        }

        return null;
    }

    // The transfer status menu's labels: "Situação de transferência" > "Na lista de transferências" / "Sem situação
    // definida"; "Situação de empréstimo" > "Disponível para empréstimo" / "Sem situação definida" / "Indisponível".
    private static readonly string[] TransferStatusMenus = ["Situação de transferência", "Transfer status"];
    private static readonly string[] LoanStatusMenus = ["Situação de empréstimo", "Loan status"];
    private static readonly string[] Listed = ["Na lista", "Transfer list"];
    private static readonly string[] NotListed = ["Sem situação", "No status", "Not listed"];
    private static readonly string[] LoanListed = ["Disponível", "Loan list", "Available"];
    private static readonly string[] NotLoanListed = ["Sem situação", "Indisponível", "No status", "Unavailable", "Not listed"];

    // A loan status off the loan list: "no status" and "unavailable for loan" look the same in his menu.
    private static bool CantTellApart(PlayerExitAction action, List<string> labels)
    {
        return action == PlayerExitAction.SetTransferStatus && labels.Count == 2
               && StartsWithAny(labels[0].Trim(), LoanStatusMenus) && StartsWithAny(labels[1].Trim(), NotLoanListed);
    }

    private static bool StartsWithAny(string label, string[] prefixes)
    {
        return prefixes.Any(prefix => label.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase));
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
