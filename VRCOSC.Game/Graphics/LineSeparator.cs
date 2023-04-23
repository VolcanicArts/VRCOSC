// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics;

public sealed partial class LineSeparator : CircularContainer
{
    public Colour4 LineColour
    {
        get => Child.Colour;
        set => Child.Colour = value;
    }

    public LineSeparator()
    {
        RelativeSizeAxes = Axes.X;
        Size = new Vector2(0.95f, 5);
        Masking = true;

        Child = new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = ThemeManager.Current[ThemeAttribute.Darker]
        };
    }
}
