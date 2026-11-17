using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;

namespace FootballManager.Infrastructure.Dtos.Youth;

/// <summary>A player out on loan, keyed by the game's property paths.</summary>
internal sealed class BridgeLoanedPlayerDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(GameConstants.Age)]
    public int? Age { get; set; }

    [JsonPropertyName(YouthConstants.LoanClub)]
    public string? Club { get; set; }

    [JsonPropertyName(YouthConstants.LoanEndDate)]
    public string? LoanEndDate { get; set; }

    [JsonPropertyName(YouthConstants.LoanMonthlyFee)]
    public string? MonthlyFee { get; set; }

    [JsonPropertyName(YouthConstants.LoanAppearances)]
    public int? Appearances { get; set; }

    [JsonPropertyName(YouthConstants.LoanAverageRating)]
    public double? AverageRating { get; set; }

    [JsonPropertyName(YouthConstants.LoanHappiness)]
    public int? Happiness { get; set; }

    // Each comment as a table cell: its text and the number it sorts by.
    [JsonPropertyName(YouthConstants.LoanComments)]
    public List<BridgeSortedValueDto>? Comments { get; set; }
}
