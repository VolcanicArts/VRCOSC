// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics;

public abstract partial class DrawableModuleAttribute<T> : Container where T : ModuleAttribute
{
    protected T ModuleAttribute;

    protected DrawableModuleAttribute(T moduleAttribute)
    {
        ModuleAttribute = moduleAttribute;
    }
}

public abstract partial class DrawableValueModuleAttribute<T> : DrawableModuleAttribute<T> where T : ModuleAttribute
{
    protected DrawableValueModuleAttribute(T moduleAttribute)
        : base(moduleAttribute)
    {
    }
}

public abstract partial class DrawableListModuleAttribute<T> : DrawableModuleAttribute<T> where T : ModuleAttribute
{
    protected DrawableListModuleAttribute(T moduleAttribute)
        : base(moduleAttribute)
    {
    }
}
