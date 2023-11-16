// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics;

public abstract partial class DrawableBindableListBindableModuleAttribute<T> : DrawableBindableListModuleAttribute<T> where T : ModuleAttribute
{
    protected DrawableBindableListBindableModuleAttribute(T moduleAttribute)
        : base(moduleAttribute)
    {
    }
}
