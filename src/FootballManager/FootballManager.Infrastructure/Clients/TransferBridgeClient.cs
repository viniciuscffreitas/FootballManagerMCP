using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Transfers;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Clients;

// Transfers through the MCPFM plugin: the player is found, and his menu read and run, as the scouting tools do; the
// panel it opens is read from the /ui tree and its buttons pressed with /click. The panel takes a moment to show and
// to take a press, so each step waits for it to change.
internal sealed class TransferBridgeClient(GameBridge bridge, IScoutingEditorGameClient players) : ITransferGameClient
{
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(300);

    // The panel showed within a few seconds in the game; a press within a frame or two.
    private const int OpenChecks = 30;
    private const int PressChecks = 10;

    public async Task<TransferTarget?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        if ((await players.FindPlayersAsync([playerId], cancellationToken)).GetValueOrDefault(playerId) is not { } player)
        {
            return null;
        }

        var offered = await players.GetOfferedActionsAsync(playerId, cancellationToken);

        return new TransferTarget
        {
            Id = player.Id,
            Name = player.Name,
            Club = player.Club,
            CanMakeOffer = offered.Contains(PlayerMenuAction.MakeOffer)
        };
    }

    private async Task<OfferPanel?> ReadOfferPanelAsync(CancellationToken cancellationToken)
    {
        return OfferPanel(await PanelElementsAsync(TransferConstants.OfferPanel, cancellationToken));
    }

    public async Task<OfferPanel?> OpenOfferPanelAsync(int playerId, CancellationToken cancellationToken = default)
    {
        await players.OpenPanelAsync(playerId, PlayerMenuAction.MakeOffer, TransferConstants.OfferPanelAction, cancellationToken);

        for (var check = 0; check < OpenChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (await ReadOfferPanelAsync(cancellationToken) is { Fee: not null } panel)
            {
                return panel;
            }
        }

        return null;
    }

    public async Task<OfferPanel?> StepFeeAsync(bool raise, CancellationToken cancellationToken = default)
    {
        var elements = await PanelElementsAsync(TransferConstants.OfferPanel, cancellationToken)
            ?? throw new GameRejectedException("No transfer offer panel is open.");
        var before = OfferPanel(elements)!;
        var fee = FeeBox(elements) ?? throw new GameRejectedException("The offer panel shows no fee.");
        var button = elements.FirstOrDefault(element =>
                         element.IsWithin(fee.Path)
                         && element.Classes.Contains(raise ? TransferConstants.RaiseButton : TransferConstants.LowerButton))
                     ?? throw new GameRejectedException($"The offer panel's fee has no {(raise ? "+" : "−")} button.");

        await ClickAsync(button.Path, cancellationToken);

        for (var check = 0; check < PressChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (await ReadOfferPanelAsync(cancellationToken) is not { } now)
            {
                return null;
            }

            if (now.Fee != before.Fee)
            {
                return now;
            }
        }

        return before;
    }

    public async Task<OfferPanel?> SendOfferAsync(CancellationToken cancellationToken = default)
    {
        var elements = await PanelElementsAsync(TransferConstants.OfferPanel, cancellationToken)
            ?? throw new GameRejectedException("No transfer offer panel is open.");
        var send = PanelButton(elements, TransferConstants.MakeOffer)
                   ?? throw new GameRejectedException($"The offer panel has no '{TransferConstants.MakeOffer}' button.");
        var before = OfferPanel(elements)!;

        await ClickAsync(send.Path, cancellationToken);

        // The answer shows in the panel a moment after: wait until it changes, or closes.
        OfferPanel? now = before;

        for (var check = 0; check < PressChecks && now is not null && now.Texts.SequenceEqual(before.Texts); check++)
        {
            await Task.Delay(Poll, cancellationToken);
            now = await ReadOfferPanelAsync(cancellationToken);
        }

        return now;
    }

    // The panel's elements on screen, the panel first, or null when it isn't open.
    private async Task<List<UiElementLine>?> PanelElementsAsync(string panel, CancellationToken cancellationToken)
    {
        var elements = (await bridge.GetTextAsync(TransferConstants.UiPath, cancellationToken))
            .Split('\n')
            .Select(UiElementLine.Parse)
            .OfType<UiElementLine>()
            .ToList();

        var root = elements.FirstOrDefault(element =>
            element.Type == panel && element.IsShown && element.Classes.Contains(TransferConstants.ShownPanel));

        // The fee's field lies under the preview the panel shows: keep it, with what's on screen.
        return root is null
            ? null
            : elements.Where(element => element.IsWithin(root.Path) && (element.IsShown || element.Type == TransferConstants.FeeText)).ToList();
    }

    private static OfferPanel? OfferPanel(List<UiElementLine>? elements)
    {
        if (elements is null)
        {
            return null;
        }

        var fee = FeeBox(elements) is { } box
            ? elements.Where(element => element.IsWithin(box.Path) && element.Type == TransferConstants.FeeText)
                .Select(element => long.TryParse(element.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : (long?)null)
                .FirstOrDefault(value => value is not null)
            : null;

        return new OfferPanel
        {
            Club = TextOf(elements, TransferConstants.OfferClub),
            Player = TextOf(elements, TransferConstants.OfferPlayer),
            Fee = fee,
            Advice = SpeakerLines(elements),
            Texts = Texts(elements)
        };
    }

    // The offer's fee: the panel's first combobox.
    private static UiElementLine? FeeBox(List<UiElementLine> elements)
    {
        return elements.FirstOrDefault(element => element.Type.StartsWith(TransferConstants.Combobox, StringComparison.Ordinal));
    }

    // What the staff member beside the panel says: his column's texts, but for his name, his job and the buttons.
    private static List<string> SpeakerLines(List<UiElementLine> elements)
    {
        if (elements.FirstOrDefault(element => element.Type == TransferConstants.SpeakerColumn) is not { } column)
        {
            return [];
        }

        var buttons = elements.Where(element => element.Type == TransferConstants.PanelButton).ToList();

        return elements
            .Where(element => element.IsWithin(column.Path)
                              && element.IsShown
                              && !string.IsNullOrWhiteSpace(element.Text)
                              && element.Type != TransferConstants.SpeakerName
                              && element.Type != TransferConstants.SpeakerJob
                              && !buttons.Any(button => element.IsWithin(button.Path)))
            .Select(element => element.Text!)
            .Distinct()
            .ToList();
    }

    // One of the panel's own buttons by its text, e.g. "Fazer Proposta".
    private static UiElementLine? PanelButton(List<UiElementLine> elements, string text)
    {
        return elements
            .Where(element => element.Type == TransferConstants.PanelButton && element.IsShown)
            .FirstOrDefault(button => elements.Any(element => element.IsWithin(button.Path) && element.Text == text));
    }

    private static string? TextOf(List<UiElementLine> elements, string type)
    {
        return elements.FirstOrDefault(element => element.Type == type && element.IsShown)?.Text;
    }

    private static List<string> Texts(List<UiElementLine> elements)
    {
        return elements
            .Where(element => element.IsShown && !string.IsNullOrWhiteSpace(element.Text))
            .Select(element => element.Text!)
            .Distinct()
            .ToList();
    }

    private Task<JsonElement> ClickAsync(string path, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write(TransferConstants.ClickEndpoint, [("path", path), ("how", TransferConstants.ClickHow)]),
            cancellationToken);
    }
}
