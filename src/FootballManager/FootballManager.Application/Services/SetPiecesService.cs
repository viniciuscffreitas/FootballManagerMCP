using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Saves;
using FootballManager.Domain.SetPieces;

namespace FootballManager.Application.Services;

// The game keeps one captain and one vice for the club; the player's menu makes him either ("Tornar Capitão",
// "Tornar Vice-capitão") and leaves the entry out for the one already in the role. Every change is checked against the
// game (the squad, the current holders, the menu) before anything is written, so a dry run shows the before and what
// would change, and an apply goes through the save guard, writes and reads the setup back.
public class SetPiecesService(ISetPiecesGameClient game, ISaveGuard saveGuard) : ISetPiecesService
{
    public async Task<Result<SetPieceSetup>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await game.ReadAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SetPieceSetup>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<SetPieceEdit>> SetCaptainsAsync(
        int? captainId, int? viceCaptainId, bool apply, CancellationToken cancellationToken = default)
    {
        if (captainId is null && viceCaptainId is null)
        {
            return Result.Fail<SetPieceEdit>(new ValidationError("captainId", ["Name a captain, a vice-captain or both."]));
        }

        if (captainId == viceCaptainId)
        {
            return Result.Fail<SetPieceEdit>(new ValidationError("viceCaptainId",
                ["The captain and the vice-captain are the same player: name two players."]));
        }

        try
        {
            var before = await game.ReadAsync(cancellationToken);
            var edit = new SetPieceEdit { Before = before };
            var captain = captainId is { } id ? Find(before, id) : null;
            var vice = viceCaptainId is { } other ? Find(before, other) : null;

            if ((captainId is not null && captain is null) || (viceCaptainId is not null && vice is null))
            {
                return Result.Fail<SetPieceEdit>(new EntityDoesNotExistError());
            }

            if (captain is null && vice is not null && vice.Id == before.Captain?.Id)
            {
                // His menu offers "Tornar Vice-capitão", but the game takes the armband off him for it: no captain left.
                return Result.Fail<SetPieceEdit>(new ValidationError("viceCaptainId",
                    [$"{vice.Name} is the captain: making him vice would leave the club without one. Name a new captain too."]));
            }

            var writes = new List<(string Role, Func<Task> Write, Action<WriteState> Record)>();

            foreach (var (player, role, now, offered, write, record) in new (SetPiecePlayer?, string, SetPiecePlayer?, Func<CaptaincyOffer, bool>, Func<int, Task>, Action<WriteState>)[]
                     {
                         (captain, "captain", before.Captain, offer => offer.MakeCaptain, playerId => game.MakeCaptainAsync(playerId, cancellationToken), state => edit.CaptainWritten = state),
                         (vice, "vice-captain", before.ViceCaptain, offer => offer.MakeViceCaptain, playerId => game.MakeViceCaptainAsync(playerId, cancellationToken), state => edit.ViceCaptainWritten = state)
                     })
            {
                if (player is null)
                {
                    continue;
                }

                var planned = await PlanAsync(edit, role, now, player, offered, cancellationToken);

                if (planned.IsFailed)
                {
                    return Result.Fail<SetPieceEdit>(planned.Errors);
                }

                if (planned.Value)
                {
                    writes.Add((role, () => write(player.Id), record));
                }
            }

            if (!apply || writes.Count == 0)
            {
                return Result.Ok(edit);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<SetPieceEdit>(clearance.Errors);
            }

            edit.Notes.Add(SaveNote(clearance.Value));

            var lost = false;

            foreach (var (role, write, record) in writes)
            {
                if (lost)
                {
                    edit.Notes.Add($"Warning: the {role} change was not sent: the game's answer to the earlier change was lost.");
                    continue;
                }

                try
                {
                    await write();
                    record(WriteState.Written);
                }
                catch (GameRejectedException exception) when (Sent(edit))
                {
                    // An earlier role already went into the game: say what changed instead of failing, so nobody repeats it.
                    edit.Notes.Add($"Warning: the {role} change was not written ({exception.Message}); " +
                                   "the earlier change stays in the game.");
                    break;
                }
                catch (GameUnavailableException exception)
                {
                    // A timeout or an error after the write went out doesn't say the game didn't take it: the read-back
                    // decides. Nothing more is sent to a game that isn't answering.
                    record(WriteState.Unknown);
                    lost = true;
                    edit.Notes.Add($"Warning: the game's answer to the {role} change was lost ({exception.Message}); " +
                                   "the read-back decides whether it was written.");
                }
            }

            await ReadBackAsync(edit, before, captain, vice, cancellationToken);
            edit.Applied = edit.CaptainWritten == WriteState.Written || edit.ViceCaptainWritten == WriteState.Written
                ? WriteState.Written
                : Sent(edit) ? WriteState.Unknown : WriteState.NotWritten;
            return Result.Ok(edit);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SetPieceEdit>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<SetPieceEdit>(new ValidationError("game", [exception.Message]));
        }
    }

    // Adds the step for a role change and says to write it, or a note when the player already holds the role; a refusal
    // when his menu doesn't offer it.
    private async Task<Result<bool>> PlanAsync(
        SetPieceEdit edit, string role, SetPiecePlayer? now, SetPiecePlayer wanted, Func<CaptaincyOffer, bool> offered,
        CancellationToken cancellationToken)
    {
        if (now?.Id == wanted.Id)
        {
            edit.Notes.Add($"{wanted.Name} is already {role}: nothing to change.");
            return Result.Ok(false);
        }

        if (!offered(await game.GetCaptaincyOfferAsync(wanted.Id, cancellationToken)))
        {
            return Result.Fail<bool>(new ValidationError("game",
                [$"{wanted.Name}'s menu doesn't offer making him {role} now."]));
        }

        edit.Steps.Add($"Make {wanted.Name} {role} (now: {now?.Name ?? "none"}).");
        return Result.Ok(true);
    }

    // Reads the setup after the writes and confirms only when it shows every role asked for, and the role left out still
    // held by the player who held it before. A read that fails leaves the writes reported: the game holds them whether
    // or not the read went through.
    private async Task ReadBackAsync(
        SetPieceEdit edit, SetPieceSetup before, SetPiecePlayer? captain, SetPiecePlayer? vice, CancellationToken cancellationToken)
    {
        try
        {
            edit.After = await game.ReadAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            edit.Notes.Add(edit.CaptainWritten == WriteState.Unknown || edit.ViceCaptainWritten == WriteState.Unknown
                ? $"Warning: whether the change was written is unknown: the setup could not be read back either " +
                  $"({exception.Message}). Check with get_set_pieces before trying again."
                : $"Warning: written, but the setup could not be read back ({exception.Message}); " +
                  "read it again before changing anything.");
            return;
        }

        // A write whose answer was lost counts as written when the read-back shows the player in the role.
        edit.CaptainWritten = Resolve(edit, "captain", edit.CaptainWritten, captain, edit.After.Captain);
        edit.ViceCaptainWritten = Resolve(edit, "vice-captain", edit.ViceCaptainWritten, vice, edit.After.ViceCaptain);

        var mismatches = new List<string>();

        foreach (var (role, wanted, now) in new[]
                 {
                     ("captain", captain ?? before.Captain, edit.After.Captain),
                     ("vice-captain", vice ?? before.ViceCaptain, edit.After.ViceCaptain)
                 })
        {
            if (now?.Id != wanted?.Id)
            {
                mismatches.Add($"{role} is {now?.Name ?? "none"}, not {wanted?.Name ?? "none"}");
            }
        }

        edit.Confirmed = mismatches.Count == 0;

        if (!edit.Confirmed)
        {
            edit.Notes.Add($"Warning: the read-back doesn't match the request: {string.Join("; ", mismatches)}.");
        }
    }

    private static WriteState Resolve(
        SetPieceEdit edit, string role, WriteState state, SetPiecePlayer? wanted, SetPiecePlayer? now)
    {
        if (state != WriteState.Unknown)
        {
            return state;
        }

        if (now?.Id == wanted?.Id)
        {
            return WriteState.Written;
        }

        edit.Notes.Add($"Warning: the {role} change was not written: the read-back shows {now?.Name ?? "none"}.");
        return WriteState.NotWritten;
    }

    // Whether any write went out, taken or with its answer lost.
    private static bool Sent(SetPieceEdit edit)
    {
        return edit.CaptainWritten != WriteState.NotWritten || edit.ViceCaptainWritten != WriteState.NotWritten;
    }

    private static SetPiecePlayer? Find(SetPieceSetup setup, int playerId)
    {
        return setup.Squad.FirstOrDefault(player => player.Id == playerId);
    }

    private static string SaveNote(SaveClearance save)
    {
        return save.Backup is null
            ? $"Written to the test save '{save.SaveName}', without a copy."
            : $"Checked '{save.SaveName}' ({save.Club}, game date {save.GameDate:yyyy-MM-dd}, version {save.GameVersion}) " +
              $"and copied it to {save.Backup} before writing.";
    }
}
