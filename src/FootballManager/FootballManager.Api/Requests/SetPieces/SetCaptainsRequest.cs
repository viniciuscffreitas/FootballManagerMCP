namespace FootballManager.Api.Requests.SetPieces;

public class SetCaptainsRequest
{
    /// <summary>The first-team player to make captain (FM id); null leaves the captain as he is.</summary>
    public int? CaptainId { get; set; }

    /// <summary>The first-team player to make vice-captain (FM id); null leaves the vice as he is.</summary>
    public int? ViceCaptainId { get; set; }
}
