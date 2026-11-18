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
internal sealed class SetPiecesBridgeClient(GameBridge bridge) : ISetPiecesGameClient
{
    private static readonly string[] ListedPlayer = [SetPieceConstants.PlayerId, SetPieceConstants.PlayerName];

    public async Task<SetPieceSetup> ReadAsync(CancellationToken cancellationToken = default)
    {
        var team = await bridge.GetRecordAsync<BridgeCaptaincyDto>(
            BridgeQuery.FromHuman().Base(SetPieceConstants.Team).WithHumanAndTeam().Props(
                [SetPieceConstants.CaptainId, SetPieceConstants.CaptainName, SetPieceConstants.ViceCaptainId, SetPieceConstants.ViceCaptainName]),
            cancellationToken);

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

    public Task SetTakersAsync(string list, IReadOnlyList<int> playerIds, CancellationToken cancellationToken = default)
    {
        throw new GameRejectedException(
            "Changing the takers isn't wired to the game yet: the set piece screen's events haven't been proven in the test save.");
    }

    public Task AskStaffToAssignTakersAsync(CancellationToken cancellationToken = default)
    {
        throw new GameRejectedException(
            "Asking the staff isn't wired to the game yet: the set piece screen's events haven't been proven in the test save.");
    }

    // The entry sends EventFmxDoAction with the payload the game built for it; the game asks no confirmation of it
    // (the confirmation dialog is the menu click's).
    private async Task RunMenuEntryAsync(int playerId, uint actionId, string entry, CancellationToken cancellationToken)
    {
        var offered = await bridge.GetRecordAsync<BridgeActionsDto>(
            BridgeQuery.FromGame().RefList(ScoutingConstants.Records, GameConstants.HumanRoot, PlayerPath(playerId))
                .WithHumanAndTeam().Props([ScoutingConstants.Actions]),
            cancellationToken);

        if (offered.Values.Actions?.ContainsKey($"#{actionId.ToString(CultureInfo.InvariantCulture)}") != true)
        {
            throw new GameRejectedException($"The player's menu doesn't offer \"{entry}\" now.");
        }

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.FromGame()
                .Base(ScoutingConstants.Action)
                .Context(ScoutingConstants.ActionId, actionId)
                .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, PlayerPath(playerId))
                .WithHumanAndTeam()
                .ToWrite("/native", ("event", ScoutingConstants.DoActionEvent), ("field", SetPieceConstants.EntryData), ("template", "none")),
            cancellationToken);
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
