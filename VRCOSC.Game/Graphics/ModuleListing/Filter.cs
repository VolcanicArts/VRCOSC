// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Filter : Container
{
    public string Text { get; init; }
    public IconUsage Icon { get; init; }

    public Filter()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 5;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray2
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.5f),
                    new Dimension(GridSizeMode.Relative, 0.5f),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(2.5f),
                            Child = new TextFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                TextAnchor = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                Text = Text
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            FillMode = FillMode.Fit,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(2.5f),
                            Size = new Vector2(0.5f),
                            Child = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Icon = Icon
                            }
                        }
                    }
                }
            }
        };
    }
}
