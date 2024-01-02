// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.Graphics.UI.List;
using VRCOSC.Graphics.UI.Text;
using VRCOSC.Router;

namespace VRCOSC.Screens.Main.Router;

public partial class RouterListInstance : HeightLimitedScrollableListItem
{
    public Route Route { get; }

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    public RouterListInstance(Route route)
    {
        Route = route;
        Children = new Drawable[]
        {
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 40,
                Padding = new MarginPadding(5),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.2f),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            new RouterListStringTextBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                ValidCurrent = route.Name.GetBoundCopy()
                            },
                            null,
                            new RouterListIPEndPointTextBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                ValidCurrent = route.Endpoint.GetBoundCopy()
                            },
                            null,
                            new IconButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Size = new Vector2(30),
                                FillMode = FillMode.Fit,
                                Icon = FontAwesome.Solid.Minus,
                                BackgroundColour = Colours.RED0,
                                CornerRadius = 5,
                                Enabled = { Value = true },
                                Action = () => appManager.RouterManager.Routes.Remove(route)
                            }
                        }
                    }
                }
            }
        };
    }

    private partial class RouterListStringTextBox : StringTextBox
    {
        public RouterListStringTextBox()
        {
            BackgroundUnfocused = Colours.GRAY1;
            BackgroundFocused = Colours.GRAY1;
            BackgroundCommit = Colours.GRAY1;
        }
    }

    private partial class RouterListIPEndPointTextBox : IPEndPointTextBox
    {
        public RouterListIPEndPointTextBox()
        {
            BackgroundUnfocused = Colours.GRAY1;
            BackgroundFocused = Colours.GRAY1;
            BackgroundCommit = Colours.GRAY1;
        }
    }
}
