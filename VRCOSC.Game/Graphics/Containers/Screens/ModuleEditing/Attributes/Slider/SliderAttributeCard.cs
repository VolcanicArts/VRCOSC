// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Slider;

public class SliderAttributeCard<T> : AttributeCard where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    protected ModuleAttributeDataWithBounds AttributeDataWithBounds;

    private VRCOSCSlider<T> slider = null!;

    public SliderAttributeCard(ModuleAttributeDataWithBounds attributeData)
        : base(attributeData)
    {
        AttributeDataWithBounds = attributeData;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddToFlow(slider = CreateSlider());

        AttributeData.Attribute.ValueChanged += updateValues;
        slider.SlowedCurrent.ValueChanged += e => updateValues(e.NewValue);
    }

    protected virtual VRCOSCSlider<T> CreateSlider() { throw new NotImplementedException(); }

    private void updateValues(ValueChangedEvent<object> e)
    {
        updateValues((T)e.NewValue);
    }

    private void updateValues(T value)
    {
        AttributeData.Attribute.Value = value;
        slider.SlowedCurrent.Value = value;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateValues;
        base.Dispose(isDisposing);
    }
}
