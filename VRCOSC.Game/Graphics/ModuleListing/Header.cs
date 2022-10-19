// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Header : Container
{
    public Header()
    {
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding
        {
            Vertical = 5
        };
    }

    [BackgroundDependencyLoader]
    private void load(VRCOSCGame game)
    {
        Children = new Drawable[]
        {
            new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.5f),
                    new Dimension(GridSizeMode.Relative, 0.5f)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Left = 5,
                                Right = 2.5f
                            },
                            Child = new TypeFilter()
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Left = 2.5f,
                                Right = 5
                            },
                            Child = new VRCOSCTextBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 5,
                                PlaceholderText = "Search",
                                Current = game.SearchTermFilter,
                                Masking = true,
                                BorderColour = Colour4.Black,
                                BorderThickness = 2
                            }
                        }
                    }
                }
            }
        };
    }
}
