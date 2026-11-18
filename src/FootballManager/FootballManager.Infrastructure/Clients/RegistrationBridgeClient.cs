using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Registration;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Registration;

namespace FootballManager.Infrastructure.Clients;

// Registrations and squad numbers through the MCPFM plugin, read from the edit sessions the game's screens bind (seen
// 18/11/2026). The writes aren't proven in the game yet (ticket #73): each refuses before sending anything.
internal sealed class RegistrationBridgeClient(GameBridge bridge) : IRegistrationGameClient
{
    private const string NotProven =
        "This tool can't write it yet: the game's way to do it isn't proven in the test save (ticket #73). Nothing was sent.";

    public async Task<SquadRegistration> GetRegistrationAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await bridge.GetListAsync<BridgeRegistrationDto>(
            BridgeQuery.FromHuman().List(RegistrationConstants.Competitions).WithHumanAndTeam().Props(RegistrationConstants.CompetitionProperties),
            cancellationToken);

        var competitions = await Task.WhenAll(sessions.Items.Select(async (item, index) =>
        {
            Answered(item, RegistrationConstants.CompetitionProperties);

            // Without its name or whether its window is open, the registration can't be checked or changed.
            if (item.Values.Name is null || item.Values.WindowOpen is null)
            {
                throw new GameUnavailableException("The game didn't give a registration's name or whether its window is open.");
            }

            var registered = PlayersAsync(index, RegistrationConstants.Registered, cancellationToken);
            var unregistered = PlayersAsync(index, RegistrationConstants.Unregistered, cancellationToken);
            return Competition(index, item.Values, await registered, await unregistered);
        }));

        return new SquadRegistration { Competitions = competitions.ToList() };
    }

    public async Task<bool> CanChangeRegistrationAsync(
        int competitionIndex, string competitionName, int playerId, CancellationToken cancellationToken = default)
    {
        var session = await SessionAsync(competitionIndex, competitionName, cancellationToken);
        var player = await PlayerListAsync(competitionIndex, playerId, cancellationToken)
                     ?? throw new GameRejectedException($"{playerId} is in neither list of {competitionName}.");

        var tick = await bridge.GetRecordAsync<BridgeRegistrationDto>(
            session.Ref(RegistrationConstants.Person, RegistrationConstants.ById(player, playerId))
                .Props([RegistrationConstants.Name, RegistrationConstants.CanChange]),
            cancellationToken);

        Answered(tick, [RegistrationConstants.CanChange]);
        return tick.Values.CanChange == true;
    }

    public async Task SetRegisteredAsync(
        int competitionIndex, string competitionName, int playerId, bool registered, CancellationToken cancellationToken = default)
    {
        await SessionAsync(competitionIndex, competitionName, cancellationToken);
        throw new GameRejectedException(NotProven);
    }

    public async Task ConfirmRegistrationAsync(int competitionIndex, string competitionName, CancellationToken cancellationToken = default)
    {
        await SessionAsync(competitionIndex, competitionName, cancellationToken);
        throw new GameRejectedException(NotProven);
    }

    public async Task<PlayerSquadNumber?> GetSquadNumberAsync(int playerId, CancellationToken cancellationToken = default)
    {
        BridgeRecordDto<BridgeRegistrationPlayerDto> person;
        var path = RegistrationConstants.ById(RegistrationConstants.FirstTeamPlayers, playerId);

        try
        {
            // A selector no first-team player matches is refused by the game: he isn't in the first team.
            person = await bridge.GetRecordAsync<BridgeRegistrationPlayerDto>(
                BridgeQuery.FromHuman().Base(path).WithHumanAndTeam().Props(RegistrationConstants.PlayerProperties),
                cancellationToken, rejectionThrows: true);
        }
        catch (GameRejectedException)
        {
            return null;
        }

        Answered(person, RegistrationConstants.PlayerProperties);

        var numbers = await bridge.GetRecordAsync<BridgeSquadNumberDto>(
            NumbersSession().Ref(RegistrationConstants.Person, path)
                .Props([RegistrationConstants.CanChangeNumber, RegistrationConstants.SuggestedNumber, RegistrationConstants.NumberDropdown]),
            cancellationToken);

        Answered(numbers, [RegistrationConstants.CanChangeNumber, RegistrationConstants.NumberDropdown]);

        return new PlayerSquadNumber
        {
            Id = playerId,
            Name = person.Values.Name ?? string.Empty,
            Number = Number(person.Values.SquadNumber),
            CanChange = numbers.Values.CanChange == true,
            Suggested = Number(numbers.Values.Suggested),
            Options = (numbers.Values.Options ?? [])
                .Where(option => option.RowData?.PropertyValue is > 0)
                .Select(option => new SquadNumberOption
                {
                    Number = (int)option.RowData!.PropertyValue!.Value,
                    Label = option.RowData.DisplayValue,
                    Taken = option.Disabled == true
                })
                .ToList()
        };
    }

    public async Task<int> CountPlayersNeedingNumbersAsync(CancellationToken cancellationToken = default)
    {
        var record = await bridge.GetRecordAsync<BridgeNeedingNumbersDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props([RegistrationConstants.NeedingNumbers]), cancellationToken);

        Answered(record, [RegistrationConstants.NeedingNumbers]);

        // No list at all is an unknown read, not "nobody needs a number".
        return record.Values.Players?.Count
               ?? throw new GameUnavailableException("The game didn't say which players need a squad number.");
    }

    public Task SetSquadNumberAsync(int playerId, int number, CancellationToken cancellationToken = default)
    {
        throw new GameRejectedException(NotProven);
    }

    public Task AskAssistantToNumberAsync(CancellationToken cancellationToken = default)
    {
        throw new GameRejectedException(NotProven);
    }

    private static BridgeQuery NumbersSession()
    {
        return BridgeQuery.FromHuman().Base(RegistrationConstants.NumbersSession)
            .Context(RegistrationConstants.NumbersFilter, RegistrationConstants.FirstTeamFilter)
            .WithHumanAndTeam();
    }

    // The registration at that place, read again: it must still be the one named, or nothing is sent.
    private async Task<BridgeQuery> SessionAsync(int index, string name, CancellationToken cancellationToken)
    {
        var session = await bridge.GetRecordAsync<BridgeRegistrationDto>(Session(index).Props([RegistrationConstants.Name]), cancellationToken);
        Answered(session, [RegistrationConstants.Name]);

        if (!string.Equals(session.Values.Name, name, StringComparison.CurrentCulture))
        {
            throw new GameRejectedException(
                $"The team's registrations changed since the check: {name} is no longer where it was. Nothing was sent; run the dry run again.");
        }

        return Session(index);
    }

    private static BridgeQuery Session(int index)
    {
        return BridgeQuery.FromHuman().Base(RegistrationConstants.Competition(index)).WithHumanAndTeam();
    }

    // Which of the registration's lists holds the player: the game passes him from there as Person.
    private async Task<string?> PlayerListAsync(int index, int playerId, CancellationToken cancellationToken)
    {
        foreach (var list in new[] { RegistrationConstants.Registered, RegistrationConstants.Unregistered })
        {
            var path = $"{RegistrationConstants.Competition(index)}.{list}";
            var players = await bridge.GetListAsync<BridgeRegistrationPlayerDto>(
                BridgeQuery.FromHuman().List(path).WithHumanAndTeam().Props([GameConstants.UniqueId]), cancellationToken);

            if (players.Items.Any(item => item.Values.Id == playerId))
            {
                return path;
            }
        }

        return null;
    }

    private async Task<List<RegistrationPlayer>> PlayersAsync(int index, string list, CancellationToken cancellationToken)
    {
        var players = await bridge.GetListAsync<BridgeRegistrationPlayerDto>(
            BridgeQuery.FromHuman().List($"{RegistrationConstants.Competition(index)}.{list}").WithHumanAndTeam()
                .Props(RegistrationConstants.PlayerProperties),
            cancellationToken);

        return players.Items.Select(item =>
        {
            Answered(item, [GameConstants.UniqueId, GameConstants.Name]);
            return new RegistrationPlayer
            {
                // A player without an id is an unknown read, never player 0.
                Id = item.Values.Id ?? throw new GameUnavailableException($"The game didn't give the id of a player in {list}."),
                Name = item.Values.Name ?? string.Empty,
                SquadNumber = Number(item.Values.SquadNumber),
                IsGoalkeeper = item.Values.IsGoalkeeper == true
            };
        }).ToList();
    }

    private static CompetitionRegistration Competition(
        int index, BridgeRegistrationDto values, List<RegistrationPlayer> registered, List<RegistrationPlayer> unregistered)
    {
        return new CompetitionRegistration
        {
            Index = index,
            Name = values.Name!,
            Competition = values.Competition,
            CompetitionId = values.CompetitionId,
            WindowOpen = values.WindowOpen == true,
            Window = values.Window,
            Deadline = values.Deadline,
            MaxSquadSize = values.MaxSquadSize,
            MeetsRules = values.MeetsRules == true,
            RulesSummary = values.RulesSummary,
            Notes = values.Notes?.Trim(),
            Rules = (values.Rules ?? []).Select(rule => new RegistrationRule
            {
                Type = rule.RuleType,
                Title = rule.SquadSelectionRuleTitle,
                Text = rule.SquadSelectionRuleString,
                Value = rule.PlayerValue,
                Display = rule.SquadSelectionRuleValue,
                Met = rule.CurrentlyMeetsRule == 1
            }).ToList(),
            Registered = registered,
            Unregistered = unregistered
        };
    }

    private static int? Number(int? value)
    {
        return value is null or <= 0 ? null : value;
    }

    // A value the game didn't answer in time, or failed to read, is unknown: never read it as empty or false.
    private static void Answered<T>(BridgeRecordDto<T> record, IEnumerable<string> properties) where T : new()
    {
        foreach (var property in properties)
        {
            if (record.Pending?.Contains(property) == true)
            {
                throw new GameUnavailableException($"The game didn't answer {property} (it may be busy).");
            }

            if (record.Errors?.TryGetValue(property, out var error) == true)
            {
                throw new GameUnavailableException($"The game failed to read {property}: {error}");
            }
        }
    }
}
