using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Board;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

public interface IBoardEditingService
{
    /// <summary>
    /// Asks the board for something through the club's menu, as the game's board screen does. A dry run unless
    /// <paramref name="apply"/>.
    /// </summary>
    /// <param name="entry">The menu entry to pick, by its label (case doesn't matter); needed to apply when the menu has several.</param>
    Task<Result<BoardRequest>> RequestAsync(BoardMenuAction action, string? entry, bool apply, CancellationToken cancellationToken = default);
}

// Requests to the board go through the club's menu, as the board screen's do: the game offers only what the board will
// hear now (an open request, or a recent refusal, takes the action away). Some entries send the request at once, with
// the game's notice that the board will answer later; others open a talk with the board, read back after the click.
public class BoardEditingService(IBoardEditorGameClient game, ISaveGuard saveGuard) : IBoardEditingService
{
    // The game shows a talk, or takes a request, a frame or more after the click.
    private static readonly TimeSpan SettlePause = TimeSpan.FromMilliseconds(800);

    public async Task<Result<BoardRequest>> RequestAsync(
        BoardMenuAction action, string? entry, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            var offered = await game.GetOfferedActionsAsync(cancellationToken);
            var plan = new BoardRequest { Action = action, Offered = offered.Order().ToList() };

            if (!offered.Contains(action) || await game.GetMenuAsync(action, cancellationToken) is not { Count: > 0 } menu)
            {
                var others = offered.Count == 0 ? "no request to the board" : string.Join(", ", plan.Offered);
                return Fail<BoardRequest>("action", $"The club's menu doesn't offer {action} now (the board won't hear it yet); it offers {others}.");
            }

            plan.Menu = menu;
            var choices = menu.Select((item, index) => (Entry: item, Index: index)).Where(choice => !choice.Entry.Disabled).ToList();
            var picked = Pick(choices, entry);

            if (!string.IsNullOrWhiteSpace(entry) && picked is null)
            {
                return Fail<BoardRequest>("entry", $"The menu has no entry '{entry}'; it has: {Labels(choices)}.");
            }

            if (picked is null && apply)
            {
                return Fail<BoardRequest>("entry", $"The menu has several entries: name the one to pick, among {Labels(choices)}.");
            }

            if (picked is { } chosen)
            {
                plan.Choice = chosen.Entry.Label;
                plan.Confirmation = chosen.Entry.Confirmation;
            }

            if (!apply)
            {
                return Result.Ok(plan);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<BoardRequest>(clearance.Errors);
            }

            plan.Notes.Add(SaveNote(clearance.Value));
            await game.RunMenuEntryAsync(action, picked!.Value.Index, cancellationToken);
            plan.Applied = true;

            await Task.Delay(SettlePause, cancellationToken);
            plan.Card = await game.ReadCardAsync(cancellationToken);
            return Result.Ok(plan);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<BoardRequest>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Fail<BoardRequest>("game", exception.Message);
        }
    }

    private static (MenuEntry Entry, int Index)? Pick(List<(MenuEntry Entry, int Index)> choices, string? entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
        {
            return choices.Count == 1 ? choices[0] : null;
        }

        foreach (var choice in choices)
        {
            if (string.Equals(choice.Entry.Label, entry.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                return choice;
            }
        }

        return null;
    }

    private static string Labels(IEnumerable<(MenuEntry Entry, int Index)> choices)
    {
        return string.Join("; ", choices.Select(choice => $"'{choice.Entry.Label}'"));
    }

    private static Result<T> Fail<T>(string property, string message)
    {
        return Result.Fail<T>(new ValidationError(property, [message]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
