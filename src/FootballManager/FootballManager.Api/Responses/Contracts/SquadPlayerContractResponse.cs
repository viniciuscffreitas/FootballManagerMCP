namespace FootballManager.Api.Responses.Contracts;

public class SquadPlayerContractResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public string? Wage { get; set; }

    /// <summary>yyyy-MM-dd; null when the contract has no end date.</summary>
    public DateOnly? EndDate { get; set; }
    public string? TimeRemaining { get; set; }
    public string? Type { get; set; }
    public string? AgreedPlayingTime { get; set; }

    /// <summary>Null when the contract has no minimum fee release clause.</summary>
    public string? ReleaseClause { get; set; }
    public int ClausesCount { get; set; }
    public bool HasRenewalOffer { get; set; }
    public string? StaffAdvice { get; set; }
}
