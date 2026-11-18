namespace FootballManager.Domain.Board;

/// <summary>The game's budget adjustment panel: a slider moves money between the wage and the transfer budget.</summary>
public class BudgetPanel
{
    /// <summary>Where the slider sits, from 0 (all the money to wages) to 1 (all to transfers), as the panel draws it.</summary>
    public float SliderPosition { get; set; }

    /// <summary>The transfer budget the slider gives, as the panel words it ("R$29.999.999").</summary>
    public string? TransferBudget { get; set; }

    /// <summary>The wage budget the slider gives, as the panel words it ("R$3.486.460 p/s").</summary>
    public string? WageBudget { get; set; }

    /// <summary>The board lets money move from transfers to wages now.</summary>
    public bool CanMoveToWages { get; set; }

    /// <summary>The board lets money move from wages to transfers now.</summary>
    public bool CanMoveToTransfers { get; set; }

    /// <summary>What the board says about the split, as the panel shows it ("A diretoria não vai permitir ...").</summary>
    public string? Board { get; set; }
}
