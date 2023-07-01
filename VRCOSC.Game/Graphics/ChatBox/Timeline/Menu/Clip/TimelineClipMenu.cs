// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.App;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Clip;

public partial class TimelineClipMenu : TimelineMenu
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private Game.ChatBox.Clips.Clip clip { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            RelativeSizeAxes = Axes.X,
            Height = 25,
            Child = new MenuButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Move Up",
                FontSize = 20,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Action = () => appManager.ChatBoxManager.IncreasePriority(clip)
            }
        });

        Add(new Container
        {
            RelativeSizeAxes = Axes.X,
            Height = 25,
            Child = new MenuButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Move Down",
                FontSize = 20,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Action = () => appManager.ChatBoxManager.DecreasePriority(clip)
            }
        });

        Add(new Container
        {
            RelativeSizeAxes = Axes.X,
            Height = 25,
            Child = new MenuButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Delete Clip",
                FontSize = 20,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Action = () =>
                {
                    appManager.ChatBoxManager.DeleteClip(clip);
                    Hide();
                }
            }
        });
    }

    public void SetClip(Game.ChatBox.Clips.Clip clip)
    {
        this.clip = clip;
    }
}
