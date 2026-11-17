namespace FootballManager.Application.ResultErrors;

/// <summary>The save guard refused a write: the wrong save is loaded, the game's version isn't cleared, or no copy could be made.</summary>
public class SaveRefusedError(string reason) : ValidationError("save", [reason]);
