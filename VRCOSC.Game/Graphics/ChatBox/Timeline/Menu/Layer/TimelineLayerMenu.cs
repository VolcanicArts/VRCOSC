// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Layer;

public partial class TimelineLayerMenu : TimelineMenu
{
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
        // get layer
        // create new clip
        // fill in info
    }
}
