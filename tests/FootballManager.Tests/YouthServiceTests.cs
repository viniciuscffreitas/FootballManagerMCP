using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Youth;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class YouthServiceTests
{
    private readonly IYouthGameClient _game = Substitute.For<IYouthGameClient>();
    private readonly YouthService _youth;

    public YouthServiceTests()
    {
        _youth = new YouthService(_game);
    }

    [Fact]
    public async Task Loans_come_back_with_the_game_report_on_each()
    {
        Read(
            [],
            [
                new LoanedPlayer
                {
                    Id = 7, Name = "Luisão", Club = "Goiás", LoanEndDate = "2026-12-31", MonthlyFee = "R$0", Appearances = 15,
                    AverageRating = 7.29, Happiness = 80, Comments = ["Playing regularly"]
                }
            ]);

        var result = (await _youth.GetYouthAndLoansAsync()).Value;

        var loan = Assert.Single(result.Loans);
        Assert.Equal(("Goiás", "2026-12-31", "R$0", 15, 7.29, 80), (loan.Club, loan.LoanEndDate, loan.MonthlyFee, loan.Appearances, loan.AverageRating, loan.Happiness));
        Assert.Equal(["Playing regularly"], loan.Comments);
        Assert.Equal("All happy", result.LoanReport);
    }

    [Fact]
    public async Task The_youth_setup_comes_back_with_its_next_intake_and_ratings()
    {
        Read([new YouthPlayer { Id = 1, Name = "Kid", Morale = "Good", ContractEndDate = "2027-12-31" }], []);

        var result = (await _youth.GetYouthAndLoansAsync()).Value;

        Assert.Equal(("Santos Sub-20", "2027-01-14", 305), (result.Youth.TeamName, result.Youth.IntakeDate, result.Youth.DaysUntilIntake));
        Assert.Equal((16, "Good", 14, 3), (result.Youth.FacilitiesRating, result.Youth.Facilities, result.Youth.CoachingLevel, result.Youth.Recruitment));
        var kid = Assert.Single(result.YouthPlayers);
        Assert.Equal(("Kid", "Good", "2027-12-31", false, null), (kid.Name, kid.Morale, kid.ContractEndDate, kid.OnLoan, kid.LoanClub));
    }

    [Fact]
    public async Task A_youth_player_out_on_loan_is_marked_with_his_loan_club()
    {
        Read(
            [new YouthPlayer { Id = 1, Name = "Enzo" }, new YouthPlayer { Id = 2, Name = "Home" }],
            [new LoanedPlayer { Id = 1, Name = "Enzo", Club = "Cheongju" }, new LoanedPlayer { Id = 9, Name = "Senior", Club = "Goiás" }]);

        var result = (await _youth.GetYouthAndLoansAsync()).Value;

        var enzo = result.YouthPlayers.Single(player => player.Id == 1);
        Assert.Equal((true, "Cheongju"), (enzo.OnLoan, enzo.LoanClub));
        var home = result.YouthPlayers.Single(player => player.Id == 2);
        Assert.Equal((false, null), (home.OnLoan, home.LoanClub));
        Assert.Equal(2, result.Loans.Count);
    }

    [Fact]
    public async Task A_game_that_cant_answer_fails_the_read()
    {
        _game.GetYouthAndLoansAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("no save"));

        var result = await _youth.GetYouthAndLoansAsync();

        Assert.IsType<GameUnavailableError>(Assert.Single(result.Errors));
    }

    private void Read(List<YouthPlayer> youthPlayers, List<LoanedPlayer> loans)
    {
        _game.GetYouthAndLoansAsync(Arg.Any<CancellationToken>()).Returns(new YouthAndLoans
        {
            Youth = new YouthSetup
            {
                TeamName = "Santos Sub-20", IntakeDate = "2027-01-14", DaysUntilIntake = 305, FacilitiesRating = 16,
                Facilities = "Good", CoachingLevel = 14, Recruitment = 3
            },
            YouthPlayers = youthPlayers,
            Loans = loans,
            LoanReport = "All happy"
        });
    }
}
