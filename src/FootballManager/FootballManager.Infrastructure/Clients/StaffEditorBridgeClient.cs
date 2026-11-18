using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Staff;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.Staff;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Staff menus through the MCPFM plugin, as the game builds them for its screens: game.Actions lists the actions the
// "Records" (a staff member's item in the first team's staff list, or the club) offer; game.Action is one of them, its
// Items the entries. An entry sending EventFmxDoAction runs through /native, as the scouting menus do; one sending no
// event (a dialog or a panel the click opens) runs through /contextaction, as the menu's click does.
internal sealed class StaffEditorBridgeClient(GameBridge bridge) : IStaffEditorGameClient
{
    public async Task<StaffMember?> FindMemberAsync(int staffId, CancellationToken cancellationToken = default)
    {
        var members = await bridge.GetListAsync<BridgeStaffMemberDto>(
            BridgeQuery.FromHuman().List(StaffConstants.Members).WithHumanAndTeam()
                .Props([GameConstants.UniqueId, GameConstants.Name, StaffConstants.Job]),
            cancellationToken, gapsExpected: true);

        return members.Items.Select(item => item.Values).FirstOrDefault(member => member.Id == staffId)?.Adapt<StaffMember>();
    }

    public async Task<HashSet<StaffMenuAction>> GetOfferedActionsAsync(int? staffId, CancellationToken cancellationToken = default)
    {
        var offered = new HashSet<StaffMenuAction>();

        foreach (var record in Records(staffId))
        {
            var keys = await OfferedAsync(record, cancellationToken);
            offered.UnionWith(Enum.GetValues<StaffMenuAction>().Where(action => keys.Contains(ActionKey(StaffConstants.ActionIdOf(action)))));
        }

        return offered;
    }

    public async Task<List<MenuEntry>?> GetMenuAsync(int? staffId, StaffMenuAction action, CancellationToken cancellationToken = default)
    {
        return await MenuAsync(staffId, action, cancellationToken) is { } menu
            ? menu.Items.Adapt<List<MenuEntry>>()
            : null;
    }

    public async Task RunMenuEntryAsync(
        int? staffId, StaffMenuAction action, IReadOnlyList<int> entry, IReadOnlyList<string> labels,
        CancellationToken cancellationToken = default)
    {
        if (entry.Count == 0 || labels.Count != entry.Count)
        {
            throw new ArgumentException("Name the menu entry to pick, with its label at each level.", nameof(entry));
        }

        if (await MenuAsync(staffId, action, cancellationToken) is not { } menu)
        {
            throw new GameRejectedException($"The game's menu no longer offers {action}.");
        }

        // The menu is read again here, after the save check: the game may have changed it since the entry was picked.
        // The entry must still be at that place with those labels, and not greyed out, or nothing is sent.
        var item = EntryAt(menu.Items, entry, labels)
                   ?? throw new GameRejectedException(
                       $"The game's menu for {action} changed since the check: it no longer has " +
                       $"'{string.Join(" > ", labels)}' to pick. Nothing was sent; run the dry run again.");

        // The entry's fields and payload, where the game keeps them: on a top entry itself, on one below in its RowData.
        var fields = Fields(item);
        var payload = $"@{ScoutingConstants.MenuItems}[{Format(entry[0])}]"
                      + string.Concat(entry.Skip(1).Select(index => $".ChildList.{Format(index)}"))
                      + (entry.Count > 1 ? ".RowData.ActionData" : ".ActionData");
        var actionId = StaffConstants.ActionIdOf(action);
        var query = OnAction(menu.Record, actionId);

        var write = fields.Event == ScoutingConstants.DoActionEventId
            ? query.ToWrite("/native", ("event", ScoutingConstants.DoActionEvent), ("field", $"Data:{payload}"), ("template", "none"))
            : query.ToWrite(ScoutingConstants.ContextActionEndpoint,
                ("action", actionId.ToString(CultureInfo.InvariantCulture)), ("field", $"Data:{payload}"));

        await bridge.WriteAsync<JsonElement>(write, cancellationToken);
    }

    // The entry at that place when each level still has that label (as the menu shows it, case aside) and the entry
    // can be picked: no sub-entries, not greyed out.
    private static BridgeMenuItemDto? EntryAt(List<BridgeMenuItemDto> items, IReadOnlyList<int> entry, IReadOnlyList<string> labels)
    {
        BridgeMenuItemDto? item = null;
        var level = items;

        for (var i = 0; i < entry.Count; i++)
        {
            if (level is null || entry[i] < 0 || entry[i] >= level.Count)
            {
                return null;
            }

            item = level[entry[i]];

            if (!string.Equals(Label(item), labels[i], StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            level = item.ChildList;
        }

        return item is null || item.ChildList is { Count: > 0 } || Fields(item).Disabled == true ? null : item;
    }

    private static BridgeMenuItemDto Fields(BridgeMenuItemDto item)
    {
        return item.Text is null && item.RowData is not null ? item.RowData : item;
    }

    private static string Label(BridgeMenuItemDto item)
    {
        return (string.IsNullOrWhiteSpace(item.Text) ? item.RowData?.Text : item.Text) ?? string.Empty;
    }

    // The action's entries, from the first record that offers it.
    private async Task<(string Record, List<BridgeMenuItemDto> Items)?> MenuAsync(int? staffId, StaffMenuAction action, CancellationToken cancellationToken)
    {
        var actionId = StaffConstants.ActionIdOf(action);

        foreach (var record in Records(staffId))
        {
            if (!(await OfferedAsync(record, cancellationToken)).Contains(ActionKey(actionId)))
            {
                continue;
            }

            var menu = await bridge.GetRecordAsync<BridgeMenuDto>(
                OnAction(record, actionId).Props([ScoutingConstants.MenuItems]), cancellationToken);
            return (record, menu.Values.Items);
        }

        return null;
    }

    private static IEnumerable<string> Records(int? staffId)
    {
        return staffId is { } id ? [StaffConstants.MemberRecord(id)] : StaffConstants.ClubRecords;
    }

    // The menu actions the game offers for the record now, keyed as the game lists them ("#<action id>").
    private async Task<HashSet<string>> OfferedAsync(string record, CancellationToken cancellationToken)
    {
        var offered = await bridge.GetRecordAsync<BridgeActionsDto>(
            BridgeQuery.FromGame()
                .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, record)
                .WithHumanAndTeam()
                .Props([ScoutingConstants.Actions]),
            cancellationToken);

        return offered.Values.Actions?.Keys.ToHashSet() ?? [];
    }

    private static BridgeQuery OnAction(string record, uint actionId)
    {
        return BridgeQuery.FromGame()
            .Base(ScoutingConstants.Action)
            .Context(ScoutingConstants.ActionId, actionId)
            .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, record)
            .WithHumanAndTeam();
    }

    private static string ActionKey(uint actionId)
    {
        return $"#{actionId.ToString(CultureInfo.InvariantCulture)}";
    }

    private static string Format(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
