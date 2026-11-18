using FootballManager.Domain.Board;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Dealings with the board through the game's own club menus and the talks they open. Throws
/// <see cref="Exceptions.GameUnavailableException"/> when the game can't answer and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface IBoardEditorGameClient
{
    /// <summary>The requests the club's menu offers now.</summary>
    Task<HashSet<BoardMenuAction>> GetOfferedActionsAsync(CancellationToken cancellationToken = default);

    /// <summary>The action's entries; null when the menu doesn't offer it now.</summary>
    Task<List<MenuEntry>?> GetMenuAsync(BoardMenuAction action, CancellationToken cancellationToken = default);

    /// <summary>Picks the action's entry at <paramref name="entry"/>, as the menu's click does.</summary>
    Task RunMenuEntryAsync(BoardMenuAction action, int entry, CancellationToken cancellationToken = default);

    /// <summary>The talk with the board open on screen; null when none is.</summary>
    Task<BoardCard?> ReadCardAsync(CancellationToken cancellationToken = default);

    /// <summary>Says the card's line at <paramref name="line"/> and waits for the answer; null when the talk closed.</summary>
    Task<BoardCard?> SayAsync(int line, CancellationToken cancellationToken = default);

    /// <summary>The two budgets as the club's menu sums them up; null when the menu offers no budget adjustment now.</summary>
    Task<string?> GetBudgetsAsync(CancellationToken cancellationToken = default);

    /// <summary>Opens the budget adjustment panel (if it isn't open) and reads it; null when it didn't open.</summary>
    Task<BudgetPanel?> OpenBudgetPanelAsync(CancellationToken cancellationToken = default);

    /// <summary>Moves the panel's slider, confirms the split and reads the panel back.</summary>
    Task<BudgetPanel?> SetBudgetSliderAsync(float value, CancellationToken cancellationToken = default);
}
