// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Slider;

public class FloatSliderAttributeCard : SliderAttributeCard<float>
{
    public FloatSliderAttributeCard(ModuleAttributeDataWithBounds attributeData)
        : base(attributeData)
    {
    }

    protected override VRCOSCSlider<float> CreateSlider()
    {
        return new VRCOSCSlider<float>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Current = new BindableNumber<float>
            {
                MinValue = (float)AttributeDataWithBounds.MinValue,
                MaxValue = (float)AttributeDataWithBounds.MaxValue,
                Precision = 0.01f
            }
        };
    }
}
