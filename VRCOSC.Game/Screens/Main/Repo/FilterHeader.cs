// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class FilterHeader : Container
{
    private Box background = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 5;
        Children = new Drawable[]
        {
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY2
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Padding = new MarginPadding(3),
                Spacing = new Vector2(2, 0),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Child = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),
                            Icon = FontAwesome.Solid.ChevronDown,
                            Colour = Colours.GRAY3
                        }
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = Fonts.REGULAR.With(size: 27),
                        Text = "Filters",
                        Colour = Colours.GRAY3
                    }
                }
            }
        };
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(Colours.GRAY6, 100, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeColour(Colours.GRAY2, 100, Easing.OutQuint);
    }
}
