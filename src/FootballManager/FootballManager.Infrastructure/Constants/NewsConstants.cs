namespace FootballManager.Infrastructure.Constants;

// The portal's news beyond the inbox, on the user's manager with his manager and team as contexts: the club's news
// takes the club's main team, the league's news the main team's league. Both lists need a Number context, which
// doesn't cap them (Number 3 still lists all of the club's 66 stories); without it the game doesn't send the list.
internal static class NewsConstants
{
    public const string TeamNews = "HumanTeamOtherNews";
    public const string TeamContext = "NewsContextTeam";
    public const string Team = TeamConstants.OwnTeam;

    public const string LeagueNews = "HumanCompetitionOtherNews";
    public const string LeagueContext = "Competition";
    public const string League = CompetitionConstants.UserLeague;

    public const string Number = "Number";
    public const int AnyNumber = 3;

    // On a story, the same properties as on an inbox message.
    public static readonly string[] StoryProperties =
        [GameConstants.UniqueId, InboxConstants.Date, InboxConstants.Title, InboxConstants.Subtitle, InboxConstants.Source];
}
