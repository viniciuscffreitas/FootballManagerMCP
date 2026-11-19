namespace FootballManager.Api.Requests.Board;

public class BudgetAdjustmentRequest
{
    /// <summary>Steps of the game's own increment, positive from the transfer to the wage budget; left out, applying only opens and reads the panel.</summary>
    public int? Steps { get; set; }
}
