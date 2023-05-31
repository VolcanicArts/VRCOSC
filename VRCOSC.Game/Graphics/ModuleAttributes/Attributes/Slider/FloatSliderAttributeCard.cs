// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Slider;

public sealed partial class FloatSliderAttributeCard : SliderAttributeCard<float, ModuleFloatRangeAttribute>
{
    public FloatSliderAttributeCard(ModuleFloatRangeAttribute attributeData)
        : base(attributeData)
    {
    }

    protected override BindableNumber<float> CreateCurrent() => (BindableNumber<float>)AttributeData.Attribute;
}
