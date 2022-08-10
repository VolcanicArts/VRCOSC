// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace VRCOSC.Game.Graphics.Updater;

public class LoadingContainer : PhaseContainer
{
    protected override float ScaleFrom => 1.1f;

    public ProgressBar ProgressBar = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
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

    protected override void OnPhaseChange()
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
                throw new ArgumentOutOfRangeException(nameof(updatePhase), updatePhase, $"Cannot use this update phase inside {nameof(LoadingContainer)}");
        }
    }
}
