using RaceElement.Data.Games;
using System;

namespace RaceElement.HUD.Overlay.Configuration;

/// <summary>
/// Can be used with <see cref="ConfigGroupingAttribute"/>
/// </summary>
/// <param name="game"></param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class HideForGameAttribute(Game game) : Attribute
{
    public Game Game { get; init; } = game;
}
