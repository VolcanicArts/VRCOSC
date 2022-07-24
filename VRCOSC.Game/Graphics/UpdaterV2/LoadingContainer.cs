// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game.Graphics.UpdaterV2;

public class LoadingContainer : VisibilityContainer
{
    public ProgressBar ProgressBar = null!;

    private UpdatePhase updatePhase;

    public UpdatePhase UpdatePhase
    {
        get => updatePhase;
        set
        {
            updatePhase = value;
            updateUsingPhase();
        }
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AutoSizeAxes = Axes.Both;

        Child = new FillFlowContainer
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
                    Child = ProgressBar = new ProgressBar
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            }
        };
    }

    private void updateUsingPhase()
    {
        switch (updatePhase)
        {
            case UpdatePhase.Check:
                ProgressBar.Text = "Checking";
                break;

            case UpdatePhase.Download:
                ProgressBar.Text = "Downloading";
                break;

            case UpdatePhase.Install:
                ProgressBar.Text = "Installing";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(updatePhase), updatePhase, $"Cannot use this update phases inside {nameof(LoadingContainer)}");
        }
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(200, Easing.OutQuint);
        this.ScaleTo(1.1f).Then().ScaleTo(1, 200, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(200, Easing.OutQuint);
        this.ScaleTo(1f).Then().ScaleTo(1.1f, 200, Easing.OutQuint);
    }
}
