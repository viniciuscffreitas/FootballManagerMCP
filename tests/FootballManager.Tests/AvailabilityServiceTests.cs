using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Availability;
using FootballManager.Domain.Players;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class AvailabilityServiceTests
{
    private readonly ISquadGameClient _squad = Substitute.For<ISquadGameClient>();
    private readonly IAvailabilityGameClient _game = Substitute.For<IAvailabilityGameClient>();
    private readonly AvailabilityService _availability;

    public AvailabilityServiceTests()
    {
        _availability = new AvailabilityService(_squad, _game);
    }

    [Fact]
    public async Task Injuries_bans_and_the_next_match_rules_come_back_together()
    {
        Squad(
            new Player { Id = 1, Name = "Fit", Condition = 97, MatchSharpness = 80, AverageRating = 7.1 },
            new Player { Id = 2, Name = "Hurt", MatchSharpness = 60 },
            new Player { Id = 3, Name = "Banned", Condition = 90, MatchSharpness = 75 });
        Readout(
            [Injury(1, "Fit", 0), Injury(2, "Hurt", 1, "A quad muscle strain", 8), Injury(3, "Banned", 0)],
            [new PlayerBan { PlayerId = 3, Name = "Banned", Scope = "National league", MatchesRemaining = 1, Reason = "3 yellow cards" }],
            new NextMatchRules { Competition = "Paulistão", EligibilityRules = ["Registered players only"], MaxSquadRule = "50 players", MaxSquadRuleStatus = "48/50" });

        var availability = (await _availability.GetAvailabilityAsync()).Value;

        Assert.Equal("Santos", availability.TeamName);
        Assert.Equal("Paulistão", availability.NextMatch.Competition);
        Assert.Equal(["Registered players only"], availability.NextMatch.EligibilityRules);
        Assert.Equal("48/50", availability.NextMatch.MaxSquadRuleStatus);

        var injured = Assert.Single(availability.Injured);
        Assert.Equal(("Hurt", "A quad muscle strain", 8), (injured.Name, injured.Injury, injured.ReturnDays));

        var suspended = Assert.Single(availability.Suspended);
        Assert.Equal(("Banned", 1, "3 yellow cards"), (suspended.Name, suspended.MatchesRemaining, suspended.Reason));

        Assert.Equal([1, 2, 3], availability.Players.Select(p => p.Id));
        Assert.True(Player(availability, 1).Available);
        Assert.Equal((97, 80, 7.1), (Player(availability, 1).Condition, Player(availability, 1).MatchSharpness, Player(availability, 1).AverageRating));
        Assert.Equal([Absence.Injured], Player(availability, 2).Absences);
        Assert.Equal([Absence.Suspended], Player(availability, 3).Absences);
    }

    [Fact]
    public async Task An_injury_that_doesnt_keep_him_out_isnt_an_absence()
    {
        Squad(new Player { Id = 1, Name = "Knock", Condition = 92 });
        Readout([Injury(1, "Knock", 0, "A bruised foot", 0)], [], new NextMatchRules());

        var availability = (await _availability.GetAvailabilityAsync()).Value;

        Assert.Empty(availability.Injured);
        Assert.True(Assert.Single(availability.Players).Available);
    }

    [Fact]
    public async Task An_injured_and_suspended_player_has_both_absences()
    {
        Squad(new Player { Id = 1, Name = "Both" });
        Readout([Injury(1, "Both", 1)], [new PlayerBan { PlayerId = 1, Name = "Both", MatchesRemaining = 2 }], new NextMatchRules());

        var availability = (await _availability.GetAvailabilityAsync()).Value;

        Assert.Equal([Absence.Injured, Absence.Suspended], Assert.Single(availability.Players).Absences);
    }

    [Fact]
    public async Task A_ban_of_a_player_outside_the_first_team_is_still_listed()
    {
        Squad(new Player { Id = 1, Name = "Fit" });
        Readout([Injury(1, "Fit", 0)], [new PlayerBan { PlayerId = 9, Name = "Reserve", MatchesRemaining = 1 }], new NextMatchRules());

        var availability = (await _availability.GetAvailabilityAsync()).Value;

        Assert.Equal("Reserve", Assert.Single(availability.Suspended).Name);
        Assert.True(Assert.Single(availability.Players).Available);
    }

    [Fact]
    public async Task The_fitness_read_asks_the_game_for_nothing_more_than_the_squad()
    {
        Squad();
        Readout([], [], new NextMatchRules());

        await _availability.GetAvailabilityAsync();

        await _squad.Received(1).GetSquadAsync(
            Arg.Is<IReadOnlyCollection<AttributeDefinition>>(attributes => attributes.Count == 0), false, false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        Squad();
        _game.GetAvailabilityAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _availability.GetAvailabilityAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Squad(params Player[] players)
    {
        _squad.GetSquadAsync(Arg.Any<IReadOnlyCollection<AttributeDefinition>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new Squad { TeamName = "Santos", Players = [.. players] });
    }

    private void Readout(List<PlayerInjury> injuries, List<PlayerBan> bans, NextMatchRules nextMatch)
    {
        _game.GetAvailabilityAsync(Arg.Any<CancellationToken>())
            .Returns(new AvailabilityReadout { Injuries = injuries, Bans = bans, NextMatch = nextMatch });
    }

    private static PlayerInjury Injury(int id, string name, int availability, string? injury = null, int? returnDays = null)
    {
        return new PlayerInjury { PlayerId = id, Name = name, Availability = availability, Injury = injury, ReturnDays = returnDays };
    }

    private static PlayerAvailability Player(MatchAvailability availability, int id)
    {
        return availability.Players.Single(player => player.Id == id);
    }
}
