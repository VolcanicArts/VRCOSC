// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osuTK.Graphics;
using VRCOSC.Graphics;

namespace VRCOSC.SDK;

public enum ModuleType
{
    Generic,
    Integrations,
    Health,
    NSFW
}

public static class ModuleTypeExtensions
{
    public static Color4 ToColour(this ModuleType type) => type switch
    {
        ModuleType.Generic => Colours.WHITE0,
        ModuleType.Integrations => Colours.YELLOW0,
        ModuleType.Health => Colours.RED0,
        ModuleType.NSFW => Colours.BLACK,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
