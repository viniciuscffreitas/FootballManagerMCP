namespace FootballManager.Domain.Registration;

/// <summary>One competition's registration: its window, its rules and who is registered for it.</summary>
public class CompetitionRegistration
{
    /// <summary>The registration's place in the team's list, as the game orders it.</summary>
    public int Index { get; set; }

    /// <summary>The registration's name, e.g. "Copa Betano do Brasil".</summary>
    public required string Name { get; set; }

    /// <summary>The competition's short name, e.g. "Copa do Brasil".</summary>
    public string? Competition { get; set; }

    public int? CompetitionId { get; set; }

    /// <summary>Whether the window is open now: registrations change only then.</summary>
    public bool WindowOpen { get; set; }

    /// <summary>The game's words for the window, e.g. "Faltam 98 dias" or "Reabre em 77 dias".</summary>
    public string? Window { get; set; }

    /// <summary>yyyy-MM-dd: the window's next deadline.</summary>
    public string? Deadline { get; set; }

    public int? MaxSquadSize { get; set; }

    /// <summary>Whether the registration meets all its rules now.</summary>
    public bool MeetsRules { get; set; }

    /// <summary>The game's summary of the rules, e.g. "A seleção do elenco está em total conformidade".</summary>
    public string? RulesSummary { get; set; }

    /// <summary>The game's extra notes on the registration (open places, what to do when a minimum can't be met).</summary>
    public string? Notes { get; set; }

    public List<RegistrationRule> Rules { get; set; } = [];
    public List<RegistrationPlayer> Registered { get; set; } = [];

    /// <summary>Players the game can register but hasn't.</summary>
    public List<RegistrationPlayer> Unregistered { get; set; } = [];
}
