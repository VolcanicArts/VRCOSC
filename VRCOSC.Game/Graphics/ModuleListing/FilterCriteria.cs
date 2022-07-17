// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class FilterCriteria : Container
{
    public FilterCriteria()
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
                Colour = VRCOSCColour.Gray3
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.25f),
                    new Dimension(GridSizeMode.Relative, 0.25f),
                    new Dimension(GridSizeMode.Relative, 0.25f),
                    new Dimension(GridSizeMode.Relative, 0.25f)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 20
                            },
                            Child = new Filter
                            {
                                Text = "Title",
                                Icon = FontAwesome.Solid.ArrowUp
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 20
                            },
                            Child = new Filter
                            {
                                Text = "Title",
                                Icon = FontAwesome.Solid.ArrowUp
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 20
                            },
                            Child = new Filter
                            {
                                Text = "Title",
                                Icon = FontAwesome.Solid.ArrowUp
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 20
                            },
                            Child = new Filter
                            {
                                Text = "Title",
                                Icon = FontAwesome.Solid.ArrowUp
                            }
                        }
                    }
                }
            }
        };
    }
}
