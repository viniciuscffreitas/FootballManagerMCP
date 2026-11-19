using FootballManager.Domain.Opposition;

namespace FootballManager.Infrastructure.Constants;

// The opposition instructions screen, as the game binds it on the active tactic: the next opponent's team, a record of
// rows (one per player of the lineup the game expects, one per position) and the staff's suggestions.
internal static class OppositionInstructionConstants
{
    public const string Instructions = "NextOppositionInstructions";
    public const string TeamId = "NextOppositionInstructionsTeam." + GameConstants.UniqueId;
    public const string TeamName = "NextOppositionInstructionsTeam." + GameConstants.Name;
    public const string Suggestions = "AllOppositionInstructionSuggestions";

    public static readonly string[] Properties = [TeamId, TeamName, Instructions, Suggestions];

    // The opponent's squad, which names the players the rows hold by record.
    public const string TeamPlayers = TacticConstants.Tactic + ".NextOppositionInstructionsTeam." + TeamConstants.Players;

    // A row of the expected lineup, picked by its place in it (Index, the same as TeamSelectionIndex).
    public const string LineupRows = TacticConstants.Tactic + "." + Instructions + ".OppositionPredictedLineupInstructions";

    // Context: the rows a change goes to, as the screen passes its selection.
    public const string Selection = "SelectedObjectList";

    // Each instruction's value on the tactic is bound to the row by TeamSelectionIndex, PositionMask and the selection
    // together; without all three the game never answers it.
    public static string Property(OppositionInstruction instruction)
    {
        return instruction switch
        {
            OppositionInstruction.Marking => "OppositionPlayerMarking",
            OppositionInstruction.Pressing => "OppositionPlayerPressingTrigger",
            OppositionInstruction.Tackling => "OppositionPlayerTackling",
            OppositionInstruction.ShowOntoFoot => "OppositionPlayerShowOnToFoot",
            _ => throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null)
        };
    }

    // The game's option numbers, in the order of OppositionInstructionValues' keys (read from the
    // OppositionPlayer*Options lists: "-", Nunca, Sempre; "-", Normal, Suave, Duro; "-", Pé Esquerdo, Pé Direito,
    // Pé mais fraco).
    public static int ValueOf(OppositionInstruction instruction, string key)
    {
        var index = OppositionInstructionValues.Of(instruction).ToList().IndexOf(key);
        return index >= 0 ? index : throw new ArgumentException($"{instruction} has no value '{key}'.", nameof(key));
    }

    public static string KeyOf(OppositionInstruction instruction, int? value)
    {
        var keys = OppositionInstructionValues.Of(instruction);
        return value is { } number && number >= 0 && number < keys.Count ? keys[number] : OppositionInstructionValues.None;
    }

    // The staff's suggestion kinds (FeedbackType) whose line names one instruction, as read in the test save:
    // 256 "Marcar ... de perto durante todo o tempo", 253 "Fazer pressão quando a bola for passada", 251 "não pegarem
    // leve na hora de desarmar", 249 "forçar ... a usar o pé mais fraco". Other kinds are listed but not applied.
    public static (OppositionInstruction Instruction, string Value)? Suggested(int? feedbackType)
    {
        return feedbackType switch
        {
            256 => (OppositionInstruction.Marking, "always"),
            253 => (OppositionInstruction.Pressing, "always"),
            251 => (OppositionInstruction.Tackling, "hard"),
            249 => (OppositionInstruction.ShowOntoFoot, "weaker"),
            _ => null
        };
    }
}
