namespace FootballManager.Domain.Finances;

/// <summary>The club's balance at a point in time, in the currency the game's screens show.</summary>
public class BalanceEntry
{
    /// <summary>When: a month ("2026-01-01") or the game's title for a season end ("Fim da temporada 2026").</summary>
    public required string When { get; set; }

    public required string Balance { get; set; }
}
