using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Opposition;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Opposition;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Infrastructure.Clients;

// The opposition instructions screen through the MCPFM plugin. The game binds it on the active tactic: a record of rows
// for the next opponent, each holding its player by record, which the opponent's squad list names. An instruction is
// changed the way the screen's dropdowns do it: a /set of the tactic's value for the selected row.
internal sealed class OppositionInstructionsBridgeClient(GameBridge bridge) : IOppositionInstructionsGameClient
{
    public async Task<OppositionInstructions?> GetAsync(CancellationToken cancellationToken = default)
    {
        var (instructions, _) = await ReadAsync(cancellationToken);
        return instructions;
    }

    public async Task SetAsync(int playerId, OppositionInstruction instruction, string value, CancellationToken cancellationToken = default)
    {
        // The lineup the game expects can change between the plan and the send (the save guard copies the save first):
        // the row is found again right before the write, so the instruction never lands on another player.
        var (instructions, places) = await ReadAsync(cancellationToken);

        if (instructions is null || !places.TryGetValue(playerId, out var place))
        {
            throw new GameRejectedException($"Player {playerId} is no longer in the lineup the game expects from the next opponent.");
        }

        var number = OppositionInstructionConstants.ValueOf(instruction, value);

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.FromHuman()
                .Base(TacticConstants.Tactic)
                .Context(TacticConstants.TacticFlag, true)
                .Context(TacticConstants.TeamSelectionIndex, place.Index)
                .Context(TacticConstants.PositionMask, unchecked((long)place.Position))
                .Ref(OppositionInstructionConstants.Selection, $"{OppositionInstructionConstants.LineupRows}[Index={place.Index}]")
                .ToWrite("/set", ("path", OppositionInstructionConstants.Property(instruction)), ("value", number.ToString(CultureInfo.InvariantCulture))),
            cancellationToken);
    }

    // The screen, and each lineup player's place on it (his row's Index and position mask) by FM id.
    private async Task<(OppositionInstructions? Instructions, Dictionary<int, (int Index, ulong Position)> Places)> ReadAsync(
        CancellationToken cancellationToken)
    {
        var screen = await bridge.GetRecordAsync<BridgeOppositionInstructionsDto>(
            BridgeQuery.OnActiveTactic().Props(OppositionInstructionConstants.Properties), cancellationToken);

        // Rows the game didn't send are unknown, not empty: reporting them as no instructions would be wrong.
        if (screen.Pending is { Count: > 0 } || screen.Errors is { Count: > 0 })
        {
            var missing = (screen.Pending ?? []).Concat(screen.Errors?.Keys ?? Enumerable.Empty<string>());
            throw new GameUnavailableException($"The game didn't send the opposition instructions ({string.Join(", ", missing)}).");
        }

        // Without a match ahead the game has no opponent to instruct against.
        if (screen.Values is not { TeamId: { } teamId, Instructions: { } rows })
        {
            return (null, []);
        }

        var squad = await bridge.GetListAsync<BridgePlayerDto>(
            BridgeQuery.FromHuman()
                .List(OppositionInstructionConstants.TeamPlayers)
                .Context(TacticConstants.TacticFlag, true)
                .Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken,
            gapsExpected: true);
        var players = squad.Items
            .Where(player => player.Source?.Index is not null && player.Values.Id is not null)
            .GroupBy(player => player.Source!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First().Values);

        var places = new Dictionary<int, (int, ulong)>();
        var lineup = new List<OppositionPlayerInstructions>();

        foreach (var row in rows.OppositionPredictedLineupInstructions ?? [])
        {
            var named = Row(row, players);

            if (named.PlayerId is { } id && row is { Index: { } index and >= 0, Position: { } position })
            {
                places[id] = (index, position);
            }

            lineup.Add(named);
        }

        var instructions = new OppositionInstructions
        {
            TeamId = teamId,
            TeamName = screen.Values.TeamName ?? string.Empty,
            Players = lineup,
            Positions = (rows.OppositionAllPositionsInstructions ?? []).Select(row => Row(row, players)).ToList(),
            StaffSuggestions = (screen.Values.Suggestions ?? []).Select(suggestion => Suggestion(suggestion, players)).ToList()
        };

        return (instructions, places);
    }

    private static OppositionPlayerInstructions Row(BridgeOppositionRowDto row, Dictionary<int, BridgePlayerDto> players)
    {
        var player = row.Player?.Index is { } record ? players.GetValueOrDefault(record) : null;

        return new OppositionPlayerInstructions
        {
            PlayerId = player?.Id,
            Name = player?.Name,
            Position = row.Position is { } mask ? PositionMasks.ToPosition(mask) : null,
            Marking = OppositionInstructionConstants.KeyOf(OppositionInstruction.Marking, row.OppositionPlayerMarking),
            Pressing = OppositionInstructionConstants.KeyOf(OppositionInstruction.Pressing, row.OppositionPlayerPressingTrigger),
            Tackling = OppositionInstructionConstants.KeyOf(OppositionInstruction.Tackling, row.OppositionPlayerTackling),
            ShowOntoFoot = OppositionInstructionConstants.KeyOf(OppositionInstruction.ShowOntoFoot, row.OppositionPlayerShowOnToFoot)
        };
    }

    private static OppositionStaffSuggestion Suggestion(BridgeOppositionSuggestionDto suggestion, Dictionary<int, BridgePlayerDto> players)
    {
        var player = suggestion.Player?.Index is { } record ? players.GetValueOrDefault(record) : null;
        var mapped = OppositionInstructionConstants.Suggested(suggestion.FeedbackType);

        return new OppositionStaffSuggestion
        {
            PlayerId = player?.Id,
            Name = player?.Name,
            Text = suggestion.FeedbackString,
            Instruction = mapped?.Instruction,
            Value = mapped?.Value,
            IsDisabled = suggestion.IsDisabled == true
        };
    }
}
