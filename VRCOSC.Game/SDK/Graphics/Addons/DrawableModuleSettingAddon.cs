// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.SDK.Attributes.Settings.Addons;

namespace VRCOSC.SDK.Graphics.Addons;

public partial class DrawableModuleSettingAddon<T> : Container where T : ModuleSettingAddon
{
    protected T ModuleSettingAddon;

    public DrawableModuleSettingAddon(T moduleSettingAddon)
    {
        ModuleSettingAddon = moduleSettingAddon;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }
}
