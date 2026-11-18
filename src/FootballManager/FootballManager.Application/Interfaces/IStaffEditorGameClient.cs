using FootballManager.Domain.Scouting;
using FootballManager.Domain.Staff;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Staff changes through the game's own menus: on a first-team staff member, or on the club for job adverts
/// (<c>staffId</c> null). Throws <see cref="Exceptions.GameUnavailableException"/> when the game can't answer and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface IStaffEditorGameClient
{
    /// <summary>The first-team staff member with that FM id; null when the first team's staff has no one with it.</summary>
    Task<StaffMember?> FindMemberAsync(int staffId, CancellationToken cancellationToken = default);

    /// <summary>The staff actions the game's menu offers now on the staff member, or on the club.</summary>
    Task<HashSet<StaffMenuAction>> GetOfferedActionsAsync(int? staffId, CancellationToken cancellationToken = default);

    /// <summary>The action's entries; null when the menu doesn't offer it now.</summary>
    Task<List<MenuEntry>?> GetMenuAsync(int? staffId, StaffMenuAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Picks the entry at <paramref name="entry"/> (an index per level, from the top), as the menu's click does. Throws
    /// <see cref="Exceptions.GameRejectedException"/>, sending nothing, when the menu read now no longer has
    /// <paramref name="labels"/> (the label at each level) there, or greys the entry out.
    /// </summary>
    Task RunMenuEntryAsync(
        int? staffId, StaffMenuAction action, IReadOnlyList<int> entry, IReadOnlyList<string> labels,
        CancellationToken cancellationToken = default);
}
