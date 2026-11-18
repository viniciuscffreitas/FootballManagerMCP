namespace FootballManager.Application.Exceptions;

/// <summary>
/// A write was sent but the game didn't answer it: it may or may not have taken. Only a read back of the game can tell.
/// </summary>
public class GameWriteUnknownException(string message, Exception? innerException = null) : GameUnavailableException(message, innerException);
