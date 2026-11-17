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

    // The panel showed within a few seconds in the game; a press within a frame or two, the agent's answer to a
    // negotiation's step within a few seconds.
    private const int OpenChecks = 30;
    private const int PressChecks = 10;
    private const int AnswerChecks = 20;

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

    // The panel shows before its fee is bound: the fee counts once two reads in a row agree (and agree with what the
    // combobox shows, see OfferPanel).
    public async Task<OfferPanel?> OpenOfferPanelAsync(int playerId, CancellationToken cancellationToken = default)
    {
        await players.OpenPanelAsync(playerId, PlayerMenuAction.MakeOffer, TransferConstants.OfferPanelAction, cancellationToken);

        long? previous = null;

        for (var check = 0; check < OpenChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            var panel = await ReadOfferPanelAsync(cancellationToken);

            if (panel is { Fee: { } fee } && fee == previous)
            {
                return panel;
            }

            previous = panel?.Fee;
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

    public async Task<ContractPanel?> ReadContractPanelAsync(CancellationToken cancellationToken = default)
    {
        return ContractPanel(await PanelElementsAsync(TransferConstants.ContractPanel, cancellationToken));
    }

    // As the message's button does: the panel event with the references the button carries, on the message.
    public async Task<ContractPanel?> OpenContractPanelAsync(int messageId, int option, CancellationToken cancellationToken = default)
    {
        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.FromHuman()
                .Base(string.Format(CultureInfo.InvariantCulture, InboxConstants.MessageById, messageId))
                .Context(InboxConstants.Tab, InboxConstants.AllMessagesTab)
                .Context(InboxConstants.SearchText, string.Empty)
                .WithHumanAndTeam()
                .ToWrite("/newsaction",
                    ("action", TransferConstants.ContractPanelAction.ToString(CultureInfo.InvariantCulture)),
                    ("field", $"References:@{InboxConstants.Actions}.ActionsForButtonsList[{option.ToString(CultureInfo.InvariantCulture)}].ActionDataReferences")),
            cancellationToken);

        for (var check = 0; check < OpenChecks; check++)
        {
            await Task.Delay(Poll, cancellationToken);

            if (await ReadContractPanelAsync(cancellationToken) is { } panel)
            {
                return panel;
            }
        }

        return null;
    }

    // The panel's own button for the step: the game's Continue button can carry the same text ("Negociar Contrato") but
    // lies outside the panel.
    public async Task<ContractPanel?> PressAsync(ContractStep step, CancellationToken cancellationToken = default)
    {
        var elements = await PanelElementsAsync(TransferConstants.ContractPanel, cancellationToken)
            ?? throw new GameRejectedException("No contract negotiation panel is open.");
        var text = TextOf(step);
        var button = PanelButton(elements, text)
                     ?? throw new GameRejectedException($"The contract panel has no '{text}' button now.");
        var before = ContractPanel(elements)!;

        await ClickAsync(button.Path, cancellationToken);

        ContractPanel? now = before;

        for (var check = 0; check < AnswerChecks && now is not null && now.Texts.SequenceEqual(before.Texts); check++)
        {
            await Task.Delay(Poll, cancellationToken);
            now = await ReadContractPanelAsync(cancellationToken);
        }

        return now;
    }

    private static ContractPanel? ContractPanel(List<UiElementLine>? elements)
    {
        if (elements is null)
        {
            return null;
        }

        var buttons = elements
            .Where(element => element.Type == TransferConstants.PanelButton && element.IsShown)
            .Select(button => string.Join(" ", elements.Where(element => element.IsWithin(button.Path) && element.IsShown && element.Text is not null)
                .Select(element => element.Text).Distinct()))
            .Where(text => text.Length > 0)
            .ToList();

        return new ContractPanel
        {
            Club = TextOf(elements, TransferConstants.ContractClub),
            Player = TextOf(elements, TransferConstants.ContractPlayer),
            Terms = Terms(elements),
            Steps = Enum.GetValues<ContractStep>().Where(step => buttons.Contains(TextOf(step))).ToList(),
            Buttons = buttons,
            Lines = SpeakerLines(elements),
            Texts = Texts(elements)
        };
    }

    // Each line's label and the texts of its row but the label: "R$65.000 p/s | 65000".
    private static List<NegotiationTerm> Terms(List<UiElementLine> elements)
    {
        var terms = new List<NegotiationTerm>();

        foreach (var label in elements.Where(element => element.Type == TransferConstants.TermLabel
                                                       && element.Classes.Contains(TransferConstants.TermLabelClass)
                                                       && element.IsShown
                                                       && !string.IsNullOrWhiteSpace(element.Text)))
        {
            var segments = label.Path.Split('/');
            var row = string.Join('/', segments.Take(Math.Max(1, segments.Length - TransferConstants.TermRowLevels)));
            var values = elements
                .Where(element => element.IsWithin(row) && element != label && !string.IsNullOrWhiteSpace(element.Text))
                .Select(element => element.Text!)
                .Distinct()
                .ToList();

            terms.Add(new NegotiationTerm { Label = label.Text!, Value = values.Count > 0 ? string.Join(" | ", values) : null });
        }

        return terms;
    }

    private static string TextOf(ContractStep step)
    {
        return step switch
        {
            ContractStep.FinalisePromises => TransferConstants.FinalisePromises,
            ContractStep.NegotiateContract => TransferConstants.NegotiateContract,
            ContractStep.CloseDeal => TransferConstants.CloseDeal,
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };
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

        return new OfferPanel
        {
            Club = TextOf(elements, TransferConstants.OfferClub),
            Player = TextOf(elements, TransferConstants.OfferPlayer),
            Fee = Fee(elements),
            Advice = SpeakerLines(elements),
            Texts = Texts(elements)
        };
    }

    // The offer's fee: the panel's first combobox.
    private static UiElementLine? FeeBox(List<UiElementLine> elements)
    {
        return elements.FirstOrDefault(element => element.Type.StartsWith(TransferConstants.Combobox, StringComparison.Ordinal));
    }

    // The number in the fee combobox's own field ("1000000"), only while it is what the combobox shows ("R$1 MI"):
    // while the panel binds, the field can still hold another number than the one on screen.
    private static long? Fee(List<UiElementLine> elements)
    {
        if (FeeBox(elements) is not { } box
            || elements.FirstOrDefault(element => element.IsWithin(box.Path) && element.Type == TransferConstants.FeeField) is not { } field
            || elements.FirstOrDefault(element => element.IsWithin(field.Path) && element.Type == TransferConstants.FeeText) is not { } text
            || !long.TryParse(text.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var fee))
        {
            return null;
        }

        var preview = elements.FirstOrDefault(element => element.IsWithin(box.Path)
                                                         && element.Classes.Contains(TransferConstants.FeePreview)
                                                         && element.Text is not null);

        return preview is not null && Shows(preview.Text!, fee) ? fee : null;
    }

    // Whether a short amount as the game writes it ("R$1 MI", "R$1.05 MI", "R$990 mil") is the fee, to its rounding.
    // One it doesn't write that way (another currency's format) can't be checked, and passes.
    private static bool Shows(string preview, long fee)
    {
        var match = TransferConstants.ShortAmount.Match(preview);

        if (!match.Success
            || !decimal.TryParse(match.Groups["number"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
        {
            return true;
        }

        var unit = match.Groups["unit"].Value == TransferConstants.Millions ? 1_000_000m : 1_000m;
        var decimals = match.Groups["number"].Value.Split('.').ElementAtOrDefault(1)?.Length ?? 0;
        var halfStep = unit / (decimal)Math.Pow(10, decimals) / 2;

        return Math.Abs(number * unit - fee) <= halfStep;
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
