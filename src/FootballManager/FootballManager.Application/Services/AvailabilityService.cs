using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Availability;
using FootballManager.Domain.Players;

namespace FootballManager.Application.Services;

public interface IAvailabilityService
{
    Task<Result<MatchAvailability>> GetAvailabilityAsync(CancellationToken cancellationToken = default);
}

public class AvailabilityService(ISquadGameClient squadClient, IAvailabilityGameClient gameClient) : IAvailabilityService
{
    // CurrentInjury.InjuryAvailability is 0 for a player whose injury (if any) doesn't keep him out.
    private const int AvailableDespiteInjury = 0;

    public async Task<Result<MatchAvailability>> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        Squad squad;
        AvailabilityReadout readout;

        try
        {
            // Condition and sharpness come from the squad read get_squad uses; no attributes, ability or stats.
            var squadRead = squadClient.GetSquadAsync([], includeAbility: false, includeStats: false, cancellationToken);
            var readoutRead = gameClient.GetAvailabilityAsync(cancellationToken);
            squad = await squadRead;
            readout = await readoutRead;
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<MatchAvailability>(new GameUnavailableError(exception.Message));
        }

        var injured = readout.Injuries.Where(injury => injury.Availability != AvailableDespiteInjury).ToList();
        var injuredIds = injured.Select(injury => injury.PlayerId).ToHashSet();
        var suspendedIds = readout.Bans.Select(ban => ban.PlayerId).ToHashSet();

        return Result.Ok(new MatchAvailability
        {
            TeamName = squad.TeamName,
            NextMatch = readout.NextMatch,
            Injured = injured,
            Suspended = readout.Bans,
            Players = squad.Players.Select(player => new PlayerAvailability
            {
                Id = player.Id,
                Name = player.Name,
                Positions = player.Positions,
                BestRole = player.BestRole,
                Condition = player.Condition,
                MatchSharpness = player.MatchSharpness,
                AverageRating = player.AverageRating,
                Absences = Absences(player.Id, injuredIds, suspendedIds)
            }).ToList()
        });
    }

    private static List<Absence> Absences(int playerId, HashSet<int> injuredIds, HashSet<int> suspendedIds)
    {
        var absences = new List<Absence>();

        if (injuredIds.Contains(playerId))
        {
            absences.Add(Absence.Injured);
        }

        if (suspendedIds.Contains(playerId))
        {
            absences.Add(Absence.Suspended);
        }

        return absences;
    }
}
