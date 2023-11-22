// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.SDK.Attributes.Settings;

namespace VRCOSC.Game.Modules.SDK.Graphics.Settings.Values;

public partial class DrawableIntSliderModuleSetting : DrawableValueModuleSetting<RangedIntModuleSetting>
{
    public DrawableIntSliderModuleSetting(RangedIntModuleSetting moduleAttribute)
        : base(moduleAttribute)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new VRCOSCSlider<int>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            RoudedCurrent = ModuleSetting.Attribute.GetBoundCopy()
        });
    }
}

public partial class DrawableFloatSliderModuleSetting : DrawableValueModuleSetting<RangedFloatModuleSetting>
{
    public DrawableFloatSliderModuleSetting(RangedFloatModuleSetting moduleAttribute)
        : base(moduleAttribute)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new VRCOSCSlider<float>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            RoudedCurrent = ModuleSetting.Attribute.GetBoundCopy()
        });
    }
}
