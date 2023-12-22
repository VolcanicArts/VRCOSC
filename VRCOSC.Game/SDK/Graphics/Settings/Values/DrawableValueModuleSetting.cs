// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Screens.Main.Modules.Settings;
using VRCOSC.SDK.Attributes.Settings;

namespace VRCOSC.SDK.Graphics.Settings.Values;

public abstract partial class DrawableValueModuleSetting<T> : DrawableModuleSetting<T> where T : ModuleSetting
{
    protected DrawableValueModuleSetting(T moduleAttribute)
        : base(moduleAttribute)
    {
    }

    protected internal void AddSide(Drawable drawable) => SideContainer.Add(drawable);
}
