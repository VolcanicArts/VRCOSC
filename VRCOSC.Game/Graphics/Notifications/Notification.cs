// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Notifications;

public abstract partial class Notification : VisibilityContainer
{
    public override bool DisposeOnDeathRemoval => true;
    protected override bool ShouldBeAlive => Alpha > 0.5f;

    protected override Container<Drawable> Content { get; }

    protected Notification()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 60;

        InternalChild = new Container
        {
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
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = VRCOSCColour.Gray3
                    },
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(CreateForeground());
    }

    protected abstract Drawable CreateForeground();

    protected override void PopIn() => InternalChild.MoveToX(0, 250, Easing.OutQuad);
    protected override void PopOut() => InternalChild.MoveToX(1, 250, Easing.InQuad).Finally(_ => Alpha = 0);

    protected override bool OnClick(ClickEvent e)
    {
        Hide();
        return true;
    }
}
