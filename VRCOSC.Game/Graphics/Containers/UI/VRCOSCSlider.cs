// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class VRCOSCSlider<T> : BasicSliderBar<T> where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    public Bindable<T> SlowedCurrent = new();

    public VRCOSCSlider()
    {
        BackgroundColour = VRCOSCColour.Gray4;
        SelectionColour = VRCOSCColour.Gray6;
        Masking = true;
        CornerRadius = 10;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteText valueText;
        Add(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(5),
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = FrameworkFont.Regular.With(size: 30),
                    Text = CurrentNumber.MinValue.ToString()!
                },
                valueText = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = FrameworkFont.Regular.With(size: 30)
                },
                new SpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = FrameworkFont.Regular.With(size: 30),
                    Text = CurrentNumber.MaxValue.ToString()!
                }
            }
        });

        Current.BindValueChanged(_ => valueText.Text = getCurrentValue().ToString()!, true);
    }

    protected override void OnDragEnd(DragEndEvent e)
    {
        base.OnDragEnd(e);
        updateSlowedCurrent();
    }

    protected override bool OnClick(ClickEvent e)
    {
        var result = base.OnClick(e);
        updateSlowedCurrent();
        return result;
    }

    private T roundValue()
    {
        // bit excessive, but it keeps the float as 2 decimal places. Might refactor into multiple slider types
        return (T)Convert.ChangeType(MathF.Round(Convert.ToSingle(Current.Value), 2), typeof(T));
    }

    private void updateSlowedCurrent()
    {
        SlowedCurrent.Value = getCurrentValue();
    }

    private T getCurrentValue()
    {
        return typeof(T) == typeof(float) ? roundValue() : Current.Value;
    }
}
