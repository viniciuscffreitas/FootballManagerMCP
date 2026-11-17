namespace FootballManager.Domain.Conversations;

/// <summary>
/// A one-to-one conversation the user can start with a player from his menu, named as the game's ActionID names it:
/// praise or criticise (warn) his training, his performances or his conduct.
/// </summary>
public enum PlayerConversation
{
    PraisePlayerTrainingDevelopment,
    WarnPlayerTrainingDevelopment,
    PraisePlayerPerformance,
    WarnPlayerPerformance,
    PraisePlayerConduct,
    WarnPlayerConduct
}
