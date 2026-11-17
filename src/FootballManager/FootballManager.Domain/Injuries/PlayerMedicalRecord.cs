namespace FootballManager.Domain.Injuries;

/// <summary>A first-team player's medical record: his injury history and his injury risk now.</summary>
public class PlayerMedicalRecord
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public int InjuriesThisSeason { get; set; }

    /// <summary>The game's injury risk score (InjuryRiskLevel); higher is riskier.</summary>
    public int? RiskLevel { get; set; }

    /// <summary>The medical team's take on his risk, in the game's words.</summary>
    public string? RiskOverview { get; set; }

    /// <summary>What raises his risk, e.g. "Baixa Condição Física"; null when nothing does.</summary>
    public string? RiskReason { get; set; }

    /// <summary>Oldest first, as the game lists it.</summary>
    public List<PastInjury> History { get; set; } = [];
}
