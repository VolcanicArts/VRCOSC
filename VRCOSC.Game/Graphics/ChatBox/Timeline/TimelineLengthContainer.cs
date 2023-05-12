// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineLengthContainer : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];
        Shear = new Vector2(0.5f, 0);

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Dark],
                RelativeSizeAxes = Axes.Both
            },
            new IntegerValueContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Shear = new Vector2(-0.5f, 0)
            }
        };
    }
}
