namespace FootballManager.Infrastructure.Constants;

// Where the open save, the game's date and its version come from.
internal static class SaveConstants
{
    public const string SessionPath = "/session";
    public const string StatusPath = "/status";

    // /status is text, a line per fact: "product: Football Manager 27 27.0.1.2494769", the build last.
    public const string ProductLine = "product:";

    // On the game root.
    public const string CurrentGameDate = "CurrentGameDate";
}
