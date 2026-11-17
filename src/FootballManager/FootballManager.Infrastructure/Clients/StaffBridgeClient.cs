using FootballManager.Application.Interfaces;
using FootballManager.Domain.Staff;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Staff;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the first team's staff through the MCPFM plugin: each member's profile and contract, the team's staff counts
// and the club's open jobs, the three in parallel.
internal sealed class StaffBridgeClient(GameBridge bridge) : IStaffGameClient
{
    public async Task<TeamStaff> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        var members = bridge.GetListAsync<BridgeStaffMemberDto>(
            BridgeQuery.FromHuman().List(StaffConstants.Members).WithHumanAndTeam()
                .Context(StaffConstants.JobType, StaffConstants.ProfileJobType).Props(StaffConstants.MemberProperties),
            cancellationToken);
        var counts = bridge.GetRecordAsync<BridgeStaffCountsDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).WithHumanAndTeam().Props(StaffConstants.TeamProperties),
            cancellationToken);
        var vacancies = bridge.GetRecordAsync<BridgeStaffVacanciesDto>(
            BridgeQuery.FromHuman().Base(StaffConstants.Club).WithHumanAndTeam().Props([StaffConstants.Vacancies]),
            cancellationToken);

        await Task.WhenAll(members, counts, vacancies);

        var staff = (await counts).Values.Adapt<TeamStaff>();
        staff.Members = (await members).Items.Select(item => item.Values).ToList().Adapt<List<StaffMember>>();
        staff.Vacancies = (await vacancies).Values.Vacancies;
        return staff;
    }
}
