// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Slider;

public sealed partial class FloatSliderAttributeCard : SliderAttributeCard<float>
{
    public FloatSliderAttributeCard(ModuleAttributeWithBounds attributeData)
        : base(attributeData)
    {
    }

    protected override Bindable<float> CreateCurrent() => new BindableNumber<float>
    {
        MinValue = (float)AttributeDataWithBounds.MinValue,
        MaxValue = (float)AttributeDataWithBounds.MaxValue,
        Precision = 0.01f,
        Value = (float)AttributeData.Attribute.Value
    };
}
