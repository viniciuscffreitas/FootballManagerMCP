using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Registration;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Registration;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;

namespace FootballManager.Infrastructure.Clients;

// Registrations and squad numbers through the MCPFM plugin, on the edit sessions the game's screens bind (proven
// 18/11/2026 in the test save): a tick is a /set of the session's "Selected" with the player passed as Person, a number a
// /set of "NumSelected"; each screen's EventFmxConfirmChanges then commits it.
internal sealed class RegistrationBridgeClient(GameBridge bridge) : IRegistrationGameClient
{

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
        var person = await PersonAsync(competitionIndex, playerId, cancellationToken)
                     ?? throw new GameRejectedException($"{playerId} isn't a player of the club the game lists.");

        var tick = await bridge.GetRecordAsync<BridgeRegistrationDto>(
            session.Ref(RegistrationConstants.Person, person)
                .Props([RegistrationConstants.Name, RegistrationConstants.CanChange]),
            cancellationToken);

        Answered(tick, [RegistrationConstants.CanChange]);
        return tick.Values.CanChange
               ?? throw new GameUnavailableException($"The game didn't say whether {playerId} can be changed in {competitionName}.");
    }

    public async Task<RegistrationPlayer?> FindClubPlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        if (await PersonAsync(null, playerId, cancellationToken) is not { } path)
        {
            return null;
        }

        var person = await bridge.GetRecordAsync<BridgeRegistrationPlayerDto>(
            BridgeQuery.FromHuman().Base(path).WithHumanAndTeam().Props(RegistrationConstants.PlayerProperties), cancellationToken);
        Answered(person, RegistrationConstants.PlayerProperties);
        return Player(person.Values, path);
    }

    public async Task SetRegisteredAsync(
        int competitionIndex, string competitionName, int playerId, bool registered, CancellationToken cancellationToken = default)
    {
        var person = await PersonAsync(competitionIndex, playerId, cancellationToken)
                     ?? throw new GameRejectedException($"{playerId} isn't a player of the club the game lists. Nothing was sent.");

        // The registration is checked last, right before the send.
        var session = await SessionAsync(competitionIndex, competitionName, cancellationToken);
        var result = await bridge.WriteAsync<BridgeWriteResultDto>(
            session.Ref(RegistrationConstants.Person, person)
                .ToWrite("/set", ("path", RegistrationConstants.Selected), ("value", registered ? "true" : "false")),
            cancellationToken);

        // The plugin reads the tick back. No read back leaves it unknown; the other value means the game didn't take it.
        if (result.After is not { ValueKind: JsonValueKind.True or JsonValueKind.False } after)
        {
            throw new GameUnavailableException($"The tick for {playerId} in {competitionName} was sent but not read back.");
        }

        if (after.ValueKind != (registered ? JsonValueKind.True : JsonValueKind.False))
        {
            throw new GameRejectedException($"The game didn't take the tick for {playerId} in {competitionName}.");
        }
    }

    public async Task<bool> HasPendingChangesAsync(int competitionIndex, string competitionName, CancellationToken cancellationToken = default)
    {
        var session = await SessionAsync(competitionIndex, competitionName, cancellationToken);
        var record = await bridge.GetRecordAsync<BridgeRegistrationDto>(session.Props([RegistrationConstants.HasChanges]), cancellationToken);
        Answered(record, [RegistrationConstants.HasChanges]);
        return record.Values.HasChanges ?? throw new GameUnavailableException($"The game didn't say whether {competitionName} has changes not confirmed.");
    }

    public async Task ConfirmRegistrationAsync(int competitionIndex, string competitionName, CancellationToken cancellationToken = default)
    {
        var session = await SessionAsync(competitionIndex, competitionName, cancellationToken);
        await bridge.WriteAsync<JsonElement>(Confirm(session), cancellationToken);
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

        // Right after the save loads, the squad numbers edit session isn't up: it answers null for whether a number can
        // change and offers only "-" (seen 18/11/2026), until the squad numbers screen is opened. That's unknown, not locked.
        if (numbers.Values.CanChange is null)
        {
            throw new GameUnavailableException(
                "The game's squad numbers screen isn't set up (it answers nothing until it's opened after loading the save): " +
                "open Squad Numbers in the game, then try again.");
        }

        // The dropdown can lag a number given a moment before (seen 18/11/2026: 90 showed free after Patrick got it, and
        // giving it to Hayner took it from him). What each first-team player wears now counts too.
        var team = await bridge.GetListAsync<BridgeRegistrationPlayerDto>(
            BridgeQuery.FromHuman().List(RegistrationConstants.FirstTeamPlayers).WithHumanAndTeam()
                .Props([GameConstants.UniqueId, GameConstants.Name, RegistrationConstants.SquadNumber]),
            cancellationToken);
        var worn = new Dictionary<int, string>();

        foreach (var item in team.Items)
        {
            Answered(item, [GameConstants.UniqueId, RegistrationConstants.SquadNumber]);

            if (item.Values.Id != playerId && Number(item.Values.SquadNumber) is { } wornNumber)
            {
                worn[wornNumber] = $"{wornNumber}. {item.Values.Name}";
            }
        }

        return new PlayerSquadNumber
        {
            Id = playerId,
            Name = person.Values.Name ?? string.Empty,
            Number = Number(person.Values.SquadNumber),
            CanChange = numbers.Values.CanChange == true,
            Suggested = Number(numbers.Values.Suggested),
            Options = (numbers.Values.Options ?? [])
                .Where(option => option.RowData?.PropertyValue is > 0)
                .Select(option =>
                {
                    var number = (int)option.RowData!.PropertyValue!.Value;
                    var holder = worn.GetValueOrDefault(number);
                    return new SquadNumberOption
                    {
                        Number = number,
                        Label = option.Disabled == true || holder is null ? option.RowData.DisplayValue : holder,
                        Taken = option.Disabled == true || holder is not null
                    };
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

    // Null right after the save loads, until the squad numbers screen is opened (seen 18/11/2026): unknown, not "clean".
    public async Task<bool> HasPendingNumberChangesAsync(CancellationToken cancellationToken = default)
    {
        var record = await bridge.GetRecordAsync<BridgeRegistrationDto>(NumbersSession().Props([RegistrationConstants.HasChanges]), cancellationToken);
        Answered(record, [RegistrationConstants.HasChanges]);
        return record.Values.HasChanges ?? throw new GameUnavailableException(
            "The game didn't say whether the squad numbers screen holds numbers not confirmed: open Squad Numbers in the game, then try again.");
    }

    public async Task<List<RegistrationPlayer>> GetFirstTeamNumbersAsync(CancellationToken cancellationToken = default)
    {
        var team = await bridge.GetListAsync<BridgeRegistrationPlayerDto>(
            BridgeQuery.FromHuman().List(RegistrationConstants.FirstTeamPlayers).WithHumanAndTeam()
                .Props(RegistrationConstants.PlayerProperties),
            cancellationToken);

        return team.Items.Select(item =>
        {
            Answered(item, [GameConstants.UniqueId, GameConstants.Name, RegistrationConstants.SquadNumber]);
            return Player(item.Values, RegistrationConstants.FirstTeamPlayers);
        }).ToList();
    }

    // As the player's dropdown on the squad numbers screen, then the screen's confirm: the game then gives him the
    // number (seen 18/11/2026: João Fernandes -1 → 2 → -1). The dropdown takes the number a moment later than it says.
    public async Task SetSquadNumberAsync(int playerId, int number, CancellationToken cancellationToken = default)
    {
        var person = RegistrationConstants.ById(RegistrationConstants.FirstTeamPlayers, playerId);
        var result = await bridge.WriteAsync<BridgeWriteResultDto>(
            NumbersSession().Ref(RegistrationConstants.Person, person)
                .ToWrite("/set", ("path", RegistrationConstants.NumberSelected), ("value", number.ToString(CultureInfo.InvariantCulture))),
            cancellationToken);

        if (result.Changed is null)
        {
            throw new GameUnavailableException($"Number {number} for {playerId} was sent but not read back. Nothing was confirmed.");
        }

        if (result.Changed == false)
        {
            throw new GameRejectedException($"The game didn't take number {number} for {playerId}. Nothing was confirmed.");
        }

        await bridge.WriteAsync<JsonElement>(Confirm(NumbersSession()), cancellationToken);
    }

    // The screen's Confirm: EventFmxConfirmChanges on its edit session, with no data. It commits without the dialog the
    // button shows ("Enviar a numeração atual do elenco?").
    private static string Confirm(BridgeQuery session)
    {
        return session.ToWrite("/native", ("event", RegistrationConstants.ConfirmEvent), ("data", "null"), ("template", "none"));
    }

    // Where the game lists the player, to pass him as Person: the registration's own lists, the first team, then the
    // other registrations (a youth player unticked from one shows in no list of it, seen 18/11/2026).
    private async Task<string?> PersonAsync(int? competitionIndex, int playerId, CancellationToken cancellationToken)
    {
        var registrations = await bridge.GetListAsync<BridgeRegistrationDto>(
            BridgeQuery.FromHuman().List(RegistrationConstants.Competitions).WithHumanAndTeam().Props([RegistrationConstants.Name]),
            cancellationToken);
        var lists = new List<string>();

        if (competitionIndex is { } own)
        {
            lists.Add($"{RegistrationConstants.Competition(own)}.{RegistrationConstants.Registered}");
            lists.Add($"{RegistrationConstants.Competition(own)}.{RegistrationConstants.Unregistered}");
        }

        lists.Add(RegistrationConstants.FirstTeamPlayers);
        lists.AddRange(Enumerable.Range(0, registrations.Items.Count).Where(i => i != competitionIndex)
            .Select(i => $"{RegistrationConstants.Competition(i)}.{RegistrationConstants.Registered}"));

        foreach (var list in lists)
        {
            var players = await bridge.GetListAsync<BridgeRegistrationPlayerDto>(
                BridgeQuery.FromHuman().List(list).WithHumanAndTeam().Props([GameConstants.UniqueId]), cancellationToken);

            if (players.Items.Any(item => item.Values.Id == playerId))
            {
                return RegistrationConstants.ById(list, playerId);
            }
        }

        return null;
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

    private async Task<List<RegistrationPlayer>> PlayersAsync(int index, string list, CancellationToken cancellationToken)
    {
        var players = await bridge.GetListAsync<BridgeRegistrationPlayerDto>(
            BridgeQuery.FromHuman().List($"{RegistrationConstants.Competition(index)}.{list}").WithHumanAndTeam()
                .Props(RegistrationConstants.PlayerProperties),
            cancellationToken);

        return players.Items.Select(item =>
        {
            Answered(item, [GameConstants.UniqueId, GameConstants.Name]);
            return Player(item.Values, list);
        }).ToList();
    }

    private static RegistrationPlayer Player(BridgeRegistrationPlayerDto values, string list)
    {
        return new RegistrationPlayer
        {
            // A player without an id is an unknown read, never player 0.
            Id = values.Id ?? throw new GameUnavailableException($"The game didn't give the id of a player in {list}."),
            Name = values.Name ?? string.Empty,
            SquadNumber = Number(values.SquadNumber),
            IsGoalkeeper = values.IsGoalkeeper == true
        };
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
