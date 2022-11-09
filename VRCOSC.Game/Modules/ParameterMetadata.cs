// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules;

public class ParameterMetadata
{
    public readonly ParameterMode Mode;
    public readonly string Name;
    public readonly string Description;
    public readonly Type ExpectedType;
    public readonly ActionMenu Menu;

    public string FormattedAddress => $"/avatar/parameters/{Name}";

    public ParameterMetadata(ParameterMode mode, string name, string description, Type expectedType, ActionMenu menu)
    {
        Mode = mode;
        Name = name;
        Description = description;
        ExpectedType = expectedType;
        Menu = menu;
    }
}

public enum ParameterMode
{
    Read = 1 << 0,
    Write = 1 << 1,
    ReadWrite = Read | Write
}
