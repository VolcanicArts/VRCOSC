// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Header : Container
{
    public Header()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding
        {
            Top = 5,
            Bottom = 2.5f
        };
    }

    [BackgroundDependencyLoader]
    private void load(ModuleListingScreen moduleListingScreen)
    {
        Children = new Drawable[]
        {
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.5f),
                    new Dimension(GridSizeMode.Relative, 0.25f),
                    new Dimension(GridSizeMode.Relative, 0.25f),
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
                                Left = 5,
                                Right = 2.5f
                            },
                            Child = new FilterCriteria()
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Left = 2.5f,
                                Right = 2.5f
                            },
                            Child = new TypeFilter()
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Left = 2.5f,
                                Right = 5
                            },
                            Child = new VRCOSCTextBox
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 5,
                                PlaceholderText = "Search",
                                Current = moduleListingScreen.SearchTermFilter
                            }
                        }
                    }
                }
            }
        };
    }
}
