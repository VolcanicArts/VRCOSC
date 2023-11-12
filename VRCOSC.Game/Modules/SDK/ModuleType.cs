// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osuTK.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Modules.SDK;

public enum ModuleType
{
    Generic
}

public static class ModuleTypeExtensions
{
    public static Color4 ToColour(this ModuleType type)
    {
        switch (type)
        {
            case ModuleType.Generic:
                return Colours.WHITE0;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
