using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Staff;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class StaffServiceTests
{
    private readonly IStaffGameClient _game = Substitute.For<IStaffGameClient>();
    private readonly StaffService _staff;

    public StaffServiceTests()
    {
        _staff = new StaffService(_game);
    }

    [Fact]
    public async Task Each_staff_member_comes_back_with_job_attributes_and_contract_and_the_counts_with_them()
    {
        _game.GetStaffAsync(Arg.Any<CancellationToken>()).Returns(new TeamStaff
        {
            TeamName = "Santos",
            AllCount = 28,
            CoachingCount = 12,
            MedicalCount = 4,
            Analyst = "Gustavo Nicoline",
            Vacancies = 18,
            Members =
            [
                new StaffMember
                {
                    Id = 1, Name = "Eudes Pedro", Age = 49, Job = "Treinador", CoachingStyle = "Tático",
                    CoachingAttributes = [Attribute("Tático", 13, "Bom"), Attribute("Defensivo", 11, "Mediano")],
                    Wage = "R$20 mil p/s", ContractEnd = "2027-12-31"
                },
                new StaffMember
                {
                    Id = 2, Name = "César Sampaio", Job = "Diretor técnico",
                    MedicalAttributes = [Attribute("Fisioterapia", 4, "Razoável")]
                }
            ]
        });

        var staff = (await _staff.GetStaffAsync()).Value;

        Assert.Equal("Santos", staff.TeamName);
        Assert.Equal((28, 12, 4, 18), (staff.AllCount, staff.CoachingCount, staff.MedicalCount, staff.Vacancies));
        Assert.Equal("Gustavo Nicoline", staff.Analyst);

        var coach = staff.Members.Single(member => member.Name == "Eudes Pedro");
        Assert.Equal(("Treinador", "Tático", "R$20 mil p/s", "2027-12-31"), (coach.Job, coach.CoachingStyle, coach.Wage, coach.ContractEnd));
        Assert.Equal([("Tático", 13), ("Defensivo", 11)], coach.CoachingAttributes.Select(a => (a.Name, a.Value)));

        var director = staff.Members.Single(member => member.Name == "César Sampaio");
        var physio = Assert.Single(director.MedicalAttributes);
        Assert.Equal(("Fisioterapia", 4, "Razoável"), (physio.Name, physio.Value, physio.Level));
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetStaffAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _staff.GetStaffAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private static StaffAttribute Attribute(string name, int value, string level)
    {
        return new StaffAttribute { Name = name, Value = value, Level = level };
    }
}
