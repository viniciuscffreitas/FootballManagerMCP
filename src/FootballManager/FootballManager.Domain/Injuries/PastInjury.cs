namespace FootballManager.Domain.Injuries;

/// <summary>One injury in a player's medical history, as the game's medical centre lists it.</summary>
public class PastInjury
{
    public DateOnly? Date { get; set; }

    /// <summary>The injury as the game names it, e.g. "A torn ACL".</summary>
    public string? Injury { get; set; }

    /// <summary>How long it kept him out, as the game writes it, e.g. "18 meses".</summary>
    public string? Duration { get; set; }

    public int? DurationDays { get; set; }
    public InjurySeverity? Severity { get; set; }

    /// <summary>How he got it, e.g. "Choque com jogador".</summary>
    public string? Cause { get; set; }

    /// <summary>Where he got it, e.g. "Em treino" or "Em jogo".</summary>
    public string? Context { get; set; }

    /// <summary>The injury he has now.</summary>
    public bool IsActive { get; set; }
}
