// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osuTK;
using VRCOSC.Game.Graphics;
using Fonts = VRCOSC.Game.Graphics.Fonts;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class TerminalEntry : PoolableDrawable
{
    public string Text
    {
        set => ((TextFlowContainer)InternalChild).Text = value;
    }

    public TerminalEntry()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChild = new TextFlowContainer(t =>
        {
            t.Font = Fonts.REGULAR.With(size: 22);
            t.Colour = Colours.WHITE1;
        })
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            TextAnchor = Anchor.CentreLeft,
            RelativePositionAxes = Axes.X
        };
    }

    public override void Show()
    {
        this.FadeInFromZero(500, Easing.OutCirc);
        InternalChild.MoveTo(new Vector2(1, 0)).MoveTo(Vector2.Zero, 500, Easing.OutQuint);
    }

    public override void Hide()
    {
        Alpha = 0;
    }
}
