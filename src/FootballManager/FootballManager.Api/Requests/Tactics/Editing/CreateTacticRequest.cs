namespace FootballManager.Api.Requests.Tactics.Editing;

/// <summary>A tactic to create in an empty slot, by key (get_tactic_options lists them).</summary>
public class CreateTacticRequest
{
    /// <summary>The empty slot to create it in, 1 to 3. Creating makes it the slot the team plays.</summary>
    public int Slot { get; set; }

    public string? Style { get; set; }

    public ByPhaseRequest<string>? Formation { get; set; }
}
