// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Graphics;

namespace VRCOSC.Screens.Main.Run;

public partial class RunTab : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY1
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.3f),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            new TerminalContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 5
                            },
                            null,
                            new GridContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                RowDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 5),
                                    new Dimension(GridSizeMode.Absolute, 60)
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new ViewContainer
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Masking = true,
                                            CornerRadius = 5
                                        }
                                    },
                                    null,
                                    new Drawable[]
                                    {
                                        new ControlsContainer
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Masking = true,
                                            CornerRadius = 5
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
