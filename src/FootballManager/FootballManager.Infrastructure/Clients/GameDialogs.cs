using System.Text.Json;
using FootballManager.Application.Exceptions;
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
    /// Waits for a dialog to show and answers "Yes" to it; its text, or null when none showed. Throws
    /// <see cref="GameRejectedException"/> when it stays open after the answer.
    /// </summary>
    public async Task<string?> AnswerYesAsync(CancellationToken cancellationToken)
    {
        (string Text, string? Yes)? dialog = null;

        for (var check = 0; check < ShowChecks && dialog is null; check++)
        {
            await Task.Delay(Poll, cancellationToken);
            dialog = await ReadAsync(cancellationToken);
        }

        if (dialog is not { } shown)
        {
            return null;
        }

        if (shown.Yes is null)
        {
            throw new GameRejectedException($"The game's dialog has no \"Yes\" button: {shown.Text}");
        }

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write(DialogConstants.ClickEndpoint, [("path", shown.Yes), ("how", DialogConstants.ClickHow)]),
            cancellationToken);

        for (var check = 0; check < CloseChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (await ReadAsync(cancellationToken) is null)
            {
                return shown.Text;
            }
        }

        throw new GameRejectedException($"The game's dialog stayed open after \"Yes\": {shown.Text}");
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
