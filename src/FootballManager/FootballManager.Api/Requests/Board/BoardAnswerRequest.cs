namespace FootballManager.Api.Requests.Board;

public class BoardAnswerRequest
{
    /// <summary>The line to say, by its number from 0 or a piece of its text; left out, the dry run only lists the lines.</summary>
    public string? Line { get; set; }
}
