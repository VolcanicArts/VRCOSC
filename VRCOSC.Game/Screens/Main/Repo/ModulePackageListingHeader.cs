// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageListingHeader : Container
{
    private readonly int[] columnWidths;

    public ModulePackageListingHeader(int[] columnWidths)
    {
        this.columnWidths = columnWidths;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.Highlight
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(3),
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, columnWidths[0]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[1]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[2]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[3]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[4]),
                        new Dimension()
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20, weight: "Bold"),
                                Text = "Name"
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20, weight: "Bold"),
                                Text = "Latest"
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20, weight: "Bold"),
                                Text = "Installed"
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20, weight: "Bold"),
                                Text = "Type"
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20, weight: "Bold"),
                                Text = "Actions"
                            },
                            null
                        }
                    }
                }
            }
        };
    }
}
