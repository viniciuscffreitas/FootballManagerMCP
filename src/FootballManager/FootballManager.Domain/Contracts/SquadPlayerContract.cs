namespace FootballManager.Domain.Contracts;

/// <summary>
/// A first-team player's contract with the user's club, as the game's contract screen shows it. Texts and money are in
/// the game's display language and currency.
/// </summary>
public class SquadPlayerContract
{
    /// <summary>FM's unique id for the person.</summary>
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>His wage as the game writes it, with its period, e.g. "R$80 mil p/s".</summary>
    public string? Wage { get; set; }

    /// <summary>The day the contract ends; null when it has no end date.</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>The time left as the game writes it, e.g. "2 anos, 291 dias".</summary>
    public string? TimeRemaining { get; set; }

    /// <summary>The kind of contract, e.g. "Contrato Profissional".</summary>
    public string? Type { get; set; }

    /// <summary>The playing time promised in it, e.g. "Goleiro titular".</summary>
    public string? AgreedPlayingTime { get; set; }

    /// <summary>The minimum fee release clause as the game writes it; null when the contract has none.</summary>
    public string? ReleaseClause { get; set; }

    /// <summary>How many clauses the contract has.</summary>
    public int ClausesCount { get; set; }

    /// <summary>Whether the club has a renewal offer out to him.</summary>
    public bool HasRenewalOffer { get; set; }

    /// <summary>The staff's advice on renewing, e.g. "Não oferecer contrato".</summary>
    public string? StaffAdvice { get; set; }
}
