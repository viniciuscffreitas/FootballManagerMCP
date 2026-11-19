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
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ISquadNumbersService, SquadNumbersService>();
        services.AddScoped<IStaffEditingService, StaffEditingService>();
        services.AddScoped<IContractsService, ContractsService>();
        services.AddScoped<ISquadDynamicsService, SquadDynamicsService>();
        services.AddScoped<IInjuriesService, InjuriesService>();
        services.AddScoped<IYouthService, YouthService>();
        services.AddScoped<ITransferActivityService, TransferActivityService>();
        services.AddScoped<IRotationService, RotationService>();
        services.AddScoped<ITacticsService, TacticsService>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ISaveGuard, SaveGuard>();
        services.AddScoped<ITacticEditingService, TacticEditingService>();
        services.AddScoped<IMatchesService, MatchesService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IOppositionService, OppositionService>();
        services.AddScoped<IOppositionInstructionsService, OppositionInstructionsService>();
        services.AddScoped<IScoutingService, ScoutingService>();
        services.AddScoped<IScoutingEditingService, ScoutingEditingService>();
        services.AddScoped<IPlayerSearchService, PlayerSearchService>();
        services.AddScoped<IInboxService, InboxService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IDatabaseSearchService, DatabaseSearchService>();
        services.AddScoped<ITrialsService, TrialsService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IPlayerExitService, PlayerExitService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<ICompetitionsService, CompetitionsService>();
        services.AddScoped<ILeagueTableService, LeagueTableService>();
        services.AddScoped<ISetPiecesService, SetPiecesService>();
        return services;
    }
}
