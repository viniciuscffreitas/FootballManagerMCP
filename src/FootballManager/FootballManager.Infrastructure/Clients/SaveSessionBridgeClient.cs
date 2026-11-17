using System.Globalization;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Saves;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Saves;
using FootballManager.Infrastructure.Dtos.Teams;

namespace FootballManager.Infrastructure.Clients;

// The open save as the plugin reports it (/session, by the game's last-save record), with the user's club and the
// game's date read from the save itself once it's loaded.
internal sealed class SaveSessionBridgeClient(GameBridge bridge) : ISaveSessionGameClient
{
    public async Task<LoadedSave> GetLoadedSaveAsync(CancellationToken cancellationToken = default)
    {
        var session = await bridge.GetJsonAsync<BridgeSessionDto>(SaveConstants.SessionPath, cancellationToken);
        var save = new LoadedSave
        {
            Loaded = session.Loaded,
            Loading = session.Loading,
            Name = session.LoadedSave,
            IsTestSave = session.IsInvestigationSave
        };

        if (!save.Loaded)
        {
            return save;
        }

        // The club's main team, not Human.Team, which reading another team switches for a moment.
        var club = bridge.GetRecordAsync<BridgeTeamDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).Props([GameConstants.Name]), cancellationToken);
        var date = bridge.GetRecordAsync<BridgeGameDateDto>(
            BridgeQuery.FromGame().Props([SaveConstants.CurrentGameDate]), cancellationToken);

        await Task.WhenAll(club, date);

        save.Club = (await club).Values.Name;
        save.GameDate = ParseDate((await date).Values.CurrentGameDate);
        return save;
    }

    public async Task<string?> GetGameVersionAsync(CancellationToken cancellationToken = default)
    {
        var status = await bridge.GetTextAsync(SaveConstants.StatusPath, cancellationToken);

        var product = status.Split('\n')
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.StartsWith(SaveConstants.ProductLine, StringComparison.Ordinal));

        return product?.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() is { } build
               && build != SaveConstants.ProductLine
            ? build
            : null;
    }

    private static DateOnly? ParseDate(string? text)
    {
        return text is { Length: >= 10 }
               && DateOnly.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }
}
