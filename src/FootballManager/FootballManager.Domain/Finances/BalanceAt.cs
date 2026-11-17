namespace FootballManager.Domain.Finances;

/// <summary>The club's balance at a point in time, in the game's unit.</summary>
public class BalanceAt
{
    /// <summary>When: a month ("2026-01-01") or the game's title for a season end ("Fim da temporada 2026").</summary>
    public required string When { get; set; }

    public long Amount { get; set; }
}
