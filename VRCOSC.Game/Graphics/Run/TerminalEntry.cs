// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Run;

public sealed partial class TerminalEntry : PoolableDrawable
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
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        })
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            TextAnchor = Anchor.CentreLeft
        };
    }

    public override void Show()
    {
        this.FadeInFromZero(500, Easing.OutCirc);
    }

    public override void Hide()
    {
        Alpha = 0;
    }
}
