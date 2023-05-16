// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using VRCOSC.Game.Graphics.Themes;

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
        AutoSizeAxes = Axes.Y;

        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            RelativePositionAxes = Axes.X,
            X = 1,
            Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 5,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                BorderThickness = 2,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ThemeManager.Current[ThemeAttribute.Dark]
                    },
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
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
        Close();
        return true;
    }

    public void Close() => Scheduler.Add(Hide);
}
