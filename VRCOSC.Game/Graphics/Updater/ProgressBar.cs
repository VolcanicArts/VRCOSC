// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Updater;

public class ProgressBar : Container
{
    public Bindable<float> Progress = new();
    public Bindable<string> Text = new();

    private SpriteText spriteText;
    private int periodCount;

    [BackgroundDependencyLoader]
    private void load()
    {
        Box bar;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            bar = new Box
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.X,
                Colour = VRCOSCColour.Green,
                X = -1
            },
            spriteText = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = FrameworkFont.Regular.With(size: 20),
                Shadow = true
            }
        };

        Progress.ValueChanged += (percentage) =>
        {
            var progress = MathF.Min(MathF.Max(percentage.NewValue, 0), 1);
            bar.MoveToX(-1 + progress, 250, Easing.OutCirc);
        };

        Text.BindValueChanged(_ =>
        {
            spriteText.Text = Text.Value;
            periodCount = 0;
            Scheduler.CancelDelayedTasks();
            Scheduler.AddDelayed(updateTextPeriods, 500, true);
        }, true);
    }

    private void updateTextPeriods()
    {
        periodCount++;
        if (periodCount == 4) periodCount = 0;
        spriteText.Text = Text + new string('.', periodCount);
    }
}
