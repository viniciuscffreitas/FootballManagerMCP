namespace FootballManager.Api.Responses.Registration;

public class RegistrationRuleResponse
{
    public int? Type { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
    public int? Value { get; set; }
    public string? Display { get; set; }
    public bool Met { get; set; }
}
