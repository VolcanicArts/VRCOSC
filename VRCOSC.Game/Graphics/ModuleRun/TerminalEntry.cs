// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class TerminalEntry : Container
{
    private readonly string text;

    public TerminalEntry(string text)
    {
        this.text = text;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChild = new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 20))
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            TextAnchor = Anchor.CentreLeft,
            Colour = VRCOSCColour.Gray8,
            Text = text
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        this.FadeInFromZero(500, Easing.OutCirc);
    }
}
