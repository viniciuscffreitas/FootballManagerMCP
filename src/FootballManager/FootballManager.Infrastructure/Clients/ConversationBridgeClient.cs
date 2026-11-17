using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Conversations;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Conversations;
using FootballManager.Infrastructure.Dtos.Scouting;

namespace FootballManager.Infrastructure.Clients;

// Conversations through the MCPFM plugin: the player's menu offers them in game.Actions; /contextaction opens one as a
// click on the menu does; the card it opens is read from the /ui tree and its buttons pressed with /click. The card
// takes a moment to show and to answer a line, so each step waits for the card to change.
internal sealed class ConversationBridgeClient(GameBridge bridge) : IConversationGameClient
{
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(500);

    // The card showed within a few seconds in the game; an answer within one or two.
    private const int OpenChecks = 20;
    private const int AnswerChecks = 12;
    private const int CloseChecks = 10;

    public async Task<ConversationPlayer?> GetPlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var squad = await bridge.GetListAsync<BridgeConversationPlayerDto>(
            BridgeQuery.FromHuman().List(ConversationConstants.Squad).Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken);

        if (squad.Items.Select(item => item.Values).FirstOrDefault(player => player.Id == playerId) is not { } found)
        {
            return null;
        }

        var offered = await bridge.GetRecordAsync<BridgeActionsDto>(
            BridgeQuery.FromGame()
                .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, PlayerPath(playerId))
                .WithHumanAndTeam()
                .Props([ScoutingConstants.Actions]),
            cancellationToken);

        var keys = offered.Values.Actions?.Keys.ToHashSet() ?? [];

        return new ConversationPlayer
        {
            Id = playerId,
            Name = found.Name ?? string.Empty,
            Available = Enum.GetValues<PlayerConversation>()
                .Where(conversation => keys.Contains(ActionKey(ConversationConstants.ActionIdOf(conversation))))
                .ToList()
        };
    }

    public async Task<ConversationCard?> ReadCardAsync(CancellationToken cancellationToken = default)
    {
        return Card(await CardElementsAsync(cancellationToken));
    }

    public async Task<ConversationCard?> OpenAsync(int playerId, PlayerConversation conversation, CancellationToken cancellationToken = default)
    {
        var actionId = ConversationConstants.ActionIdOf(conversation);

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.FromGame()
                .Base(ScoutingConstants.Action)
                .Context(ScoutingConstants.ActionId, actionId)
                .RefList(ScoutingConstants.Records, GameConstants.HumanRoot, PlayerPath(playerId))
                .WithHumanAndTeam()
                .ToWrite(ConversationConstants.OpenEndpoint,
                    ("action", actionId.ToString(CultureInfo.InvariantCulture)), ("field", ConversationConstants.EntryData)),
            cancellationToken);

        for (var check = 0; check < OpenChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (await ReadCardAsync(cancellationToken) is { } card)
            {
                return card;
            }
        }

        return null;
    }

    public async Task<ConversationCard?> SayAsync(int line, CancellationToken cancellationToken = default)
    {
        var elements = await CardElementsAsync(cancellationToken)
            ?? throw new GameRejectedException("No conversation card is open.");
        var before = Card(elements)!;
        var buttons = LineButtons(elements);

        if (line >= buttons.Count)
        {
            throw new GameRejectedException($"The card has no line {line}.");
        }

        await ClickAsync(buttons[line].Path, cancellationToken);

        // The answer comes in after the line, sometimes in steps: wait until the card changes, then until it holds still.
        ConversationCard? last = before;

        for (var check = 0; check < AnswerChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);
            var now = await ReadCardAsync(cancellationToken);

            if (now is null)
            {
                return null;
            }

            if (!Same(now, before) && Same(now, last))
            {
                return now;
            }

            last = now;
        }

        return last;
    }

    public async Task<bool> EndAsync(CancellationToken cancellationToken = default)
    {
        if (await CardElementsAsync(cancellationToken) is not { } elements)
        {
            return true;
        }

        // "End conversation" is a button around its text; failing that, the corner X.
        var end = elements.FirstOrDefault(element => element.Text == ConversationConstants.EndConversation) is { } label
            ? elements.Where(element => element.IsButton && label.IsWithin(element.Path)).MaxBy(element => element.Path.Length)
            : null;
        end ??= elements.FirstOrDefault(element => element.Type.StartsWith(ConversationConstants.CloseButton, StringComparison.Ordinal));

        if (end is null)
        {
            return false;
        }

        await ClickAsync(end.Path, cancellationToken);

        for (var check = 0; check < CloseChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (await CardElementsAsync(cancellationToken) is null)
            {
                return true;
            }
        }

        return false;
    }

    // The card's elements, the card first, or null when no card is open.
    private async Task<List<UiElementLine>?> CardElementsAsync(CancellationToken cancellationToken)
    {
        var elements = (await bridge.GetTextAsync(ConversationConstants.UiPath, cancellationToken))
            .Split('\n')
            .Select(UiElementLine.Parse)
            .OfType<UiElementLine>()
            .ToList();

        var card = elements.FirstOrDefault(element =>
            element.Type.Contains(ConversationConstants.Card, StringComparison.Ordinal)
            || element.Classes.Any(name => name.Contains(ConversationConstants.Card, StringComparison.Ordinal)));

        return card is null ? null : elements.Where(element => element.IsWithin(card.Path)).ToList();
    }

    private static ConversationCard? Card(List<UiElementLine>? elements)
    {
        if (elements is null)
        {
            return null;
        }

        var buttons = LineButtons(elements);
        var texts = elements.Where(element => !string.IsNullOrWhiteSpace(element.Text)).ToList();
        var lineTexts = texts.Where(element => buttons.Any(button => element.IsWithin(button.Path))).ToList();
        var label = texts.FindIndex(element => element.Text == ConversationConstants.BodyLanguageLabel);
        var bodyLanguage = label >= 0 && label + 1 < texts.Count ? texts[label + 1] : null;

        return new ConversationCard
        {
            // A line's button may hold more than one text (its tone, say): all of them, in order.
            Lines = buttons
                .Select(button => string.Join(" | ", texts.Where(element => element.IsWithin(button.Path)).Select(element => element.Text).Distinct()))
                .ToList(),
            BodyLanguage = bodyLanguage?.Text,
            Texts = texts
                .Where(element => !lineTexts.Contains(element) && element != bodyLanguage && element.Text != ConversationConstants.BodyLanguageLabel)
                .Select(element => element.Text!)
                .Distinct()
                .ToList()
        };
    }

    private static List<UiElementLine> LineButtons(List<UiElementLine> elements)
    {
        return elements.Where(element => element.Classes.Contains(ConversationConstants.LineClass)).ToList();
    }

    private static bool Same(ConversationCard one, ConversationCard other)
    {
        return one.Lines.SequenceEqual(other.Lines) && one.Texts.SequenceEqual(other.Texts) && one.BodyLanguage == other.BodyLanguage;
    }

    private Task<JsonElement> ClickAsync(string path, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write(ConversationConstants.ClickEndpoint, [("path", path), ("how", ConversationConstants.ClickHow)]),
            cancellationToken);
    }

    private static string PlayerPath(int playerId)
    {
        return $"{ConversationConstants.Squad}[{GameConstants.UniqueId}={playerId.ToString(CultureInfo.InvariantCulture)}]";
    }

    private static string ActionKey(uint actionId)
    {
        return $"#{actionId.ToString(CultureInfo.InvariantCulture)}";
    }
}
