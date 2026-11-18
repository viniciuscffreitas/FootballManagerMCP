namespace FootballManager.Domain.Board;

/// <summary>The game's budget adjustment panel: one slider moves money between the transfer and the wage budget.</summary>
public class BudgetPanel
{
    /// <summary>The slider's position, on the game's own scale.</summary>
    public float SliderValue { get; set; }

    /// <summary>The transfer budget the slider gives, as the game words it ("R$30 MI").</summary>
    public string? TransferBudget { get; set; }

    /// <summary>The wage budget the slider gives, as the game words it ("R$52.5 mil p/s").</summary>
    public string? WageBudget { get; set; }

    public bool CanEdit { get; set; }
    public bool CanEditUp { get; set; }
    public bool CanEditDown { get; set; }

    /// <summary>What the board says about the split, as the panel shows it.</summary>
    public string? Board { get; set; }
}
