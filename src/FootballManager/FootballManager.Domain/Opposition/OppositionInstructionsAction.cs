namespace FootballManager.Domain.Opposition;

/// <summary>What to do with the instructions against the next opponent, as the opposition instructions screen offers.</summary>
public enum OppositionInstructionsAction
{
    /// <summary>Give one opponent player the instructions named.</summary>
    Set,

    /// <summary>Take every instruction off one opponent player.</summary>
    Clear,

    /// <summary>Give the instructions the staff suggests, for every player or for one.</summary>
    ApplyStaffAdvice
}
