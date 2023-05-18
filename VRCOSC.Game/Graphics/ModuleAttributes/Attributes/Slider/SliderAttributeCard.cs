// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Slider;

public abstract partial class SliderAttributeCard<TValue, TAttribute> : AttributeCard<TAttribute> where TValue : struct, IComparable<TValue>, IConvertible, IEquatable<TValue> where TAttribute : ModuleAttribute
{
    protected SliderAttributeCard(TAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new VRCOSCSlider<TValue>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Current = CreateCurrent().GetBoundCopy()
        });
    }

    protected abstract BindableNumber<TValue> CreateCurrent();
}
