using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Conversations;
using FootballManager.Domain.Saves;

namespace FootballManager.Application.Services;

public interface IConversationService
{
    /// <summary>The first-team player and the conversations his menu offers now.</summary>
    Task<Result<ConversationPlayer>> GetConversationsAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the conversation from the player's menu and says the lines at <paramref name="choices"/>, one per round,
    /// then ends it. With no choice left while the card still offers lines, the card stays open and the call returns
    /// them; <paramref name="continueOpen"/> then says the next choices on that card. Only with <paramref name="apply"/>.
    /// </summary>
    Task<Result<PlayerTalk>> TalkAsync(
        int playerId, PlayerConversation? conversation, IReadOnlyList<int> choices, bool continueOpen, bool apply,
        CancellationToken cancellationToken = default);
}

// A conversation is a card the player's menu opens: each round offers a few lines, the player answers the one said,
// and a criticism has a second round (back down, convince, threaten). The game takes the conversation off the menu as
// soon as the card opens, answered or not, so a dry run opens nothing and an apply goes through the save guard first.
public class ConversationService(IConversationGameClient game, ISaveGuard saveGuard) : IConversationService
{
    // A praise has one round and a criticism two; more choices than this are a mistake.
    public const int MaxChoices = 6;

    public async Task<Result<ConversationPlayer>> GetConversationsAsync(int playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await game.GetPlayerAsync(playerId, cancellationToken) is { } player
                ? Result.Ok(player)
                : Result.Fail<ConversationPlayer>(new EntityDoesNotExistError());
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ConversationPlayer>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<PlayerTalk>> TalkAsync(
        int playerId, PlayerConversation? conversation, IReadOnlyList<int> choices, bool continueOpen, bool apply,
        CancellationToken cancellationToken = default)
    {
        var errors = Check(conversation, choices, continueOpen);

        if (errors.Count > 0)
        {
            return Result.Fail<PlayerTalk>(errors);
        }

        try
        {
            if (await game.GetPlayerAsync(playerId, cancellationToken) is not { } player)
            {
                return Result.Fail<PlayerTalk>(new EntityDoesNotExistError());
            }

            var talk = new PlayerTalk
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                Conversation = continueOpen ? null : conversation,
                Applied = apply
            };

            return continueOpen
                ? await ContinueAsync(talk, choices, apply, cancellationToken)
                : await OpenAsync(talk, player, conversation!.Value, choices, apply, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<PlayerTalk>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<PlayerTalk>(new ValidationError("game", [exception.Message]));
        }
    }

    private static List<IError> Check(PlayerConversation? conversation, IReadOnlyList<int> choices, bool continueOpen)
    {
        var errors = new List<IError>();

        if (!continueOpen && conversation is null)
        {
            errors.Add(new ValidationError("conversation",
                [$"Name the conversation to open: one of {string.Join(", ", Enum.GetNames<PlayerConversation>())}."]));
        }

        if (choices.Any(choice => choice < 0))
        {
            errors.Add(new ValidationError("choices", ["A choice is a line's index in its round, from 0."]));
        }

        if (choices.Count > MaxChoices)
        {
            errors.Add(new ValidationError("choices", [$"At most {MaxChoices} choices, one per round."]));
        }

        return errors;
    }

    private async Task<Result<PlayerTalk>> OpenAsync(
        PlayerTalk talk, ConversationPlayer player, PlayerConversation conversation, IReadOnlyList<int> choices, bool apply,
        CancellationToken cancellationToken)
    {
        if (!player.Available.Contains(conversation))
        {
            var offered = player.Available.Count > 0 ? string.Join(", ", player.Available) : "none";
            return Result.Fail<PlayerTalk>(new ValidationError("conversation",
                [$"The game doesn't offer {conversation} for {player.Name} now (offered: {offered}). A conversation " +
                 "leaves his menu once used, even when its card was closed without an answer."]));
        }

        if (!apply)
        {
            talk.Notes.Add($"Applying opens {conversation} with {player.Name} from his menu. Opening it uses it up: " +
                           "closing it without an answer still takes it off his menu, so only apply once the user agreed. " +
                           (choices.Count > 0
                               ? $"The lines at {string.Join(", ", choices)} would be said, one per round, and the conversation ended."
                               : "Without choices the card stays open and its first lines come back, to choose in a next call with continue."));
            return Result.Ok(talk);
        }

        if (await game.ReadCardAsync(cancellationToken) is not null)
        {
            return Result.Fail<PlayerTalk>(new ValidationError("continue",
                ["A conversation card is already open in the game: say its lines with continue=true (a dry run with " +
                 "continue reads them) before opening another."]));
        }

        if (await PrepareAsync(talk, cancellationToken) is { } refused)
        {
            return refused;
        }

        if (await game.OpenAsync(player.Id, conversation, cancellationToken) is not { } card)
        {
            talk.Notes.Add("The card didn't show. The game may have taken the conversation off his menu anyway: " +
                           "check get_player_conversations, and continue=true in case the card shows late.");
            return Result.Ok(talk);
        }

        return Result.Ok(await PlayAsync(talk, card, choices, cancellationToken));
    }

    private async Task<Result<PlayerTalk>> ContinueAsync(
        PlayerTalk talk, IReadOnlyList<int> choices, bool apply, CancellationToken cancellationToken)
    {
        if (await game.ReadCardAsync(cancellationToken) is not { } card)
        {
            return Result.Fail<PlayerTalk>(new ValidationError("continue",
                ["No conversation card is open in the game: open one with a conversation and continue=false."]));
        }

        if (!apply)
        {
            talk.Lines = card.Lines;
            talk.BodyLanguage = card.BodyLanguage;
            return Result.Ok(talk);
        }

        if (await PrepareAsync(talk, cancellationToken) is { } refused)
        {
            return refused;
        }

        return Result.Ok(await PlayAsync(talk, card, choices, cancellationToken));
    }

    // The save guard's clearance as a note, or its refusal; nothing is written after a refusal.
    private async Task<Result<PlayerTalk>?> PrepareAsync(PlayerTalk talk, CancellationToken cancellationToken)
    {
        var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

        if (clearance.IsFailed)
        {
            return Result.Fail<PlayerTalk>(clearance.Errors);
        }

        talk.Notes.Add(SaveNote(clearance.Value));
        return null;
    }

    // Says one choice per round while the card offers lines. Out of choices, or given a line the round doesn't have, it
    // leaves the card open with the round's lines for the next call; once no lines are left it ends the conversation.
    private async Task<PlayerTalk> PlayAsync(
        PlayerTalk talk, ConversationCard card, IReadOnlyList<int> choices, CancellationToken cancellationToken)
    {
        var next = 0;

        try
        {
            while (card.Lines.Count > 0)
            {
                if (next == choices.Count || choices[next] >= card.Lines.Count)
                {
                    if (next < choices.Count)
                    {
                        talk.Notes.Add($"This round has no line {choices[next]} (0 to {card.Lines.Count - 1}): nothing was said.");
                    }

                    talk.Lines = card.Lines;
                    talk.BodyLanguage = card.BodyLanguage;
                    talk.Notes.Add("The card is open and waits for a line: call again with continue=true, apply=true " +
                                   "and the choice(s), one per round.");
                    return talk;
                }

                var chosen = choices[next++];
                var after = await game.SayAsync(chosen, cancellationToken);

                talk.Rounds.Add(new TalkRound
                {
                    Lines = card.Lines,
                    Chosen = chosen,
                    ChosenLine = card.Lines[chosen],
                    Reaction = after?.Texts.Except(card.Texts).ToList() ?? [],
                    BodyLanguage = after?.BodyLanguage
                });

                if (after is null)
                {
                    talk.Notes.Add("The card closed after that line.");
                    talk.Ended = true;
                    return talk;
                }

                card = after;
            }

            talk.BodyLanguage = card.BodyLanguage;

            if (next < choices.Count)
            {
                talk.Notes.Add($"{choices.Count - next} choice(s) left unused: the conversation had no more rounds.");
            }

            talk.Ended = await game.EndAsync(cancellationToken);

            if (!talk.Ended)
            {
                talk.Notes.Add("The card didn't close: it may still be open in the game.");
            }
        }
        catch (Exception exception) when (exception is GameRejectedException or GameUnavailableException)
        {
            talk.Notes.Add($"The game stopped partway ({exception.Message}). The rounds listed were said; the card " +
                           "may still be open: a dry run with continue=true reads it.");
        }

        return talk;
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
