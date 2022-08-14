// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game.Graphics.Notifications;

public class ProgressNotification : BasicNotification
{
    private float progress;

    public float Progress
    {
        get => progress;
        set
        {
            progress = value;
            progressBar.Current.Value = progress;
        }
    }

    private ProgressBar progressBar = null!;

    protected override Drawable CreateForeground()
    {
        return new GridContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    base.CreateForeground()
                },
                new Drawable[]
                {
                    progressBar = new ProgressBar
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColour = VRCOSCColour.Gray5,
                        SelectionColour = VRCOSCColour.GreenLight
                    }
                }
            }
        };
    }
}
