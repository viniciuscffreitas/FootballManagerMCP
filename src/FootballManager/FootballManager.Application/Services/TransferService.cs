using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
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
}

// An offer goes through the panel the player's menu opens (as the user's click does): it starts at a fee of the
// game's choosing, its +/− buttons move the fee a step each (R$10 mil in the game), and "make offer" sends it to the
// club with the panel's other terms as they stand (no instalments or clauses unless added by hand). The club answers
// in the inbox later; a counter-offer is a message answered with its plain replies. A dry run opens nothing.
public class TransferService(ITransferGameClient game, ISaveGuard saveGuard) : ITransferService
{
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
