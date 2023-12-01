// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.UI.List;

public partial class HeightLimitedScrollableListItem : Container
{
    protected override Container<Drawable> Content { get; }

    protected readonly Box Background;

    protected HeightLimitedScrollableListItem()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChildren = new Drawable[]
        {
            Background = new Box
            {
                RelativeSizeAxes = Axes.Both
            },
            Content = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            }
        };
    }

    public void SetBackground(Colour4 colour)
    {
        Background.Colour = colour;
    }
}
