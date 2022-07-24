// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game.Graphics.UpdaterV2;

public sealed class UpdaterScreen : VisibilityContainer
{
    private ProgressBar progressBar = null!;

    public UpdaterScreen()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        State.Value = Visibility.Visible;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black.Opacity(0.75f)
            },
            new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Spacing = new Vector2(0, 5),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new LoadingCircle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(100)
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(400, 30),
                        Masking = true,
                        Child = progressBar = new ProgressBar
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            }
        };

        progressBar.Current.Value = 0.5f;
        progressBar.Text = "Checking";
    }

    protected override void PopIn()
    {
        this.FadeIn(250, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeIn(250, Easing.InQuint);
    }

    protected override bool OnMouseDown(MouseDownEvent e) => State.Value == Visibility.Visible;
    protected override bool OnHover(HoverEvent e) => State.Value == Visibility.Visible;
}
