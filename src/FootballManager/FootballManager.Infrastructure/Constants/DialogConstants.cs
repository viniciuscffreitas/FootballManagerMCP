namespace FootballManager.Infrastructure.Constants;

// The game's Yes/No confirmation dialog, as the plugin's /ui lists it (seen on 16/11/2026 for "Enviar a numeração atual
// do elenco?"): the panel GenericModalDialog, shown with the class panel--visible, holds the title, the description
// and the primary ("Sim") and secondary ("Não") buttons.
internal static class DialogConstants
{
    // The dialog sits about 17 levels deep in its document.
    public const string UiPath = "/ui?maxDepth=40";
    public const string Panel = "Panel#GenericModalDialog";
    public const string ShownPanel = "panel--visible";
    public const string Title = "SIText#dialog-title";
    public const string Description = "SIText#dialog-description";
    public const string YesButton = "SIButton#buttons-button-primary-default";
    public const string ClickEndpoint = "/click";
    public const string ClickHow = "simulate";
}
