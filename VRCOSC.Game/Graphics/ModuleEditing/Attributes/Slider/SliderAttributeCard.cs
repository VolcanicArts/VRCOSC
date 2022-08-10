// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;

public abstract class SliderAttributeCard<T> : AttributeCard where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    protected ModuleAttributeDataWithBounds AttributeDataWithBounds;

    private VRCOSCSlider<T> slider = null!;

    protected SliderAttributeCard(ModuleAttributeDataWithBounds attributeData)
        : base(attributeData)
    {
        AttributeDataWithBounds = attributeData;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ContentFlow.Add(slider = new VRCOSCSlider<T>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Current = CreateCurrent()
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        slider.SlowedCurrent.ValueChanged += e => UpdateValues(e.NewValue);
    }

    protected override void UpdateValues(object value)
    {
        base.UpdateValues(value);
        slider.SlowedCurrent.Value = (T)value;
    }

    protected abstract Bindable<T> CreateCurrent();
}
