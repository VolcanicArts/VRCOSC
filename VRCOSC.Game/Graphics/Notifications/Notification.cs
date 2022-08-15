// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Notifications;

public class Notification : VisibilityContainer
{
    public override bool DisposeOnDeathRemoval => true;
    protected override bool ShouldBeAlive => Alpha > 0.5f;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 60;

        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            RelativePositionAxes = Axes.X,
            X = 1,
            Padding = new MarginPadding
            {
                Horizontal = 5,
                Top = 5
            },
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = VRCOSCColour.Gray3
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Child = CreateForeground()
                    }
                }
            }
        };
    }

    protected virtual Drawable CreateForeground() { throw new NotImplementedException(); }

    protected override void PopIn()
    {
        Child.MoveToX(0, 250, Easing.OutQuad);
    }

    protected override void PopOut()
    {
        Child.MoveToX(1, 250, Easing.InQuad).Finally(_ => Alpha = 0);
    }

    protected override bool OnClick(ClickEvent e)
    {
        Hide();
        return true;
    }
}
