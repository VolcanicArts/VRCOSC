// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using osu.Framework.Bindables;

namespace VRCOSC.Game.Modules;

public class ModuleSetting
{
    public string Key { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }
    public Bindable<object> Value { get; } = new();
    public Type Type { get; init; }
}
