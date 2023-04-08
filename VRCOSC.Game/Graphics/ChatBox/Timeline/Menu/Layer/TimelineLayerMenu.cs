// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.ChatBox;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Layer;

public partial class TimelineLayerMenu : TimelineMenu
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    public int XPos;
    public TimelineLayer Layer;

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
                Text = "Add Clip",
                FontSize = 20,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Action = createClip
            }
        });
    }

    private void createClip()
    {
        var clip = new Game.ChatBox.Clips.Clip();

        var (lowerBound, upperBound) = Layer.GetBoundsNearestTo(XPos, false);

        clip.Start.Value = lowerBound;
        clip.End.Value = upperBound;
        clip.Priority.Value = Layer.Priority;

        chatBoxManager.Clips.Add(clip);
    }
}
