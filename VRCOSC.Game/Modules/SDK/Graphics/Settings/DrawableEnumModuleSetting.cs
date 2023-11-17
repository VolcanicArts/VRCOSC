// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics.Settings;

public partial class DrawableEnumModuleSetting<T> : DrawableModuleSetting<EnumModuleSetting<T>> where T : Enum
{
    public DrawableEnumModuleSetting(EnumModuleSetting<T> moduleSetting)
        : base(moduleSetting)
    {
    }
}
