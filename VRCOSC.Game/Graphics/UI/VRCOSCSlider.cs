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

namespace VRCOSC.Graphics.UI;

public sealed partial class VRCOSCSlider<T> : BasicSliderBar<T> where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    public required BindableNumber<T> RoudedCurrent { get; init; }

    public VRCOSCSlider()
    {
        BackgroundColour = Colours.GRAY2;
        SelectionColour = Colours.GRAY5;
        Masking = true;
        CornerRadius = 5;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Current = RoudedCurrent.GetUnboundCopy();

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
                    Font = FrameworkFont.Regular.With(size: Height * 0.8f),
                    Colour = Colours.WHITE0,
                    Text = CurrentNumber.MinValue.ToString()!
                },
                valueText = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = FrameworkFont.Regular.With(size: Height * 0.8f),
                    Colour = Colours.WHITE0
                },
                new SpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = FrameworkFont.Regular.With(size: Height * 0.8f),
                    Colour = Colours.WHITE0,
                    Text = CurrentNumber.MaxValue.ToString()!
                }
            }
        });

        Current.BindValueChanged(_ => valueText.Text = getCurrentValue().ToString()!, true);
        RoudedCurrent.BindValueChanged(e => Current.Value = e.NewValue);
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        RoudedCurrent.Value = getCurrentValue();
    }

    private T getCurrentValue() => typeof(T) == typeof(float) ? roundValue() : Current.Value;

    // bit excessive, but it keeps the float as 2 decimal places. Might refactor into multiple slider types
    private T roundValue() => (T)Convert.ChangeType(MathF.Round(Convert.ToSingle(Current.Value), 2), typeof(T));
}
