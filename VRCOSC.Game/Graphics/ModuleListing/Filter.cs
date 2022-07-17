// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Filter : ClickableContainer
{
    public SortType SortType { get; init; }

    private SortDirection sortDirection = SortDirection.Ascending;
    private SpriteIcon arrowIcon = null!;

    [Resolved]
    private ModuleListingScreen moduleListingScreen { get; set; } = null!;

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
                                Text = SortType.ToString()
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
                            Child = arrowIcon = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Icon = FontAwesome.Solid.ArrowDown
                            }
                        }
                    }
                }
            }
        };

        Action += () =>
        {
            swapDirection();
            moduleListingScreen.SortFilter.Value.SortType = SortType;
            moduleListingScreen.SortFilter.Value.SortDirection = sortDirection;
            moduleListingScreen.SortFilter.TriggerChange();
        };
    }

    private void swapDirection()
    {
        sortDirection = sortDirection.Equals(SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;

        arrowIcon.RotateTo(sortDirection.Equals(SortDirection.Ascending) ? 180 : 0, 250, Easing.InOutQuart);
    }
}
