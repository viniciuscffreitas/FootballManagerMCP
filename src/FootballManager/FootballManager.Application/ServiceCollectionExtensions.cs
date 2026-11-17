using FootballManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FootballManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISquadService, SquadService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IFinancesService, FinancesService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IContractsService, ContractsService>();
        services.AddScoped<ISquadDynamicsService, SquadDynamicsService>();
        services.AddScoped<IInjuriesService, InjuriesService>();
        services.AddScoped<IRotationService, RotationService>();
        services.AddScoped<ITacticsService, TacticsService>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ISaveGuard, SaveGuard>();
        services.AddScoped<ITacticEditingService, TacticEditingService>();
        services.AddScoped<IMatchesService, MatchesService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IScoutingService, ScoutingService>();
        services.AddScoped<IScoutingEditingService, ScoutingEditingService>();
        services.AddScoped<IPlayerSearchService, PlayerSearchService>();
        services.AddScoped<IInboxService, InboxService>();
        services.AddScoped<IDatabaseSearchService, DatabaseSearchService>();
        services.AddScoped<ITrialsService, TrialsService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<ICompetitionsService, CompetitionsService>();
        services.AddScoped<ILeagueTableService, LeagueTableService>();
        return services;
    }
}
