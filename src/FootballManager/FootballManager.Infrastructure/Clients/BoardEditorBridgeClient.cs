using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Board;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// The board through the MCPFM plugin, as the club's menu offers it: game.Actions lists the actions the club (the menu's
// "Records") offers; game.Action is one of them, its Items the entries. An entry sending EventFmxDoAction runs through
// /native, as the scouting menus do; one opening a panel (the board meeting card) runs through /newsaction with the
// references it carries. The talk it opens is read from the /ui tree and its lines pressed with /click.
internal sealed class BoardEditorBridgeClient(GameBridge bridge) : IBoardEditorGameClient
{
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(500);

    // An answer came within one or two seconds in the vision meeting; the budget panel opens within a few.
    private const int AnswerChecks = 12;
    private const int PanelChecks = 10;

    public async Task<HashSet<BoardMenuAction>> GetOfferedActionsAsync(CancellationToken cancellationToken = default)
    {
        var keys = await OfferedAsync(cancellationToken);
        return Enum.GetValues<BoardMenuAction>()
            .Where(action => keys.Contains(ActionKey(BoardEditingConstants.ActionIdOf(action))))
            .ToHashSet();
    }

    public async Task<List<MenuEntry>?> GetMenuAsync(BoardMenuAction action, CancellationToken cancellationToken = default)
    {
        return await MenuAsync(BoardEditingConstants.ActionIdOf(action), cancellationToken) is { } items
            ? items.Adapt<List<MenuEntry>>()
            : null;
    }

    public async Task RunMenuEntryAsync(BoardMenuAction action, int entry, CancellationToken cancellationToken = default)
    {
        var actionId = BoardEditingConstants.ActionIdOf(action);

        if (await MenuAsync(actionId, cancellationToken) is not { } items || entry >= items.Count)
        {
            throw new GameRejectedException($"The club's menu no longer offers {action}.");
        }

        var item = items[entry];
        var data = $"@{ScoutingConstants.MenuItems}[{Format(entry)}].ActionData";
        string write;

        if (item.Event == ScoutingConstants.DoActionEventId)
        {
            write = OnAction(actionId).ToWrite("/native", ("event", ScoutingConstants.DoActionEvent), ("field", $"Data:{data}"), ("template", "none"));
        }
        else if (item.ActionData?.NewsAction is { } panel and >= 0)
        {
            write = OnAction(actionId).ToWrite("/newsaction", ("action", Format(panel)),
                ("card", Format(item.ActionData.DynamicCardId ?? -1)), ("field", $"References:{data}.ActionDataReferences"));
        }
        else
        {
            write = OnAction(actionId).ToWrite(ScoutingConstants.ContextActionEndpoint, ("action", Format(actionId)), ("field", $"Data:{data}"));
        }

        await bridge.WriteAsync<JsonElement>(write, cancellationToken);
    }

    public async Task<BoardCard?> ReadCardAsync(CancellationToken cancellationToken = default)
    {
        var lines = await LinesAsync(cancellationToken);
        return lines.Count == 0 ? null : new BoardCard { Lines = lines.Select(line => line.Text).ToList() };
    }

    public async Task<BoardCard?> SayAsync(int line, CancellationToken cancellationToken = default)
    {
        var lines = await LinesAsync(cancellationToken);

        if (line >= lines.Count)
        {
            throw new GameRejectedException($"The talk has no line {line}.");
        }

        var before = lines.Select(item => item.Text).ToList();

        await ClickAsync(lines[line].Click, cancellationToken);

        // The board answers after the line, sometimes in steps: wait until the lines change, then until they hold still.
        List<string> last = before;

        for (var check = 0; check < AnswerChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);
            var now = (await LinesAsync(cancellationToken)).Select(item => item.Text).ToList();

            if (!now.SequenceEqual(before) && now.SequenceEqual(last))
            {
                return now.Count == 0 ? null : new BoardCard { Lines = now };
            }

            last = now;
        }

        return last.Count == 0 ? null : new BoardCard { Lines = last };
    }

    public async Task<string?> GetBudgetsAsync(CancellationToken cancellationToken = default)
    {
        return await MenuAsync(BoardEditingConstants.BudgetAdjustmentAction, cancellationToken) is [var entry, ..]
            ? entry.SecondaryText ?? string.Empty
            : null;
    }

    public async Task<BudgetPanel?> OpenBudgetPanelAsync(CancellationToken cancellationToken = default)
    {
        if (Panel(await BudgetDialogAsync(cancellationToken)) is { } open)
        {
            return open;
        }

        // The entry opens the panel with the references it carries, as make_transfer_offer opens the offer panel.
        await bridge.WriteAsync<JsonElement>(
            OnAction(BoardEditingConstants.BudgetAdjustmentAction).ToWrite("/newsaction",
                ("action", Format(BoardEditingConstants.BudgetPanelNewsAction)),
                ("field", $"References:@{ScoutingConstants.MenuItems}[0].ActionData.ActionDataReferences")),
            cancellationToken);

        for (var check = 0; check < PanelChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (Panel(await BudgetDialogAsync(cancellationToken)) is { } panel)
            {
                return panel;
            }
        }

        return null;
    }

    public async Task<BudgetPanel?> MoveBudgetAsync(int steps, CancellationToken cancellationToken = default)
    {
        var button = steps > 0 ? BoardEditingConstants.DownButton : BoardEditingConstants.UpButton;

        for (var i = 0; i < Math.Abs(steps); i++)
        {
            var dialog = await BudgetDialogAsync(cancellationToken)
                         ?? throw new GameRejectedException("The budget panel closed before the change.");
            await ClickAsync(Find(dialog, button)?.Path ?? throw new GameRejectedException("The budget panel has no button to move the split."), cancellationToken);
            await Task.Delay(Poll, cancellationToken);
        }

        // The edited split, as the panel shows it before the confirm closes it.
        var edited = await BudgetDialogAsync(cancellationToken)
                     ?? throw new GameRejectedException("The budget panel closed before the confirm.");
        var confirm = Find(edited, BoardEditingConstants.ConfirmButton)
                      ?? throw new GameRejectedException("The budget panel has no confirm button.");
        var after = Panel(edited);

        await ClickAsync(confirm.Path, cancellationToken);
        await Task.Delay(Poll, cancellationToken);
        return after;
    }

    // The budget panel's elements, the dialog first; null while it's closed.
    private async Task<List<UiElementLine>?> BudgetDialogAsync(CancellationToken cancellationToken)
    {
        var elements = (await bridge.GetTextAsync(BoardEditingConstants.BudgetPanelUiPath, cancellationToken))
            .Split('\n')
            .Select(UiElementLine.Parse)
            .OfType<UiElementLine>()
            .ToList();

        var dialog = elements.FirstOrDefault(element => element.Type == BoardEditingConstants.BudgetDialog && element.IsShown);
        return dialog is null ? null : elements.Where(element => element.IsWithin(dialog.Path)).ToList();
    }

    private static BudgetPanel? Panel(List<UiElementLine>? dialog)
    {
        if (dialog is null)
        {
            return null;
        }

        var texts = dialog.Where(element => !string.IsNullOrEmpty(element.Text)).ToList();

        return new BudgetPanel
        {
            WageBudget = After(texts, BoardEditingConstants.WageLabel),
            TransferBudget = After(texts, BoardEditingConstants.TransferLabel),
            Board = dialog.FirstOrDefault(element => element.Type == BoardEditingConstants.BoardText)?.Text,
            CanMoveToWages = IsEnabled(dialog, BoardEditingConstants.DownEnabled),
            CanMoveToTransfers = IsEnabled(dialog, BoardEditingConstants.UpEnabled),
            SliderPosition = SliderPosition(dialog)
        };
    }

    // A budget's value is the text right after its label; the "*" the panel adds to an edited one is left out.
    private static string? After(List<UiElementLine> texts, string label)
    {
        var at = texts.FindIndex(element => element.Text == label);
        return at >= 0 && at + 1 < texts.Count ? texts[at + 1].Text!.TrimEnd(BoardEditingConstants.EditedMark[0]) : null;
    }

    private static bool IsEnabled(List<UiElementLine> dialog, string enabler)
    {
        return dialog.FirstOrDefault(element => element.Type == enabler) is { } found
               && !found.Classes.Contains(BoardEditingConstants.Disabled);
    }

    // The dragger's place along the slider's track: 0 at the wage end (left), 1 at the transfer end (right).
    private static float SliderPosition(List<UiElementLine> dialog)
    {
        var track = dialog.FirstOrDefault(element => element.Type == BoardEditingConstants.SliderTracker)?.Bounds;
        var dragger = dialog.FirstOrDefault(element => element.Type == BoardEditingConstants.SliderDragger)?.Bounds;

        if (track is not { Width: > 0 } line || dragger is not { } knob || line.Width <= knob.Width)
        {
            return 0;
        }

        return Math.Clamp((knob.X - line.X) / (line.Width - knob.Width), 0, 1);
    }

    private Task<JsonElement> ClickAsync(string path, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write(ConversationConstants.ClickEndpoint, [("path", path), ("how", ConversationConstants.ClickHow)]),
            cancellationToken);
    }

    private static UiElementLine? Find(List<UiElementLine> dialog, string type)
    {
        return dialog.FirstOrDefault(element => element.Type.StartsWith(type, StringComparison.Ordinal));
    }

    // The talk's lines on screen, in order: the text to show and the path that takes the click.
    private async Task<List<(string Text, string Click)>> LinesAsync(CancellationToken cancellationToken)
    {
        var elements = (await bridge.GetTextAsync(BoardEditingConstants.UiPath, cancellationToken))
            .Split('\n')
            .Select(UiElementLine.Parse)
            .OfType<UiElementLine>()
            .Where(element => element.IsShown)
            .ToList();

        var lines = new List<(string Text, string Click)>();

        // The meeting card's own way out, after the lines.
        var exit = elements.FirstOrDefault(element => element.Text == BoardEditingConstants.LeaveMeeting) is { } label
            ? elements.Where(element => element.IsButton && label.IsWithin(element.Path)).MaxBy(element => element.Path.Length)
            : null;

        foreach (var answer in elements.Where(IsLine))
        {
            // A line inside another (the meeting's inner button) is the same line.
            if (lines.Any(line => answer.IsWithin(line.Click)))
            {
                continue;
            }

            var inside = elements.Where(element => element.IsWithin(answer.Path)).ToList();
            var click = answer.Type.Contains(BoardEditingConstants.MeetingAnswer, StringComparison.Ordinal)
                ? inside.FirstOrDefault(element => element != answer && element.IsButton) ?? answer
                : answer;
            var text = string.Join(" | ", inside.Select(element => element.Text).OfType<string>().Where(value => value.Length > 0).Distinct());

            if (text.Length > 0)
            {
                lines.Add((text, click.Path));
            }
        }

        if (exit is not null && lines.All(line => line.Click != exit.Path))
        {
            lines.Add((BoardEditingConstants.LeaveMeeting, exit.Path));
        }

        return lines;
    }

    private static bool IsLine(UiElementLine element)
    {
        return element.Type.Contains(BoardEditingConstants.MeetingAnswer, StringComparison.Ordinal)
               || element.Classes.Any(name => name.Contains(BoardEditingConstants.SuggestionClass, StringComparison.Ordinal));
    }

    private async Task<List<BridgeMenuItemDto>?> MenuAsync(uint actionId, CancellationToken cancellationToken)
    {
        if (!(await OfferedAsync(cancellationToken)).Contains(ActionKey(actionId)))
        {
            return null;
        }

        var menu = await bridge.GetRecordAsync<BridgeMenuDto>(OnAction(actionId).Props([ScoutingConstants.MenuItems]), cancellationToken);
        return menu.Values.Items;
    }

    // The menu actions the club's menu offers now, keyed as the game lists them ("#<action id>").
    private async Task<HashSet<string>> OfferedAsync(CancellationToken cancellationToken)
    {
        var offered = await bridge.GetRecordAsync<BridgeActionsDto>(
            BridgeQuery.FromGame()
                .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, BoardEditingConstants.Club)
                .WithHumanAndTeam()
                .Props([ScoutingConstants.Actions]),
            cancellationToken);

        return offered.Values.Actions?.Keys.ToHashSet() ?? [];
    }

    private static BridgeQuery OnAction(uint actionId)
    {
        return BridgeQuery.FromGame()
            .Base(ScoutingConstants.Action)
            .Context(ScoutingConstants.ActionId, actionId)
            .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, BoardEditingConstants.Club)
            .WithHumanAndTeam();
    }

    private static string ActionKey(uint actionId)
    {
        return $"#{Format(actionId)}";
    }

    private static string Format(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
