namespace FootballManager.Api.Requests.SetPieces;

public class SetTakersRequest
{
    /// <summary>The taker list's key, as get_set_pieces lists it.</summary>
    public required string List { get; set; }

    /// <summary>The takers (FM ids), first to take it first.</summary>
    public List<int> PlayerIds { get; set; } = [];
}
