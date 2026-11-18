namespace FootballManager.Api.Responses.Board;

public class BudgetPanelResponse
{
    public float SliderValue { get; set; }
    public string? TransferBudget { get; set; }
    public string? WageBudget { get; set; }
    public bool CanEdit { get; set; }
    public bool CanEditUp { get; set; }
    public bool CanEditDown { get; set; }
    public string? Board { get; set; }
}
