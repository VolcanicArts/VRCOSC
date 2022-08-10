// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;

public class IntSliderAttributeCard : SliderAttributeCard<int>
{
    public IntSliderAttributeCard(ModuleAttributeDataWithBounds attributeData)
        : base(attributeData)
    {
    }

    protected override VRCOSCSlider<int> CreateSlider()
    {
        return base.CreateSlider().With(a =>
        {
            a.Current = new BindableNumber<int>
            {
                MinValue = (int)AttributeDataWithBounds.MinValue,
                MaxValue = (int)AttributeDataWithBounds.MaxValue,
                Precision = 1
            };
        });
    }
}
