namespace FootballManager.Application.Exceptions;

/// <summary>
/// A write went out to the game but its answer was lost (a timeout, or an error after the send): the game may have
/// taken it. Only a read back tells.
/// </summary>
public class GameAnswerLostException(string message, Exception? innerException = null) : GameUnavailableException(message, innerException);
