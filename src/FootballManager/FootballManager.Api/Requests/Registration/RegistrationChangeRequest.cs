namespace FootballManager.Api.Requests.Registration;

public class RegistrationChangeRequest
{
    /// <summary>The registration's name or the competition's, as get_squad_registration lists them.</summary>
    public string? Competition { get; set; }

    /// <summary>FM ids of players to register; an empty item is refused.</summary>
    public List<int?>? Register { get; set; }

    /// <summary>FM ids of players to unregister; an empty item is refused.</summary>
    public List<int?>? Unregister { get; set; }
}
