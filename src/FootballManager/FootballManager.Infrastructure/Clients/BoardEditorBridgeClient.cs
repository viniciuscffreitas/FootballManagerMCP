using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Board;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Board;
using FootballManager.Infrastructure.Dtos.Scouting;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// The board through the MCPFM plugin, as the club's menu offers it: game.Actions lists the actions the club (the menu's
// "Records") offers; game.Action is one of them, its Items the entries. An entry sending EventFmxDoAction runs through
// /native, as the scouting menus do; one sending no event (a talk with the board) runs through /contextaction, as the
// menu's click does. The talk it opens is read from the /ui tree and its lines pressed with /click.
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

        var payload = $"Data:@{ScoutingConstants.MenuItems}[{Format(entry)}].ActionData";
        var write = items[entry].Event == ScoutingConstants.DoActionEventId
            ? OnAction(actionId).ToWrite("/native", ("event", ScoutingConstants.DoActionEvent), ("field", payload), ("template", "none"))
            : OnAction(actionId).ToWrite(ScoutingConstants.ContextActionEndpoint, ("action", Format(actionId)), ("field", payload));

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

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write(ConversationConstants.ClickEndpoint, [("path", lines[line].Click), ("how", ConversationConstants.ClickHow)]),
            cancellationToken);

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
        if (await ReadPanelAsync(cancellationToken) is { } open)
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

            if (await ReadPanelAsync(cancellationToken) is { } panel)
            {
                return panel;
            }
        }

        return null;
    }

    public async Task<BudgetPanel?> MoveBudgetAsync(int steps, CancellationToken cancellationToken = default)
    {
        var panel = BridgeQuery.FromHuman().Base(BoardEditingConstants.BudgetPanel).WithHumanAndTeam();
        var step = steps > 0 ? "1" : "-1";

        for (var i = 0; i < Math.Abs(steps); i++)
        {
            await bridge.WriteAsync<JsonElement>(
                panel.ToWrite("/native", ("event", BoardEditingConstants.AdjustEvent), ("field", $"Data:{step}"), ("template", "none")),
                cancellationToken);
        }

        await bridge.WriteAsync<JsonElement>(
            panel.ToWrite("/native", ("event", BoardEditingConstants.ConfirmEvent), ("data", "null"), ("template", "none")),
            cancellationToken);

        await Task.Delay(Poll, cancellationToken);
        return await ReadPanelAsync(cancellationToken);
    }

    // The panel as it reads now; null while it's closed (its record answers nothing then).
    private async Task<BudgetPanel?> ReadPanelAsync(CancellationToken cancellationToken)
    {
        var panel = await bridge.GetRecordAsync<BridgeBudgetPanelDto>(
            BridgeQuery.FromHuman().Base(BoardEditingConstants.BudgetPanel).WithHumanAndTeam().Props(BoardEditingConstants.PanelProperties),
            cancellationToken);

        return panel.Values.SliderValue is null ? null : panel.Values.Adapt<BudgetPanel>();
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
