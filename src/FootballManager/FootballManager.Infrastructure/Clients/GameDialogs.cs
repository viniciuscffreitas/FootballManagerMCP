using System.Globalization;
using System.Text;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Clients;

// The game's confirmation dialog: read from the /ui tree and answered with /click on its "Yes" button, as the user's
// click does. A dialog takes a moment to show after what opens it, and to close once answered.
internal sealed class GameDialogs(GameBridge bridge)
{
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(300);

    // The dialog showed within a second or two in the game; closing within a frame or two.
    private const int ShowChecks = 10;
    private const int CloseChecks = 10;

    /// <summary>The open dialog's title and description, or null when none is open.</summary>
    public async Task<string?> OpenDialogAsync(CancellationToken cancellationToken)
    {
        return (await ReadAsync(cancellationToken))?.Text;
    }

    /// <summary>
    /// Waits for a dialog to show and answers "Yes" to it only when its text holds every one of
    /// <paramref name="expected"/> (case, accents and line breaks aside), so that a dialog other than the action's is
    /// never answered. Never throws for the game: what it could tell is in the answer's state.
    /// </summary>
    public async Task<DialogAnswer> AnswerYesAsync(IReadOnlyList<string> expected, CancellationToken cancellationToken)
    {
        (string Text, string? Yes)? dialog = null;

        try
        {
            for (var check = 0; check < ShowChecks && dialog is null; check++)
            {
                await Task.Delay(Poll, cancellationToken);
                dialog = await ReadAsync(cancellationToken);
            }
        }
        catch (GameUnavailableException)
        {
            // Nothing was clicked; whether a dialog showed isn't known.
            return new DialogAnswer { State = DialogState.NotAnswered };
        }

        if (dialog is not { } shown)
        {
            return new DialogAnswer { State = DialogState.NotShown };
        }

        var text = Plain(shown.Text);

        if (shown.Yes is null || !expected.All(part => text.Contains(Plain(part), StringComparison.Ordinal)))
        {
            return new DialogAnswer { State = DialogState.NotAnswered, Text = shown.Text };
        }

        try
        {
            await bridge.WriteAsync<JsonElement>(
                BridgeQuery.Write(DialogConstants.ClickEndpoint, [("path", shown.Yes), ("how", DialogConstants.ClickHow)]),
                cancellationToken);
        }
        catch (GameRejectedException)
        {
            return new DialogAnswer { State = DialogState.NotAnswered, Text = shown.Text };
        }
        catch (GameUnavailableException)
        {
            return new DialogAnswer { State = DialogState.Unknown, Text = shown.Text };
        }

        try
        {
            for (var check = 0; check < CloseChecks; check++)
            {
                await Task.Delay(Poll, cancellationToken);

                if (await ReadAsync(cancellationToken) is null)
                {
                    return new DialogAnswer { State = DialogState.Answered, Text = shown.Text };
                }
            }
        }
        catch (GameUnavailableException)
        {
            // Clicked, but whether it closed isn't known.
        }

        return new DialogAnswer { State = DialogState.Unknown, Text = shown.Text };
    }

    // Lower case, without accents, every run of white space as one space.
    private static string Plain(string text)
    {
        var letters = text.ToLowerInvariant().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return string.Join(' ', new string(letters.ToArray()).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    // The shown dialog's texts ("Confirme, por favor: Enviar a numeração atual do elenco?") and its Yes button's path.
    private async Task<(string Text, string? Yes)?> ReadAsync(CancellationToken cancellationToken)
    {
        var elements = (await bridge.GetTextAsync(DialogConstants.UiPath, cancellationToken))
            .Split('\n')
            .Select(UiElementLine.Parse)
            .OfType<UiElementLine>()
            .ToList();

        var panel = elements.FirstOrDefault(element =>
            element.Type == DialogConstants.Panel && element.IsShown && element.Classes.Contains(DialogConstants.ShownPanel));

        if (panel is null)
        {
            return null;
        }

        var inside = elements.Where(element => element.IsWithin(panel.Path) && element.IsShown).ToList();
        var title = inside.FirstOrDefault(element => element.Type == DialogConstants.Title)?.Text;
        var description = inside.FirstOrDefault(element => element.Type == DialogConstants.Description)?.Text;
        var yes = inside.FirstOrDefault(element => element.Type == DialogConstants.YesButton)?.Path;
        var text = string.Join(": ", new[] { title, description }.Where(part => !string.IsNullOrWhiteSpace(part)));

        return (text, yes);
    }
}
