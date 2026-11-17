namespace FootballManager.Domain.Transfers;

/// <summary>The contract negotiation panel's own buttons, in the order the negotiation goes through them.</summary>
public enum ContractStep
{
    /// <summary>Agree to the promises the player's agent asked for ("Finalizar Promessas").</summary>
    FinalisePromises,

    /// <summary>Go on from the promises to the contract terms ("Negociar Contrato", the panel's, not Continue's).</summary>
    NegotiateContract,

    /// <summary>Offer the contract at the terms the panel shows ("Fechar Acordo").</summary>
    CloseDeal
}
