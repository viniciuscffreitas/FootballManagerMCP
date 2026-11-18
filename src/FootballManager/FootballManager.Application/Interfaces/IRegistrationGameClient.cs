using FootballManager.Domain.Registration;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// The first team's competition registrations and squad numbers, read and changed as the game's screens do. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game can't answer (after a send: the write is unknown) and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write, which is then not written.
/// </summary>
public interface IRegistrationGameClient
{
    Task<SquadRegistration> GetRegistrationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// A player of the club the game lists anywhere (the first team, a registration): someone a registration's own
    /// lists leave out, e.g. a youth player unticked from it. Null when the game lists no one with that id.
    /// </summary>
    Task<RegistrationPlayer?> FindClubPlayerAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>Whether the game lets the user register or unregister the player in that registration now.</summary>
    Task<bool> CanChangeRegistrationAsync(int competitionIndex, string competitionName, int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ticks or unticks the player in the registration, as its screen's tick does. Throws
    /// <see cref="Exceptions.GameRejectedException"/>, sending nothing, when the registration at
    /// <paramref name="competitionIndex"/> is no longer <paramref name="competitionName"/>.
    /// </summary>
    Task SetRegisteredAsync(int competitionIndex, string competitionName, int playerId, bool registered, CancellationToken cancellationToken = default);

    /// <summary>Confirms the registration's changes, as its screen's Confirm button does.</summary>
    Task ConfirmRegistrationAsync(int competitionIndex, string competitionName, CancellationToken cancellationToken = default);

    /// <summary>The player's number and the numbers the game offers him; null when he isn't in the first team.</summary>
    Task<PlayerSquadNumber?> GetSquadNumberAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>How many first-team players have no squad number.</summary>
    Task<int> CountPlayersNeedingNumbersAsync(CancellationToken cancellationToken = default);

    /// <summary>Gives the player the number, as the squad numbers screen's dropdown does, and confirms it.</summary>
    Task SetSquadNumberAsync(int playerId, int number, CancellationToken cancellationToken = default);

    /// <summary>"Pedir ao auxiliar para atribuir" on the squad numbers screen, then its confirmation.</summary>
    Task AskAssistantToNumberAsync(CancellationToken cancellationToken = default);
}
