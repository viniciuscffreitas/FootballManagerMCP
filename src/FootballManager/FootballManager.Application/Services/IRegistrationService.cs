using FluentResults;
using FootballManager.Domain.Registration;

namespace FootballManager.Application.Services;

public interface IRegistrationService
{
    /// <summary>The first team's registration in each competition: window, rules, who is registered and who isn't.</summary>
    Task<Result<SquadRegistration>> GetRegistrationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers and unregisters first-team players in one competition, as its registration screen does. A dry run
    /// unless <paramref name="apply"/>: it checks each player and the competition's rules against the change.
    /// </summary>
    /// <param name="competition">The registration's name or the competition's, as get_squad_registration lists them.</param>
    Task<Result<RegistrationChange>> ChangeRegistrationAsync(
        string? competition, IReadOnlyList<int?>? register, IReadOnlyList<int?>? unregister, bool apply,
        CancellationToken cancellationToken = default);
}
