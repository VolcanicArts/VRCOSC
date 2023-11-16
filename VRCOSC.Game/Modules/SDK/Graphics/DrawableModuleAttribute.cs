// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics;

public partial class DrawableModuleAttribute : Container
{
}

public partial class DrawableModuleAttribute<T> : DrawableModuleAttribute where T : ModuleAttribute
{
    protected readonly T ModuleAttribute;

    public DrawableModuleAttribute(T moduleAttribute)
    {
        ModuleAttribute = moduleAttribute;
    }
}
