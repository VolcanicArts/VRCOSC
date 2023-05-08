// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;

public sealed partial class IntSliderAttributeCard : SliderAttributeCard<int>
{
    public IntSliderAttributeCard(ModuleAttributeWithBounds attributeData)
        : base(attributeData)
    {
    }

    protected override Bindable<int> CreateCurrent() => new BindableNumber<int>
    {
        MinValue = (int)AttributeDataWithBounds.MinValue,
        MaxValue = (int)AttributeDataWithBounds.MaxValue,
        Precision = 1,
        Value = (int)AttributeData.Attribute.Value
    };
}
