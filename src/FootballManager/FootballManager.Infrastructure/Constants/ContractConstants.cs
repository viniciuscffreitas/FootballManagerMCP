namespace FootballManager.Infrastructure.Constants;

// A first-team player's contract, as the game's contract screen reads it. Money comes as the game writes it
// (DisplayValue, in the save's currency); the numbers behind it (SortValue) are in the game's base currency, not the
// user's, so they aren't read.
internal static class ContractConstants
{
    public const string Wage = "FullContract.Wage";
    // yyyy-MM-dd; the game dates a contract without an end 1900.
    public const string EndDate = "FullContract.EndDate";
    public const string HasEndDate = "FullContract.HasEndDate";
    // The game's own spelling of the property.
    public const string TimeRemaining = "FullContract.ContractTimeRemainingDecription";
    public const string Type = "FullContract.ContractType";
    public const string AgreedPlayingTime = "FullContract.AgreedPlayingTime";
    // "-" with 0 behind it when the contract has no release clause.
    public const string ReleaseClause = "FullContract.MinimumFeeReleaseClauseValue";
    public const string ClausesCount = "FullContract.ContractClausesCount";
    public const string HasRenewalOffer = "FullContract.HasRenewalOffer";
    public const string StaffAdvice = "ContractRenewalDecisionStaffAdviceString";

    public static readonly string[] Properties =
    [
        GameConstants.UniqueId, GameConstants.Name, Wage, EndDate, HasEndDate, TimeRemaining, Type, AgreedPlayingTime,
        ReleaseClause, ClausesCount, HasRenewalOffer, StaffAdvice
    ];
}
