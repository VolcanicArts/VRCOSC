// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.ChatBox;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Clip;

public partial class TimelineClipMenu : TimelineMenu
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private Game.ChatBox.Clips.Clip clip;

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
                Action = () => chatBoxManager.IncreasePriority(clip)
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
                Action = () => chatBoxManager.DecreasePriority(clip)
            }
        });
    }

    public void SetClip(Game.ChatBox.Clips.Clip clip)
    {
        this.clip = clip;
    }
}
