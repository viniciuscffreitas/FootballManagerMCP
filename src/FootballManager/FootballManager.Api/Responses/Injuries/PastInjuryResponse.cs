using FootballManager.Domain.Injuries;

namespace FootballManager.Api.Responses.Injuries;

public class PastInjuryResponse
{
    public DateOnly? Date { get; set; }
    public string? Injury { get; set; }

    /// <summary>As the game writes it, e.g. "18 meses".</summary>
    public string? Duration { get; set; }

    public int? DurationDays { get; set; }
    public InjurySeverity? Severity { get; set; }
    public string? Cause { get; set; }

    /// <summary>Where he got it, e.g. "Em treino".</summary>
    public string? Context { get; set; }

    public bool IsActive { get; set; }
}
