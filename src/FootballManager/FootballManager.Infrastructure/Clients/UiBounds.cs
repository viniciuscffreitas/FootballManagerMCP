namespace FootballManager.Infrastructure.Clients;

/// <summary>Where an element of the game's UI sits on screen, as /ui lists it: "@120,300 400x40".</summary>
internal sealed record UiBounds(float X, float Y, float Width, float Height);
