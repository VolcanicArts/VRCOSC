// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.Updater;

public class ProgressBar : Container
{
    public Bindable<float> Progress = new();

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
            }
        };

        Progress.ValueChanged += (percentage) =>
        {
            var progress = MathF.Min(MathF.Max(percentage.NewValue, 0), 1);
            bar.MoveToX(-1 + progress, 250, Easing.OutCirc);
        };
    }
}
