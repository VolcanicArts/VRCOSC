// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Graphics;

namespace VRCOSC.Screens.Main.Run;

public partial class ParameterListHeader : Container
{
    private readonly string title;

    public ParameterListHeader(string title)
    {
        this.title = title;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Child = new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY0
                },
                new TextFlowContainer(t =>
                {
                    t.Font = Fonts.BOLD.With(size: 30);
                    t.Colour = Colours.WHITE2;
                })
                {
                    RelativeSizeAxes = Axes.Both,
                    TextAnchor = Anchor.Centre,
                    Text = title
                }
            }
        };
    }
}
