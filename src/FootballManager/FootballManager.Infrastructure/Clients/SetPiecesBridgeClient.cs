using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.SetPieces;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Conversations;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.SetPieces;

namespace FootballManager.Infrastructure.Clients;

// The captaincy and the set piece takers through the MCPFM plugin. Reads come from the team and the active tactic's
// SetPiece record; the captain and the vice change through the player's menu, as its "Tornar Capitão" entry does.
// Changing the takers isn't here yet: the set piece screen's events haven't been proven in the test save.
internal sealed class SetPiecesBridgeClient(GameBridge bridge) : ISetPiecesGameClient
{
    private static readonly string[] ListedPlayer = [SetPieceConstants.PlayerId, SetPieceConstants.PlayerName];

    public async Task<SetPieceSetup> ReadAsync(CancellationToken cancellationToken = default)
    {
        var team = await bridge.GetRecordAsync<BridgeCaptaincyDto>(
            BridgeQuery.FromHuman().Base(SetPieceConstants.Team).WithHumanAndTeam().Props(
                [SetPieceConstants.CaptainId, SetPieceConstants.CaptainName, SetPieceConstants.ViceCaptainId, SetPieceConstants.ViceCaptainName]),
            cancellationToken);

        // A role the game didn't answer, or failed to read, is unknown: read as none, the plan and the read-back after a
        // write would both go wrong (a vice that is the captain, a lost write taken as not written).
        foreach (var role in new[] { SetPieceConstants.CaptainId, SetPieceConstants.ViceCaptainId })
        {
            if (team.Pending?.Contains(role) == true)
            {
                throw new GameUnavailableException($"The game didn't answer {role} (it may be busy).");
            }

            if (team.Errors?.TryGetValue(role, out var error) == true)
            {
                throw new GameUnavailableException($"The game failed to read {role}: {error}");
            }
        }

        var squad = await bridge.GetListAsync<BridgeConversationPlayerDto>(
            BridgeQuery.FromHuman().List(SetPieceConstants.Squad).Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken);

        var lists = new List<SetPieceTakerList>
        {
            new()
            {
                Key = SetPieceConstants.PenaltiesKey,
                Label = "Penalties",
                Takers = await ListedAsync(OnSetPiece(SetPieceConstants.PenaltyTakers), cancellationToken)
            }
        };

        foreach (var (key, type, label) in SetPieceConstants.SideLists)
        {
            foreach (var (side, sideName) in new[] { (SetPieceConstants.LeftSide, "Left"), (SetPieceConstants.RightSide, "Right") })
            {
                lists.Add(new SetPieceTakerList
                {
                    Key = key + sideName,
                    Label = $"{label}, {sideName.ToLowerInvariant()}",
                    SideVerified = SetPieceConstants.SidesVerified,
                    Takers = await ListedAsync(
                        OnSetPiece(SetPieceConstants.SideTakers)
                            .Context(SetPieceConstants.TypeContext, type)
                            .Context(SetPieceConstants.ScenarioContext, SetPieceConstants.Attacking)
                            .Context(SetPieceConstants.SideContext, side),
                        cancellationToken)
                });
            }
        }

        return new SetPieceSetup
        {
            Captain = Player(team.Values.CaptainId, team.Values.CaptainName),
            ViceCaptain = Player(team.Values.ViceCaptainId, team.Values.ViceCaptainName),
            CaptainStandIns = await ListedAsync(OnSetPiece(SetPieceConstants.CaptainStandIns), cancellationToken),
            TakerLists = lists,
            Squad = squad.Items
                .Select(item => Player(item.Values.Id, item.Values.Name))
                .OfType<SetPiecePlayer>()
                .ToList()
        };
    }

    public Task MakeCaptainAsync(int playerId, CancellationToken cancellationToken = default)
    {
        return RunMenuEntryAsync(playerId, SetPieceConstants.MakeCaptainAction, "Tornar Capitão", cancellationToken);
    }

    public Task MakeViceCaptainAsync(int playerId, CancellationToken cancellationToken = default)
    {
        return RunMenuEntryAsync(playerId, SetPieceConstants.MakeViceCaptainAction, "Tornar Vice-capitão", cancellationToken);
    }

    public async Task<CaptaincyOffer> GetCaptaincyOfferAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var offered = await OfferedAsync(playerId, cancellationToken);

        return new CaptaincyOffer
        {
            MakeCaptain = offered.Contains(ActionKey(SetPieceConstants.MakeCaptainAction)),
            MakeViceCaptain = offered.Contains(ActionKey(SetPieceConstants.MakeViceCaptainAction))
        };
    }

    // The entry sends EventFmxDoAction with the payload the game built for it; the game asks no confirmation of it
    // (the confirmation dialog is the menu click's).
    private async Task RunMenuEntryAsync(int playerId, uint actionId, string entry, CancellationToken cancellationToken)
    {
        if (!(await OfferedAsync(playerId, cancellationToken)).Contains(ActionKey(actionId)))
        {
            throw new GameRejectedException($"The player's menu doesn't offer \"{entry}\" now.");
        }

        // The send picks the menu's first entry (EntryData), so that entry is read right before it: it must be the one
        // named, not greyed out, and send EventFmxDoAction. The plugin reads a missing event as 0, which would run as
        // something else, so anything other than that event is refused without sending.
        var menu = await bridge.GetRecordAsync<BridgeMenuDto>(OnAction(playerId, actionId).Props([ScoutingConstants.MenuItems]), cancellationToken);
        var item = menu.Values.Items.FirstOrDefault();

        if (item is null
            || !string.Equals(item.Text ?? item.RowData?.Text, entry, StringComparison.CurrentCultureIgnoreCase)
            || item.Disabled == true
            || item.Event != ScoutingConstants.DoActionEventId)
        {
            throw new GameRejectedException(
                $"The player's menu entry for \"{entry}\" isn't the one this tool knows how to send " +
                $"(found: \"{item?.Text ?? "none"}\", event {item?.Event?.ToString(CultureInfo.InvariantCulture) ?? "none"}" +
                $"{(item?.Disabled == true ? ", greyed out" : "")}). Nothing was sent.");
        }

        await bridge.WriteAsync<JsonElement>(
            OnAction(playerId, actionId)
                .ToWrite("/native", ("event", ScoutingConstants.DoActionEvent), ("field", SetPieceConstants.EntryData), ("template", "none")),
            cancellationToken);
    }

    // The menu actions the game offers for the player now, keyed as the game lists them ("#<action id>").
    private async Task<HashSet<string>> OfferedAsync(int playerId, CancellationToken cancellationToken)
    {
        var offered = await bridge.GetRecordAsync<BridgeActionsDto>(
            BridgeQuery.FromGame().RefList(ScoutingConstants.Records, GameConstants.HumanRoot, PlayerPath(playerId))
                .WithHumanAndTeam().Props([ScoutingConstants.Actions]),
            cancellationToken);

        return offered.Values.Actions?.Keys.ToHashSet() ?? [];
    }

    private static BridgeQuery OnAction(int playerId, uint actionId)
    {
        return BridgeQuery.FromGame()
            .Base(ScoutingConstants.Action)
            .Context(ScoutingConstants.ActionId, actionId)
            .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, PlayerPath(playerId))
            .WithHumanAndTeam();
    }

    private static string ActionKey(uint actionId)
    {
        return $"#{actionId.ToString(CultureInfo.InvariantCulture)}";
    }

    private static BridgeQuery OnSetPiece(string list)
    {
        return BridgeQuery.FromHuman().List(list).Context(TacticConstants.TacticFlag, true).WithHumanAndTeam();
    }

    private async Task<List<SetPiecePlayer>> ListedAsync(BridgeQuery query, CancellationToken cancellationToken)
    {
        var listed = await bridge.GetListAsync<BridgeSetPieceListedPlayerDto>(query.Props(ListedPlayer), cancellationToken);

        return listed.Items
            .Select(item => Player(item.Values.Id, item.Values.Name))
            .OfType<SetPiecePlayer>()
            .ToList();
    }

    private static SetPiecePlayer? Player(int? id, string? name)
    {
        return id is { } found ? new SetPiecePlayer { Id = found, Name = name ?? string.Empty } : null;
    }

    private static string PlayerPath(int playerId)
    {
        return $"{SetPieceConstants.Squad}[{GameConstants.UniqueId}={playerId.ToString(CultureInfo.InvariantCulture)}]";
    }
}
