using FootballManager.Application.Interfaces;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Files;
using FootballManager.Infrastructure.Mappings;
using FootballManager.Infrastructure.Settings;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        InfrastructureSettings settings)
    {
        services.AddSingleton(settings);

        services.AddHttpClient(GameBridge.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(settings.GameBridgeBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

            if (!string.IsNullOrWhiteSpace(settings.GameBridgeHostHeader))
            {
                client.DefaultRequestHeaders.Host = settings.GameBridgeHostHeader;
            }
        });

        services.AddScoped<GameBridge>();
        services.AddSingleton<RemovedFocusLog>();
        services.AddSingleton<SearchPositions>();
        services.AddSingleton<SearchedRecords>();
        services.AddSingleton<CompetitionPlaces>();
        services.AddScoped<GameVocabulary>();
        services.AddScoped<TeamInstructionReader>();
        services.AddScoped<ISquadGameClient, SquadBridgeClient>();
        services.AddScoped<IAvailabilityGameClient, AvailabilityBridgeClient>();
        services.AddScoped<ILeagueTableGameClient, LeagueTableBridgeClient>();
        services.AddScoped<IFinancesGameClient, FinancesBridgeClient>();
        services.AddScoped<IBoardGameClient, BoardBridgeClient>();
        services.AddScoped<IStaffGameClient, StaffBridgeClient>();
        services.AddScoped<ITacticsGameClient, TacticsBridgeClient>();
        services.AddScoped<ITacticEditorGameClient, TacticEditorBridgeClient>();
        services.AddScoped<IMatchesGameClient, MatchesBridgeClient>();
        services.AddScoped<TeamsBridgeClient>();
        services.AddScoped<ITeamsGameClient>(provider => provider.GetRequiredService<TeamsBridgeClient>());
        services.AddScoped<IScoutingGameClient, ScoutingBridgeClient>();
        services.AddScoped<IScoutingEditorGameClient, ScoutingEditorBridgeClient>();
        services.AddScoped<IPlayerSearchGameClient, PlayerSearchBridgeClient>();
        services.AddScoped<IInboxGameClient, InboxBridgeClient>();
        services.AddScoped<ITrainingGameClient, TrainingBridgeClient>();
        services.AddScoped<ISaveSessionGameClient, SaveSessionBridgeClient>();
        services.AddScoped<IConversationGameClient, ConversationBridgeClient>();
        services.AddScoped<ITransferGameClient, TransferBridgeClient>();
        services.AddSingleton<ISaveFiles, LocalSaveFiles>();
        services.AddScoped<ICompetitionsGameClient, CompetitionsBridgeClient>();
        services.AddScoped<DatabaseSearchBridgeClient>();
        services.AddScoped<IDatabaseSearchGameClient>(provider => provider.GetRequiredService<DatabaseSearchBridgeClient>());

        var mappingConfig = new MappingConfig();
        mappingConfig.Register(TypeAdapterConfig.GlobalSettings);

        return services;
    }
}
