using FootballManager.Domain.Saves;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads which save the running game has open and the game's version. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game can't be reached.
/// </summary>
public interface ISaveSessionGameClient
{
    /// <summary>The open save's name, whether it's the test save, and once loaded its club and game date.</summary>
    Task<LoadedSave> GetLoadedSaveAsync(CancellationToken cancellationToken = default);

    /// <summary>The game's build, e.g. "27.0.1.2494769"; null when the game doesn't say.</summary>
    Task<string?> GetGameVersionAsync(CancellationToken cancellationToken = default);
}
