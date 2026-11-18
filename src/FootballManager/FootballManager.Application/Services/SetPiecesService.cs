using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Saves;
using FootballManager.Domain.SetPieces;

namespace FootballManager.Application.Services;

public interface ISetPiecesService
{
    /// <summary>The captain, the vice, the armband's stand-ins and every taker list, read live.</summary>
    Task<Result<SetPieceSetup>> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes one player captain and/or another vice-captain. A dry run unless <paramref name="apply"/>; a player who
    /// already holds the role is left as he is.
    /// </summary>
    Task<Result<SetPieceEdit>> SetCaptainsAsync(int? captainId, int? viceCaptainId, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Puts these players at the top of one taker list, in order. A dry run unless <paramref name="apply"/>.</summary>
    Task<Result<SetPieceEdit>> SetTakersAsync(string list, IReadOnlyList<int> playerIds, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Has the staff pick every taker. A dry run unless <paramref name="apply"/>.</summary>
    Task<Result<SetPieceEdit>> AskStaffAsync(bool apply, CancellationToken cancellationToken = default);
}

// The game keeps one captain and one vice for the club, and an ordered list of takers per set piece: the first in a
// list takes it, the next one when he's off the pitch. Every change is checked against what the game holds now, so a
// dry run shows the before and the after, and an apply goes through the save guard and is read back.
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

        return await RunAsync(apply, cancellationToken, (before, edit) =>
        {
            var captain = captainId is { } id ? Find(before, id) : null;
            var vice = viceCaptainId is { } other ? Find(before, other) : null;

            if ((captainId is not null && captain is null) || (viceCaptainId is not null && vice is null))
            {
                return Result.Fail(new EntityDoesNotExistError());
            }

            var writes = new List<Func<Task>>();

            if (captain is not null)
            {
                Plan(edit, writes, "captain", before.Captain, captain, () => game.MakeCaptainAsync(captain.Id, cancellationToken));
            }

            if (vice is not null)
            {
                Plan(edit, writes, "vice-captain", before.ViceCaptain, vice, () => game.MakeViceCaptainAsync(vice.Id, cancellationToken));
            }

            return Result.Ok(writes);
        });
    }

    public async Task<Result<SetPieceEdit>> SetTakersAsync(
        string list, IReadOnlyList<int> playerIds, bool apply, CancellationToken cancellationToken = default)
    {
        if (playerIds.Count == 0)
        {
            return Result.Fail<SetPieceEdit>(new ValidationError("playerIds", ["Name at least one taker, first to take it first."]));
        }

        if (playerIds.Distinct().Count() != playerIds.Count)
        {
            return Result.Fail<SetPieceEdit>(new ValidationError("playerIds", ["A player is named twice: name each taker once."]));
        }

        return await RunAsync(apply, cancellationToken, (before, edit) =>
        {
            if (before.TakerLists.FirstOrDefault(taker => string.Equals(taker.Key, list, StringComparison.OrdinalIgnoreCase)) is not { } found)
            {
                var offered = string.Join(", ", before.TakerLists.Select(taker => taker.Key));
                return Result.Fail(new ValidationError("list", [$"The set piece screen has no list '{list}' (lists: {offered})."]));
            }

            var players = playerIds.Select(id => Find(before, id)).ToList();

            if (players.Any(player => player is null))
            {
                return Result.Fail(new EntityDoesNotExistError());
            }

            var current = string.Join(", ", found.Takers.Select(player => player.Name));
            var wanted = string.Join(", ", players.Select(player => player!.Name));

            if (found.Takers.Select(player => player.Id).Take(playerIds.Count).SequenceEqual(playerIds))
            {
                edit.Notes.Add($"{found.Label ?? found.Key} already starts with {wanted}: nothing to change.");
                return Result.Ok(new List<Func<Task>>());
            }

            edit.Steps.Add($"{found.Label ?? found.Key}: {wanted} first, in that order (now: {(current.Length > 0 ? current : "none")}).");
            return Result.Ok(new List<Func<Task>> { () => game.SetTakersAsync(found.Key, playerIds, cancellationToken) });
        });
    }

    public async Task<Result<SetPieceEdit>> AskStaffAsync(bool apply, CancellationToken cancellationToken = default)
    {
        return await RunAsync(apply, cancellationToken, (_, edit) =>
        {
            edit.Steps.Add("The staff picks every set piece taker, as the screen's request to the assistant does.");
            return Result.Ok(new List<Func<Task>> { () => game.AskStaffToAssignTakersAsync(cancellationToken) });
        });
    }

    // Reads the setup, lets the change check itself against it and plan its writes, and on an apply clears the save,
    // writes in order and reads the setup back. A plan with no writes changes nothing, apply or not.
    private async Task<Result<SetPieceEdit>> RunAsync(
        bool apply, CancellationToken cancellationToken, Func<SetPieceSetup, SetPieceEdit, Result<List<Func<Task>>>> plan)
    {
        try
        {
            var before = await game.ReadAsync(cancellationToken);
            var edit = new SetPieceEdit { Before = before };
            var writes = plan(before, edit);

            if (writes.IsFailed)
            {
                return Result.Fail<SetPieceEdit>(writes.Errors);
            }

            if (!apply || writes.Value.Count == 0)
            {
                return Result.Ok(edit);
            }

            var clearance = await saveGuard.PrepareWriteAsync(cancellationToken);

            if (clearance.IsFailed)
            {
                return Result.Fail<SetPieceEdit>(clearance.Errors);
            }

            edit.Notes.Add(SaveNote(clearance.Value));

            foreach (var write in writes.Value)
            {
                await write();
            }

            edit.Applied = true;
            edit.After = await game.ReadAsync(cancellationToken);
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

    private static void Plan(
        SetPieceEdit edit, List<Func<Task>> writes, string role, SetPiecePlayer? now, SetPiecePlayer wanted, Func<Task> write)
    {
        if (now?.Id == wanted.Id)
        {
            edit.Notes.Add($"{wanted.Name} is already {role}: nothing to change.");
            return;
        }

        edit.Steps.Add($"Make {wanted.Name} {role} (now: {now?.Name ?? "none"}).");
        writes.Add(write);
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
