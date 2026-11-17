namespace FootballManager.Domain.Transfers;

/// <summary>A line of a negotiation panel: its label and what it's set to, e.g. "Salário": "R$65.000 p/s | 65000".</summary>
public class NegotiationTerm
{
    public required string Label { get; set; }
    public string? Value { get; set; }
}
