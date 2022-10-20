// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class TerminalEntry : PoolableDrawable
{
    public string Text
    {
        set => ((TextFlowContainer)InternalChild).Text = value;
    }

    public TerminalEntry()
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
            Colour = VRCOSCColour.Gray8
        };
    }

    public override void Show()
    {
        this.FadeInFromZero(500, Easing.OutCirc);
    }

    public override void Hide()
    {
        this.FadeOutFromOne();
    }
}
