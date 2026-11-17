namespace FootballManager.Infrastructure.Constants;

// Who can play the next match, as the game's team selection screen reads it. The game's own "unavailable" and
// "ineligible" reasons (UnavailableReason, IneligibilityReason, UnregisteredPlayers) never answer through the plugin,
// so injuries, bans and the competition's rules are read from their own records instead.
internal static class AvailabilityConstants
{
    // A player's injury record; InjuryAvailability is 0 when it doesn't keep him out, 1 when it does.
    public const string InjuryAvailability = "CurrentInjury.InjuryAvailability";
    public const string InjuryDescription = "CurrentInjury.InjuryDescription";
    public const string InjuryExpectedReturn = "CurrentInjury.InjuryDurationLong";
    public const string InjuryReturnDays = "CurrentInjury.InjuryReturnDays";

    public static readonly string[] InjuryProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, InjuryAvailability, InjuryDescription, InjuryExpectedReturn, InjuryReturnDays
    ];

    // Every ban the team's players serve, whatever the competition; each item names its player.
    public const string Bans = TeamConstants.OwnTeam + ".AllCompsActiveBans";
    public const string BanPlayerId = "player.UniqueId";
    public const string BanPlayerName = "player.Name";
    public const string BanScope = "BanScope.BanScopeString";
    public const string BanMatchesRemaining = "ActiveBanMatchesRemaining";
    public const string BanDaysRemaining = "ActiveBanDaysRemaining";
    public const string BanReason = "ActiveBanReason";

    public static readonly string[] BanProperties =
    [
        BanPlayerId, BanPlayerName, BanScope, BanMatchesRemaining, BanDaysRemaining, BanReason
    ];

    // The game sends -1 for the count a ban isn't measured in (days for a match ban, matches for a day ban).
    public const int NotCounted = -1;

    // The next match's squad rules, read on the team.
    public const string NextCompetition = "NextFixture.Competition.Name";
    public const string EligibilityRule = "NextMatchEligibilityRule";
    public const string MaxSquadRule = "NextMatchMaxSquadRule";
    public const string MaxSquadRuleStatus = "NextMatchMaxSquadRuleStatus";

    public static readonly string[] NextMatchProperties = [NextCompetition, EligibilityRule, MaxSquadRule, MaxSquadRuleStatus];
}
