namespace FootballManager.Api.Responses.Board;

public class BudgetPanelResponse
{
    public float SliderPosition { get; set; }
    public string? TransferBudget { get; set; }
    public string? WageBudget { get; set; }
    public bool CanMoveToWages { get; set; }
    public bool CanMoveToTransfers { get; set; }
    public string? Board { get; set; }
}
