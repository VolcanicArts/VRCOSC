// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI;

public sealed partial class VRCOSCSlider<T> : BasicSliderBar<T> where T : struct, IComparable<T>, IConvertible, IEquatable<T>
{
    public required BindableNumber<T> RoudedCurrent { get; init; }

    public VRCOSCSlider()
    {
        BackgroundColour = ThemeManager.Current[ThemeAttribute.Dark];
        SelectionColour = ThemeManager.Current[ThemeAttribute.Mid];
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];
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
                    Colour = ThemeManager.Current[ThemeAttribute.Text],
                    Text = CurrentNumber.MinValue.ToString()!
                },
                valueText = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = FrameworkFont.Regular.With(size: Height * 0.8f),
                    Colour = ThemeManager.Current[ThemeAttribute.Text]
                },
                new SpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = FrameworkFont.Regular.With(size: Height * 0.8f),
                    Colour = ThemeManager.Current[ThemeAttribute.Text],
                    Text = CurrentNumber.MaxValue.ToString()!
                }
            }
        });

        Current.BindValueChanged(_ =>
        {
            valueText.Text = getCurrentValue().ToString()!;
            RoudedCurrent.Value = getCurrentValue();
        }, true);

        RoudedCurrent.BindValueChanged(e =>
        {
            Current.Value = e.NewValue;
        });
    }

    private T getCurrentValue() => typeof(T) == typeof(float) ? roundValue() : Current.Value;

    // bit excessive, but it keeps the float as 2 decimal places. Might refactor into multiple slider types
    private T roundValue() => (T)Convert.ChangeType(MathF.Round(Convert.ToSingle(Current.Value), 2), typeof(T));
}
