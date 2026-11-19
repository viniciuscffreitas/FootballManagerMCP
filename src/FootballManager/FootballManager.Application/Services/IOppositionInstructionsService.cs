using FluentResults;
using FootballManager.Domain.Opposition;

namespace FootballManager.Application.Services;

public interface IOppositionInstructionsService
{
    /// <summary>
    /// Plans, and with <paramref name="apply"/> makes, a change to the instructions against the next opponent. The
    /// player is an opponent of the lineup the game expects, by FM id or name; <paramref name="settings"/> are the
    /// instructions to set, by <see cref="OppositionInstructionValues"/> key.
    /// </summary>
    Task<Result<OppositionInstructionsChange>> ChangeAsync(
        OppositionInstructionsAction? action,
        string? player,
        IReadOnlyDictionary<OppositionInstruction, string>? settings,
        bool apply,
        CancellationToken cancellationToken = default);
}
