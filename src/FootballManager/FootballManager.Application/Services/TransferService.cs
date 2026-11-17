using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Saves;
using FootballManager.Domain.Transfers;

namespace FootballManager.Application.Services;

public interface ITransferService
{
    /// <summary>
    /// Bids <paramref name="fee"/> for the player through the game's transfer offer panel: opens it from his menu,
    /// moves its fee with the +/− buttons to exactly that, and sends it. Only when <paramref name="apply"/> is set,
    /// through the save guard; nothing is sent unless the panel shows exactly the fee asked.
    /// </summary>
    Task<Result<TransferOffer>> MakeOfferAsync(int playerId, long fee, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Takes the player's contract negotiation, once his club accepted the user's offer, through the game's contract
    /// panel: opens it from the acceptance's message, agrees to the agent's promises and goes on to the contract,
    /// stopping there to show its terms; with the panel already on the contract's terms, offers the deal at them. Only
    /// when <paramref name="apply"/> is set, through the save guard.
    /// </summary>
    Task<Result<ContractNegotiation>> NegotiateContractAsync(int playerId, bool apply, CancellationToken cancellationToken = default);
}

// An offer goes through the panel the player's menu opens (as the user's click does): it starts at a fee of the
// game's choosing, its +/− buttons move the fee a step each (R$10 mil in the game), and "make offer" sends it to the
// club with the panel's other terms as they stand (no instalments or clauses unless added by hand). The club answers
// in the inbox later; a counter-offer is a message answered with its plain replies. Once it accepts, its message opens
// the contract negotiation panel (NewsAction 7): the agent's promises first, then the contract's terms (salary,
// length, bonuses), each page left with the panel's own buttons. A dry run opens and presses nothing.
public class TransferService(ITransferGameClient game, IInboxGameClient inbox, ISaveGuard saveGuard) : ITransferService
{
    /// <summary>The NewsAction of the button that opens the contract negotiation panel.</summary>
    public const int ContractPanelAction = 7;

    // The acceptance is looked for among the inbox's newest messages.
    private const int MessagesScanned = 50;

    // The negotiation's pages: promises, contract; a step more than that means the game didn't move on.
    private const int MaxContractSteps = 3;

    // At about a second a press, this keeps a call well under the MCP server's timeout: R$1 mi from where the panel
    // starts, at R$10 mil a press.
    public const int MaxFeeSteps = 100;

    public async Task<Result<TransferOffer>> MakeOfferAsync(int playerId, long fee, bool apply, CancellationToken cancellationToken = default)
    {
        if (fee <= 0)
        {
            return Refuse<TransferOffer>("fee", "The fee is the amount offered, in the game's currency units, above 0.");
        }

        try
        {
            if (await game.FindPlayerAsync(playerId, cancellationToken) is not { } player)
            {
                return Result.Fail<TransferOffer>(new EntityDoesNotExistError());
            }

            if (!player.CanMakeOffer)
            {
                return Refuse<TransferOffer>("playerId",
                    $"The game doesn't offer a transfer offer for {player.Name} now ({player.Club ?? "no club"}): he may " +
                    "be one of the club's own players, or an offer for him may be waiting for an answer.");
            }

            var offer = new TransferOffer { PlayerId = player.Id, PlayerName = player.Name, Club = player.Club, Fee = fee, Applied = apply };

            if (!apply)
            {
                offer.Notes.Add($"Applying opens the transfer offer panel for {player.Name}, moves its fee with the +/− buttons " +
                                $"from the value the panel starts at to {Money(fee)}, and sends it only if the panel shows " +
                                "exactly that. The other terms stay as the panel starts them. The starting fee and the " +
                                "director of football's advice come back with it.");
                return Result.Ok(offer);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<TransferOffer>(clearance.Errors);
            }

            offer.Notes.Add(SaveNote(clearance.Value));

            return await SendAsync(offer, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TransferOffer>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Refuse<TransferOffer>("game", exception.Message);
        }
    }

    private async Task<Result<TransferOffer>> SendAsync(TransferOffer offer, CancellationToken cancellationToken)
    {
        if (await game.OpenOfferPanelAsync(offer.PlayerId, cancellationToken) is not { } panel)
        {
            offer.Notes.Add("The transfer offer panel didn't show: nothing was sent.");
            return Result.Ok(offer);
        }

        if (!IsFor(panel.Player, offer.PlayerName))
        {
            return Refuse<TransferOffer>("playerId",
                $"The offer panel opened for {panel.Player} ({panel.Club}), not {offer.PlayerName}: nothing was sent.");
        }

        offer.StartingFee = panel.Fee;
        offer.Advice = panel.Advice;

        var reached = await MoveFeeAsync(panel, offer.Fee, cancellationToken);

        if (reached.IsFailed)
        {
            return Result.Fail<TransferOffer>(reached.Errors);
        }

        var before = reached.Value.Texts;
        var after = await game.SendOfferAsync(cancellationToken);

        offer.Sent = true;
        offer.Response = after?.Texts.Except(before).ToList() ?? [];
        offer.Notes.Add("The club answers in the inbox, usually within a day or two of the game's calendar: get_inbox. A " +
                        "counter-offer is a message whose answers (accept, reject) go with get_message_options and " +
                        "reply_to_message; once the club accepts, negotiate_contract takes it on.");

        return Result.Ok(offer);
    }

    // Presses + or − until the panel's fee is the one asked. Refuses, with the panel left as it is, when the game stops
    // moving it, when a press goes past it, or when the presses it would take don't fit in one call.
    private async Task<Result<OfferPanel>> MoveFeeAsync(OfferPanel panel, long fee, CancellationToken cancellationToken)
    {
        if (panel.Fee is not { } start)
        {
            return Refuse<OfferPanel>("game", "The offer panel's fee couldn't be read: nothing was sent.");
        }

        var current = start;

        for (var press = 0; current != fee; press++)
        {
            var raise = current < fee;

            if (press == MaxFeeSteps
                || await game.StepFeeAsync(raise, cancellationToken) is not { Fee: { } moved } after
                || moved == current)
            {
                return Refuse<OfferPanel>("fee",
                    $"The panel's buttons stopped at {Money(current)} on the way from {Money(start)} to {Money(fee)}: nothing was sent.");
            }

            var step = Math.Abs(moved - current);

            if (raise ? moved > fee : moved < fee)
            {
                return Refuse<OfferPanel>("fee",
                    $"The panel's buttons move the fee {Money(step)} a press and can't make exactly {Money(fee)} " +
                    $"(it went from {Money(current)} to {Money(moved)}): nothing was sent. Ask for a fee they reach, " +
                    $"from {Money(start)} in steps of {Money(step)}.");
            }

            if (Math.Abs(fee - moved) / step > MaxFeeSteps - press - 1)
            {
                return Refuse<OfferPanel>("fee",
                    $"{Money(fee)} is {Math.Abs(fee - start) / step} presses of {Money(step)} from where the panel " +
                    $"starts ({Money(start)}), more than the {MaxFeeSteps} one call makes: nothing was sent.");
            }

            current = moved;
            panel = after;
        }

        return Result.Ok(panel);
    }

    public async Task<Result<ContractNegotiation>> NegotiateContractAsync(int playerId, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await game.FindPlayerAsync(playerId, cancellationToken) is not { } player)
            {
                return Result.Fail<ContractNegotiation>(new EntityDoesNotExistError());
            }

            var negotiation = new ContractNegotiation { PlayerId = player.Id, PlayerName = player.Name, Club = player.Club, Applied = apply };
            var panel = await game.ReadContractPanelAsync(cancellationToken);
            (MessageOptions Message, MessageOption Option)? opening = null;

            if (panel is not null)
            {
                if (!IsFor(panel.Player, player.Name))
                {
                    return Refuse<ContractNegotiation>("playerId",
                        $"The contract panel open in the game is for {panel.Player} ({panel.Club}), not {player.Name}: " +
                        "finish or close that negotiation in the game first.");
                }

                Show(negotiation, panel);
            }
            else if (await FindOpeningAsync(player.Name, cancellationToken) is { } found)
            {
                opening = found;
                negotiation.MessageId = found.Message.MessageId;
                negotiation.MessageTitle = found.Message.Title;
            }
            else
            {
                return Refuse<ContractNegotiation>("playerId",
                    $"No message in the inbox opens a contract negotiation with {player.Name}, and no contract panel is " +
                    "open: his club must accept an offer first (make_transfer_offer, then reply_to_message to its answer).");
            }

            if (!apply)
            {
                negotiation.Notes.Add(panel is null
                    ? $"Applying opens the contract negotiation from '{opening!.Value.Message.Title}', agrees to the promises " +
                      "the agent asks for, goes on to the contract and stops there: its terms (salary, length, bonuses) " +
                      "come back to show the user, and a next apply offers the deal at them."
                    : panel.Steps.Contains(ContractStep.CloseDeal)
                        ? "Applying offers the deal at these terms (\"Fechar Acordo\"): show the user the salary and length first."
                        : "Applying goes on through the panel's pages to the contract and stops there, to show its terms.");
                return Result.Ok(negotiation);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<ContractNegotiation>(clearance.Errors);
            }

            negotiation.Notes.Add(SaveNote(clearance.Value));

            if (panel is null)
            {
                panel = await game.OpenContractPanelAsync(opening!.Value.Message.MessageId, opening.Value.Option.Index, cancellationToken);

                if (panel is null)
                {
                    negotiation.Notes.Add("The contract panel didn't show: a dry run says whether it opened late.");
                    return Result.Ok(negotiation);
                }

                if (!IsFor(panel.Player, player.Name))
                {
                    return Refuse<ContractNegotiation>("playerId",
                        $"The contract panel opened for {panel.Player} ({panel.Club}), not {player.Name}: nothing was pressed.");
                }

                Show(negotiation, panel);
            }

            await StepAsync(negotiation, panel, cancellationToken);
            return Result.Ok(negotiation);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ContractNegotiation>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Refuse<ContractNegotiation>("game", exception.Message);
        }
    }

    // Presses the panel's steps in turn. The deal is offered only when the panel was on the contract's terms as the
    // call began, so the terms offered are ones a dry run or the previous call showed; reaching them stops the call.
    private async Task StepAsync(ContractNegotiation negotiation, ContractPanel panel, CancellationToken cancellationToken)
    {
        var mayClose = panel.Steps.Contains(ContractStep.CloseDeal);

        for (var press = 0; press < MaxContractSteps; press++)
        {
            if (panel.Steps.Count == 0)
            {
                return;
            }

            var step = panel.Steps.Min();

            if (step == ContractStep.CloseDeal && !mayClose)
            {
                negotiation.Notes.Add("These are the contract's terms: show the user the salary and length, and apply " +
                                      "again to offer the deal at them.");
                return;
            }

            var after = await game.PressAsync(step, cancellationToken);
            negotiation.Steps.Add(step);

            if (after is null)
            {
                negotiation.Closed = step == ContractStep.CloseDeal;
                negotiation.Notes.Add("The panel closed after that step.");
                return;
            }

            negotiation.Response.AddRange(after.Texts.Except(panel.Texts).Except(negotiation.Response));
            Show(negotiation, after);

            if (after.Steps.Contains(step))
            {
                negotiation.Notes.Add($"The game didn't move on after {step}: the agent's answer is in the response, " +
                                      "and the panel is left open as it is.");
                return;
            }

            if (step == ContractStep.CloseDeal)
            {
                negotiation.Closed = true;
                negotiation.Notes.Add("The deal was offered at these terms. The transfer completes on the game's " +
                                      "calendar: a message such as \"Transferência de X adiada\" (the club wants to " +
                                      "postpone it) is answered with get_message_options and reply_to_message.");
                return;
            }

            panel = after;
        }
    }

    // The newest message about the player whose buttons open the contract negotiation: the club's acceptance.
    private async Task<(MessageOptions Message, MessageOption Option)?> FindOpeningAsync(string playerName, CancellationToken cancellationToken)
    {
        var page = await inbox.GetInboxAsync(false, 0, MessagesScanned, false, cancellationToken);

        foreach (var message in page.Messages.Where(message => Mentions(message, playerName)))
        {
            if (await inbox.GetMessageOptionsAsync(message.Id, cancellationToken) is { } options
                && options.Options.FirstOrDefault(option => option.NewsAction == ContractPanelAction && !option.IsDisabled) is { } open)
            {
                return (options, open);
            }
        }

        return null;
    }

    private static bool Mentions(InboxMessage message, string playerName)
    {
        return message.Title.Contains(playerName, StringComparison.OrdinalIgnoreCase)
               || (message.Body?.Contains(playerName, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static void Show(ContractNegotiation negotiation, ContractPanel panel)
    {
        negotiation.Terms = panel.Terms;
        negotiation.Buttons = panel.Buttons;
    }

    // The panel names the player as the game shows him, e.g. "(Walter)".
    private static bool IsFor(string? shown, string name)
    {
        var bare = shown?.Trim().Trim('(', ')').Trim();
        return !string.IsNullOrEmpty(bare)
               && (name.Contains(bare, StringComparison.OrdinalIgnoreCase) || bare.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private static string Money(long amount)
    {
        return amount.ToString(CultureInfo.InvariantCulture);
    }

    private static Result<T> Refuse<T>(string field, string reason)
    {
        return Result.Fail<T>(new ValidationError(field, [reason]));
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
