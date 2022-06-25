// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class SliderAttributeCard<T> : AttributeCard where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    private readonly T minValue;
    private readonly T maxValue;
    private readonly T precision;
    private VRCOSCSlider<T> slider;

    public SliderAttributeCard(ModuleAttributeDataWithBounds attributeData, T precision)
        : base(attributeData)
    {
        minValue = (T)attributeData.MinValue;
        maxValue = (T)attributeData.MaxValue;
        this.precision = precision;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(1.0f, 0.5f),
            Padding = new MarginPadding(10),
            Child = slider = new VRCOSCSlider<T>()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Current = new BindableNumber<T>
                {
                    MinValue = minValue,
                    MaxValue = maxValue,
                    Precision = precision
                }
            }
        });

        AttributeData.Attribute.ValueChanged += updateBindables;
        AttributeData.Attribute.TriggerChange();

        slider.SlowedCurrent.BindValueChanged(e => AttributeData.Attribute.Value = e.NewValue, true);
    }

    private void updateBindables(ValueChangedEvent<object> e)
    {
        slider.Current.Value = (T)e.NewValue;
        slider.SlowedCurrent.Value = (T)e.NewValue;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateBindables;
        base.Dispose(isDisposing);
    }
}
