using FluentResults;
using FootballManager.Domain.Registration;

namespace FootballManager.Application.Services;

public interface ISquadNumbersService
{
    /// <summary>
    /// Gives a first-team player a squad number, or (<paramref name="askAssistant"/>) asks the assistant to number
    /// everyone, as the game's squad numbers screen does. A dry run unless <paramref name="apply"/>.
    /// </summary>
    Task<Result<SquadNumberChange>> ChangeNumberAsync(
        int? playerId, int? number, bool askAssistant, bool apply, CancellationToken cancellationToken = default);
}
