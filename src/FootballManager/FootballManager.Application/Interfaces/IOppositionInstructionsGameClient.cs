using FootballManager.Domain.Opposition;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads and changes the instructions against the user's next opponent in the running game. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when it can't reach the game.
/// </summary>
public interface IOppositionInstructionsGameClient
{
    /// <summary>The instructions against the next opponent; null when the team has no match ahead.</summary>
    Task<OppositionInstructions?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets one instruction on an opponent player of the lineup the game expects, to a key of
    /// <see cref="OppositionInstructionValues"/>. Throws <see cref="Exceptions.GameRejectedException"/>, writing nothing,
    /// when the player is no longer in that lineup.
    /// </summary>
    Task SetAsync(int playerId, OppositionInstruction instruction, string value, CancellationToken cancellationToken = default);
}
