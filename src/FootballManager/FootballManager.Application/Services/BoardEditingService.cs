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

    /// <summary>
    /// Says a line in the talk with the board open on screen: a request's, or the club vision and season expectations
    /// meeting's. A dry run unless <paramref name="apply"/>.
    /// </summary>
    /// <param name="line">The line by its number from 0 or by a piece of its text (case doesn't matter).</param>
    Task<Result<BoardAnswer>> AnswerAsync(string? line, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves money between the transfer and the wage budget with the game's budget adjustment slider. A dry run unless
    /// <paramref name="apply"/>; applying with no <paramref name="sliderValue"/> only opens the panel and reads it.
    /// </summary>
    Task<Result<BudgetAdjustment>> AdjustBudgetAsync(float? sliderValue, bool apply, CancellationToken cancellationToken = default);
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

    public async Task<Result<BoardAnswer>> AnswerAsync(string? line, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await game.ReadCardAsync(cancellationToken) is not { Lines.Count: > 0 } card)
            {
                return Fail<BoardAnswer>("line", "No talk with the board is open: open one with a request, or reach the club vision meeting.");
            }

            var answer = new BoardAnswer { Card = card };
            var index = LineIndex(card, line);

            if (!string.IsNullOrWhiteSpace(line) && index is null)
            {
                return Fail<BoardAnswer>("line", $"The talk has no line '{line}'; it has: {string.Join("; ", card.Lines.Select((text, i) => $"{i} '{text}'"))}.");
            }

            if (index is null && apply)
            {
                return Fail<BoardAnswer>("line", "Name the line to say, by its number or a piece of its text.");
            }

            answer.Line = index is { } picked ? card.Lines[picked] : null;

            if (!apply)
            {
                return Result.Ok(answer);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<BoardAnswer>(clearance.Errors);
            }

            answer.Notes.Add(SaveNote(clearance.Value));
            answer.After = await game.SayAsync(index!.Value, cancellationToken);
            answer.Applied = true;
            return Result.Ok(answer);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<BoardAnswer>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Fail<BoardAnswer>("game", exception.Message);
        }
    }

    public async Task<Result<BudgetAdjustment>> AdjustBudgetAsync(float? sliderValue, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            // The club's menu sums up both budgets, and offers the adjustment only while the board lets the user move them.
            if (await game.GetBudgetsAsync(cancellationToken) is not { } budgets)
            {
                return Fail<BudgetAdjustment>("budget", "The club's menu doesn't offer a budget adjustment now.");
            }

            var plan = new BudgetAdjustment { Budgets = budgets, SliderValue = sliderValue };

            if (!apply)
            {
                return Result.Ok(plan);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<BudgetAdjustment>(clearance.Errors);
            }

            plan.Notes.Add(SaveNote(clearance.Value));

            if (await game.OpenBudgetPanelAsync(cancellationToken) is not { } panel)
            {
                return Fail<BudgetAdjustment>("budget", "The budget adjustment panel didn't open.");
            }

            plan.Before = panel;

            if (sliderValue is not { } value)
            {
                plan.Applied = true;
                return Result.Ok(plan);
            }

            if (!panel.CanEdit)
            {
                return Fail<BudgetAdjustment>("sliderValue", $"The board doesn't let the user move the budgets now: {panel.Board}");
            }

            plan.After = await game.SetBudgetSliderAsync(value, cancellationToken);
            plan.Applied = true;
            return Result.Ok(plan);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<BudgetAdjustment>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Fail<BudgetAdjustment>("game", exception.Message);
        }
    }

    // A number picks the line at that place; text picks the first line holding it.
    private static int? LineIndex(BoardCard card, string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return card.Lines.Count == 1 ? 0 : null;
        }

        if (int.TryParse(line.Trim(), out var number))
        {
            return number >= 0 && number < card.Lines.Count ? number : null;
        }

        var found = card.Lines.FindIndex(text => text.Contains(line.Trim(), StringComparison.CurrentCultureIgnoreCase));
        return found >= 0 ? found : null;
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
