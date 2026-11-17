namespace FootballManager.Api.Responses.Youth;

public class YouthPlayerResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Positions { get; set; }
    public string? Morale { get; set; }
    public string? ContractEndDate { get; set; }
    public bool OnLoan { get; set; }
    public string? LoanClub { get; set; }
}
