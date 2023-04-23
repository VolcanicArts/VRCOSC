// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineWrapper : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 10;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Dark],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Child = new TimelineEditor
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both
                }
            }
        };
    }
}
