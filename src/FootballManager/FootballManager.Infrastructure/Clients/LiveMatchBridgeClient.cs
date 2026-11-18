using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Conversations;
using FootballManager.Domain.Matches;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Matches;

namespace FootballManager.Infrastructure.Clients;

// Changes to the match being played through the plugin. Everything read while the ball rolls is a light /get on the
// Match root or the team's tactic; only the team talk, which shows while the match waits, is read from the UI tree.
internal sealed class LiveMatchBridgeClient(GameBridge bridge, GameVocabulary vocabulary) : ILiveMatchGameClient
{
    // The tactic swaps a substitution in within a frame or two; the team talk answers a line within a second.
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(250);
    private const int SwapChecks = 4;
    private const int AnswerChecks = 4;

    public async Task<LiveSide?> GetSideAsync(CancellationToken cancellationToken = default)
    {
        var state = bridge.GetRecordAsync<BridgeLiveStateDto>(BridgeQuery.FromGame().Props(LiveMatchConstants.State), cancellationToken);
        var teams = bridge.GetRecordAsync<BridgeLiveTeamsDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props(LiveMatchConstants.Teams), cancellationToken);

        await Task.WhenAll(state, teams);

        var game = (await state).Values;
        var user = (await teams).Values;

        // As get_live_match: the Match root only exists while the game is in the user's match of the day.
        if (game.IsDuringMatch != true || game.CurrentDate is null || user.MatchDate != game.CurrentDate)
        {
            return null;
        }

        var match = await bridge.GetRecordAsync<BridgeLiveMatchDto>(
            BridgeQuery.FromMatch().WithHumanAndTeam().Props([MatchConstants.HomeTeamId]), cancellationToken);
        var isHome = match.Values.HomeTeamId == user.TeamId;

        var side = await bridge.GetRecordAsync<BridgeLiveSideDto>(
            BridgeQuery.FromMatch().WithHumanAndTeam().Context(MatchConstants.IsHomeTeam, isHome)
                .Props([LiveMatchConstants.SubstitutionsRemaining]),
            cancellationToken);

        return new LiveSide { IsHome = isHome, SubstitutionsRemaining = Number(side.Values.SubstitutionsRemaining) };
    }

    public async Task<List<MatchPlace>> GetPlacesAsync(bool isHome, CancellationToken cancellationToken = default)
    {
        return await PlacesAsync(isHome, 0, LiveControlConstants.LastPlace, cancellationToken);
    }

    public async Task<MatchPlace> SubstituteAsync(bool isHome, MatchPlace off, MatchPlace on, CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["$type"] = "Record",
            [LiveControlConstants.Drag] = DragPlace(LiveControlConstants.Drag, on),
            [LiveControlConstants.Drop] = DragPlace(LiveControlConstants.Drop, off)
        });

        await bridge.WriteAsync<JsonElement>(
            OnMatchTactic(isHome).ToWrite("/native",
                ("event", LiveControlConstants.SwapEvent), ("data", data), ("template", "none")),
            cancellationToken);

        // The place on the pitch as the tactic holds it now, once it shows the substitute.
        var now = off;

        for (var check = 0; check < SwapChecks; check++)
        {
            now = (await PlacesAsync(isHome, off.Index, off.Index, cancellationToken)).FirstOrDefault() ?? now;

            if (now.PlayerId == on.PlayerId)
            {
                break;
            }

            await Task.Delay(Poll, cancellationToken);
        }

        return now;
    }

    public async Task<int?> GetTacticPlayerAsync(int index, CancellationToken cancellationToken = default)
    {
        var place = await bridge.GetRecordAsync<BridgeTacticPlayerDto>(
            BridgeQuery.OnActiveTactic()
                .Context(TacticConstants.TeamSelectionIndex, index)
                .Context(TacticConstants.InPossession, true)
                .Props([TacticConstants.LineupPlayerId]),
            cancellationToken);

        return place.Values.PlayerId;
    }

    public async Task<List<string>> GetMentalitiesAsync(CancellationToken cancellationToken = default)
    {
        return (await vocabulary.KeysAsync(cancellationToken)).Mentalities.Values.ToList();
    }

    public async Task<List<string>> GetRolesAsync(bool inPossession, CancellationToken cancellationToken = default)
    {
        return (await vocabulary.KeysAsync(cancellationToken)).Roles(inPossession).Values.ToList();
    }

    public async Task<string?> GetMatchDayStateAsync(CancellationToken cancellationToken = default)
    {
        var state = await bridge.GetRecordAsync<BridgeMatchDayStateDto>(
            BridgeQuery.FromContinue().Props([LiveControlConstants.State]), cancellationToken);

        return state.Values.State;
    }

    public async Task<ConversationCard?> ReadTeamTalkAsync(CancellationToken cancellationToken = default)
    {
        return Panel(await PanelElementsAsync(cancellationToken));
    }

    public async Task<ConversationCard?> SayTeamTalkAsync(int line, CancellationToken cancellationToken = default)
    {
        var elements = await PanelElementsAsync(cancellationToken)
            ?? throw new GameRejectedException("The team talk panel isn't on screen.");
        var before = Panel(elements)!;
        var buttons = LineButtons(elements);

        if (line >= buttons.Count)
        {
            throw new GameRejectedException($"The panel has no line {line}.");
        }

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write(ConversationConstants.ClickEndpoint, [("path", buttons[line].Path), ("how", ConversationConstants.ClickHow)]),
            cancellationToken);

        // The squad's reactions come in after the line: wait until the panel changes, briefly.
        var now = before;

        for (var check = 0; check < AnswerChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (Panel(await PanelElementsAsync(cancellationToken)) is not { } shown)
            {
                return null;
            }

            now = shown;

            if (!now.Lines.SequenceEqual(before.Lines) || !now.Texts.SequenceEqual(before.Texts))
            {
                break;
            }
        }

        return now;
    }

    private async Task<List<MatchPlace>> PlacesAsync(bool isHome, int from, int to, CancellationToken cancellationToken)
    {
        var places = await bridge.GetListAsync<BridgeMatchPlaceDto>(
            OnMatchTactic(isHome)
                .Context(TacticConstants.InPossession, true)
                .Each(TacticConstants.TeamSelectionIndex, from, to)
                .Props([TacticConstants.PositionValue, TacticConstants.LineupPlayerId, TacticConstants.LineupPlayerName]),
            cancellationToken,
            gapsExpected: true);

        // The rows come in index order; places past the bench's last hold nobody.
        return places.Items
            .Select((item, offset) => new MatchPlace
            {
                Index = from + offset,
                PlayerId = item.Values.PlayerId,
                PlayerName = item.Values.PlayerName,
                PositionMask = item.Values.PositionMask ?? 0
            })
            .Where(place => place.PlayerId is not null)
            .ToList();
    }

    private static BridgeQuery OnMatchTactic(bool isHome)
    {
        return BridgeQuery.FromMatch().Base(LiveControlConstants.MatchTactic).WithHumanAndTeam().Context(MatchConstants.IsHomeTeam, isHome);
    }

    // One end of the drag: the place's position mask under its own name, its index, and the drag's flags.
    private static Dictionary<string, object> DragPlace(string name, MatchPlace place)
    {
        return new Dictionary<string, object>
        {
            ["$type"] = "Record",
            [name] = $"u:{place.PositionMask.ToString(CultureInfo.InvariantCulture)}",
            [LiveControlConstants.DragIsPlace] = true,
            [LiveControlConstants.DragKind] = LiveControlConstants.DragKindPlace,
            [LiveControlConstants.DragIndex] = place.Index
        };
    }

    // The team talk panel's elements, the panel first, or null when it isn't on screen.
    private async Task<List<UiElementLine>?> PanelElementsAsync(CancellationToken cancellationToken)
    {
        var elements = (await bridge.GetTextAsync(ConversationConstants.UiPath, cancellationToken))
            .Split('\n')
            .Select(UiElementLine.Parse)
            .OfType<UiElementLine>()
            .ToList();

        var panel = elements.FirstOrDefault(element => element.Type.EndsWith("#" + LiveControlConstants.TeamTalkPanel, StringComparison.Ordinal));

        return panel is null ? null : elements.Where(element => element.IsWithin(panel.Path)).ToList();
    }

    private static ConversationCard? Panel(List<UiElementLine>? elements)
    {
        if (elements is null)
        {
            return null;
        }

        var buttons = LineButtons(elements);
        var texts = elements.Where(element => !string.IsNullOrWhiteSpace(element.Text)).ToList();

        return new ConversationCard
        {
            Lines = buttons
                .Select(button => string.Join(" | ", texts
                    .Where(element => element.IsWithin(button.Path) && element.Text != LiveControlConstants.AdviceLabel)
                    .Select(element => element.Text)
                    .Distinct()))
                .ToList(),
            Texts = texts
                .Where(element => !buttons.Any(button => element.IsWithin(button.Path)))
                .Select(element => element.Text!)
                .ToList()
        };
    }

    private static List<UiElementLine> LineButtons(List<UiElementLine> elements)
    {
        return elements
            .Where(element => element.IsButton && element.Classes.Any(name => name.Contains(ConversationConstants.LineClass, StringComparison.Ordinal)))
            .ToList();
    }

    private static int? Number(JsonElement? value)
    {
        return value switch
        {
            { ValueKind: JsonValueKind.Number } number when number.TryGetDouble(out var real) => (int)real,
            { ValueKind: JsonValueKind.String } text when int.TryParse(text.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }
}
