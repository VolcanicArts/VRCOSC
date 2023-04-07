// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineLayer : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;

        Children = new Drawable[]
        {
            // new Box
            // {
            //     Colour = ThemeManager.Current[ThemeAttribute.Dark],
            //     RelativeSizeAxes = Axes.Both,
            // }
        };
    }

    public void Add(Clip clip)
    {
        Add(new DrawableClip(clip));
    }
}
