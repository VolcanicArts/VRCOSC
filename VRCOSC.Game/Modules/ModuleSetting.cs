// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using System;

namespace VRCOSC.Game.Modules;

public class ModuleSetting
{
    public string Key { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }
    public object Value { get; set; }
    public Type Type { get; init; }
}
