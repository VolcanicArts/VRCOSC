// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics;

public abstract partial class DrawableModuleSetting<T> : Container where T : ModuleSetting
{
    protected T ModuleSetting;

    protected DrawableModuleSetting(T moduleSetting)
    {
        ModuleSetting = moduleSetting;
    }
}

public abstract partial class DrawableValueModuleSetting<T> : DrawableModuleSetting<T> where T : ModuleSetting
{
    protected DrawableValueModuleSetting(T moduleAttribute)
        : base(moduleAttribute)
    {
    }
}

public abstract partial class DrawableListModuleSetting<T> : DrawableModuleSetting<T> where T : ModuleSetting
{
    protected DrawableListModuleSetting(T moduleSetting)
        : base(moduleSetting)
    {
    }
}
