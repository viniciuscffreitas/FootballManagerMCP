using System.Text.Json;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Tests;

public class PerformanceMappingTests
{
    private readonly TypeAdapterConfig _config = new();

    public PerformanceMappingTests()
    {
        new MappingConfig().Register(_config);
    }

    [Fact]
    public void A_scouted_player_reads_his_league_and_numbers_in_both_shapes_the_game_sends()
    {
        var player = Read("""
            {
              "UniqueId": 19046041, "Name": "Lucas Veríssimo",
              "Division.Name": "Série A",
              "ClubAppearancesThisSeason": 15,
              "Minutes": { "PropertyValue": 1350, "Count": 15 },
              "ClubAverageRatingThisSeason": 7.070000171661377,
              "HeaderCompletionPercentage": { "PropertyValue": 71.4, "Count": 15 },
              "ProgressivePassesPer90": { "PropertyValue": 0.53, "Count": 15 }
            }
            """);

        Assert.Equal("Série A", player.League);
        Assert.Equal(15, player.Performance!.Appearances);
        Assert.Equal(1350, player.Performance.Minutes);
        Assert.Equal(7.07, player.Performance.AverageRating!.Value, 2);
        Assert.Equal(71.4, player.Performance.HeaderCompletionPercentage);
        Assert.Equal(0.53, player.Performance.ProgressivePassesPer90);
        Assert.Null(player.Performance.Goals);
    }

    [Fact]
    public void Without_his_numbers_asked_for_a_scouted_player_has_none()
    {
        var player = Read("""{ "UniqueId": 1, "Name": "Rony" }""");

        Assert.Null(player.Performance);
        Assert.Null(player.League);
    }

    private ScoutedPlayer Read(string json)
    {
        return JsonSerializer.Deserialize<BridgeScoutedPlayerDto>(json)!.Adapt<ScoutedPlayer>(_config);
    }
}
