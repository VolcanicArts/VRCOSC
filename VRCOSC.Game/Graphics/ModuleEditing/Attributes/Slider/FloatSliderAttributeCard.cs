// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;

public sealed class FloatSliderAttributeCard : SliderAttributeCard<float>
{
    public FloatSliderAttributeCard(ModuleAttributeSingleWithBounds attributeData)
        : base(attributeData)
    {
    }

    protected override Bindable<float> CreateCurrent() => new BindableNumber<float>
    {
        MinValue = (float)AttributeDataWithBounds.MinValue,
        MaxValue = (float)AttributeDataWithBounds.MaxValue,
        Precision = 0.01f
    };
}
