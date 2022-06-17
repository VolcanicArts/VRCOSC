// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules;

public struct InputParameterData
{
    public readonly Type Type;
    public readonly ActionMenu ActionMenu;

    public InputParameterData(Type type, ActionMenu actionMenu)
    {
        Type = type;
        ActionMenu = actionMenu;
    }
}
