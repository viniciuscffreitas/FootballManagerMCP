using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class YouthTools
{
    [McpServerTool(Name = "get_youth_and_loans")]
    [Description("The club's youth set-up and its players out on loan in the user's running Football Manager 27 save, " +
                 "read live from the game. Returns: youth (the highest youth team, e.g. \"Santos Sub-20\", the date of " +
                 "the next youth intake and days until it, the youth facilities' rating out of 20 with its text, the " +
                 "youth coaching level out of 20, the youth recruitment level as the game holds it, and intake " +
                 "candidates still to decide); youthPlayers (every player of that youth team with FM id, age, " +
                 "positions, morale and contract end; the game keeps loaned youth players in the list, so onLoan and " +
                 "loanClub mark them); loans (every player out on loan: loan club, loan end date, the monthly fee the " +
                 "loan club pays as the game shows it, e.g. \"R$0\", appearances and average rating there, happiness " +
                 "0-100 and the loan report's comments); loanReport (the game's summary of the loans) and " +
                 "playersSuggestedForLoan. Read only. Texts are in the game's display language. Fails with a 503 when " +
                 "the game isn't running or no save is loaded.")]
    public static async Task<string> GetYouthAndLoans(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/youth");
        return await response.ReadContentOrError();
    }
}
