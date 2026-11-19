using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Changes the user's scouting in the running game the way its screens do: the recruitment focus wizard, and the
/// menu the game offers on a player. Throws <see cref="Exceptions.GameUnavailableException"/> when the game can't be
/// reached, and <see cref="Exceptions.GameRejectedException"/> when it refuses or doesn't take a change.
/// </summary>
public interface IScoutingEditorGameClient
{
    /// <summary>Every area a recruitment focus can cover.</summary>
    Task<List<ScoutingArea>> GetAreasAsync(CancellationToken cancellationToken = default);

    /// <summary>The ages and positions a new focus can take.</summary>
    Task<FocusChoices> GetFocusChoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the focus through the game's wizard and returns it as the game lists it, or null when the game created it
    /// (its count of focuses grew) but its list doesn't show it yet.
    /// </summary>
    Task<RecruitmentFocus?> CreateFocusAsync(FocusDraft draft, CancellationToken cancellationToken = default);

    /// <summary>Removes the focus (one of <see cref="IScoutingGameClient.GetFocusesAsync"/>'s).</summary>
    Task RemoveFocusAsync(int focusId, CancellationToken cancellationToken = default);

    /// <summary>Makes a new shortlist with this name, as the game's "new shortlist" dialog does.</summary>
    Task CreateShortlistAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// A player from the club's own lists (its squad, its youth team, its players out on loan) or one of the user's
    /// scouting lists (the scouting centre's recommendations, the shortlist, a focus's players), or null when none holds him.
    /// </summary>
    Task<ScoutedPlayer?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The players of these ids that the player search's listed results, the scouting lists, the club's own squad
    /// (trialists included), youth team and players out on loan, or the game's search box hold, by id; an id none holds is left out. Unlike <see cref="FindPlayerAsync"/>, it doesn't read the whole
    /// player search: only the places its listed results were seen at.
    /// </summary>
    Task<Dictionary<int, ScoutedPlayer>> FindPlayersAsync(IReadOnlyCollection<int> playerIds, CancellationToken cancellationToken = default);

    /// <summary>The menu actions of <see cref="PlayerMenuAction"/> the game offers for the player now.</summary>
    Task<HashSet<PlayerMenuAction>> GetOfferedActionsAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// A team's squad, by the team's FM id (a team of the user's competitions, or a club the game's search has listed),
    /// with each player made reachable for <see cref="GetMenuAsync"/>; null for a team neither has listed.
    /// </summary>
    Task<TeamPlayers?> GetTeamPlayersAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>The player's menu for <paramref name="action"/>, or null when the game doesn't offer it for him.</summary>
    Task<List<MenuEntry>?> GetMenuAsync(int playerId, PlayerMenuAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Picks the menu entry at <paramref name="entry"/> (an index per level, from the top) and returns the player as
    /// the game then holds him.
    /// </summary>
    Task<ScoutedPlayer?> RunMenuEntryAsync(
        int playerId, PlayerMenuAction action, IReadOnlyList<int> entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the menu entry at <paramref name="entry"/> (an index per level, from the top) only if the menu, read again
    /// right before the send, still has it there with <paramref name="labels"/> at each level and greys out none of them;
    /// otherwise it throws <see cref="Exceptions.GameRejectedException"/> and sends nothing. A send whose answer is lost
    /// throws <see cref="Exceptions.GameAnswerLostException"/>: the game may have taken it. The player is looked for again
    /// on the next read: the entry may move him to another list (a recalled player is no longer out on loan).
    /// </summary>
    Task SendMenuEntryAsync(
        int playerId, PlayerMenuAction action, IReadOnlyList<int> entry, IReadOnlyList<string> labels,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the panel the menu's first entry for <paramref name="action"/> opens (the game's NewsAction
    /// <paramref name="newsAction"/>, e.g. 6 for the transfer offer panel), as a click on it does.
    /// </summary>
    Task OpenPanelAsync(int playerId, PlayerMenuAction action, int newsAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the menu's first entry for <paramref name="action"/> the way a click on the menu does, for an entry that
    /// opens a panel with no NewsAction of its own (e.g. a contract negotiation with one of the club's players).
    /// </summary>
    Task RunMenuClickAsync(int playerId, PlayerMenuAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Picks the menu entry at <paramref name="entry"/> (an index per level, from the top) as a click on the menu does.
    /// The player's menu is read again first: the action must still be offered, and the entry at that place must have
    /// <paramref name="labels"/> at every level, be greyed out at none, and send an event the client knows. Otherwise,
    /// or while a dialog is already open in the game, it throws <see cref="Exceptions.GameRejectedException"/> and sends
    /// nothing. With <paramref name="confirmation"/> (the terms approved in the dry run, e.g. a release's cost), the
    /// entry's confirmation must also still be it (case and white space aside), or it throws and sends nothing.
    /// A send the game didn't answer throws <see cref="Exceptions.GameAnswerLostException"/>.
    /// </summary>
    Task ClickMenuEntryAsync(
        int playerId, PlayerMenuAction action, IReadOnlyList<int> entry, IReadOnlyList<string> labels,
        string? confirmation = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for the game's confirmation dialog and answers "Yes" to it only when its text is
    /// <paramref name="confirmation"/> or holds the whole of it (case and white space aside): the dialog of the action
    /// asked for, not another one. An empty confirmation never answers a dialog: it only reports the one that showed.
    /// </summary>
    Task<DialogAnswer> AnswerDialogAsync(string confirmation, CancellationToken cancellationToken = default);
}
