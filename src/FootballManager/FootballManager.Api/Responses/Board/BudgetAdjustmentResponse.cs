namespace FootballManager.Api.Responses.Board;

public class BudgetAdjustmentResponse
{
    public bool Applied { get; set; }
    public string? Budgets { get; set; }
    public int? Steps { get; set; }
    public BudgetPanelResponse? Before { get; set; }
    public BudgetPanelResponse? After { get; set; }
    public List<string> Notes { get; set; } = [];
}
