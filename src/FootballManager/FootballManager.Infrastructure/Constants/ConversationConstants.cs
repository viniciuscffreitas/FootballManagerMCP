using FootballManager.Domain.Conversations;

namespace FootballManager.Infrastructure.Constants;

// One-to-one conversations, as the player's context menu opens them. The menu entry opens a panel rather than sending
// an event (its Event is 0): EventFmxDoAction and OpenNewsActionPanelEvent open nothing, so the plugin's /contextaction
// runs it through PluginContextMenuContributor.PerformAction, as a click on the menu does, with the entry's ActionData.
// The card it opens (QuickChatDynamicCard) is UI only: its lines are buttons (class suggestion-button) pressed through
// /click, and it's read from the /ui tree.
internal static class ConversationConstants
{
    // FM.GamePlugin.ActionID values, offered in game.Actions for the player as the menu's Records.
    public static uint ActionIdOf(PlayerConversation conversation)
    {
        return conversation switch
        {
            PlayerConversation.PraisePlayerTrainingDevelopment => 1800548932,
            PlayerConversation.WarnPlayerTrainingDevelopment => 544892514,
            PlayerConversation.PraisePlayerPerformance => 1517112644,
            PlayerConversation.WarnPlayerPerformance => 1684630650,
            PlayerConversation.PraisePlayerConduct => 961772120,
            PlayerConversation.WarnPlayerConduct => 1684493637,
            _ => throw new ArgumentOutOfRangeException(nameof(conversation), conversation, null)
        };
    }

    public const string Squad = TeamConstants.OwnTeam + "." + TeamConstants.Players;
    public const string OpenEndpoint = "/contextaction";

    // The menu's only entry; its payload is what the click passes on.
    public const string EntryData = "Data:@" + ScoutingConstants.MenuItems + "[0].ActionData";

    // The /ui tree deep enough to reach the card's buttons, and what marks the card and its parts there.
    public const string UiPath = "/ui?maxDepth=160";
    public const string ClickEndpoint = "/click";
    public const string ClickHow = "simulate";
    public const string Card = "QuickChatDynamicCard";
    public const string LineClass = "suggestion-button";
    public const string Button = "SIButton";

    // The card's corner X: it closes the card without an answer.
    public const string CloseButton = "SIButton#buttons-controls-pc-default";

    // Texts of the card in the game's display language (Brazilian Portuguese in the user's game). The body language
    // follows its label; "End conversation" closes the card once the rounds are over. If the language differs, the
    // body language reads as null and the corner X closes the card instead.
    public const string BodyLanguageLabel = "Linguagem corporal:";
    public const string EndConversation = "Encerrar conversa";
}
