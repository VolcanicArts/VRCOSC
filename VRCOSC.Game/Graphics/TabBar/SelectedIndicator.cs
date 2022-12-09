// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.TabBar;

public partial class SelectedIndicator : Container
{
    public SelectedIndicator()
    {
        Child = new CircularContainer
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(0.1f, 0f),
            Masking = true,
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Accent]
            }
        };
    }

    public void Select()
    {
        Child.ResizeTo(new Vector2(0.1f, 0.75f), 100, Easing.OutQuad);
    }

    public void DeSelect()
    {
        Child.ResizeTo(Vector2.Zero, 200, Easing.OutQuad);
    }

    public void PreSelect()
    {
        Child.ResizeTo(new Vector2(0.1f, 0.25f), 200, Easing.OutQuad);
    }
}
