using FootballManager.Domain.Availability;

namespace FootballManager.Api.Responses.Availability;

public class PlayerAvailabilityResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Positions { get; set; }
    public string? BestRole { get; set; }
    public int? Condition { get; set; }
    public int? MatchSharpness { get; set; }
    public double? AverageRating { get; set; }
    public bool Available { get; set; }

    /// <summary>Why he can't be picked; null when he can.</summary>
    public List<Absence>? Absences { get; set; }
}
