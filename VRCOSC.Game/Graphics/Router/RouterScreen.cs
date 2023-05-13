// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterScreen : Container
{
    [Resolved]
    private RouterManager routerManager { get; set; } = null!;

    private FillFlowContainer<RouterDataFlowEntry> routerDataFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 65),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new RouterHeader
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre
                                }
                            },
                            null,
                            new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 10,
                                    BorderThickness = 2,
                                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = ThemeManager.Current[ThemeAttribute.Dark],
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        new Container
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(2),
                                            Child = new BasicScrollContainer
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                ClampExtension = 0,
                                                ScrollbarVisible = false,
                                                Child = routerDataFlow = new FillFlowContainer<RouterDataFlowEntry>
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Padding = new MarginPadding(5),
                                                    Direction = FillDirection.Full,
                                                    LayoutEasing = Easing.OutQuad,
                                                    LayoutDuration = 150
                                                }
                                            }
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

    protected override void LoadComplete()
    {
        var drawableRouterDataSpawner = new DrawableRouterDataSpawner();
        routerDataFlow.Add(drawableRouterDataSpawner);
        routerDataFlow.SetLayoutPosition(drawableRouterDataSpawner, 1);
        routerDataFlow.ChangeChildDepth(drawableRouterDataSpawner, float.MinValue);

        routerManager.Store.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (RouterData newRouterData in e.NewItems)
                {
                    var drawableRouterData = new DrawableRouterData(newRouterData);
                    drawableRouterData.Position = routerDataFlow[^1].Position;
                    routerDataFlow.Add(drawableRouterData);
                }
            }
        }, true);
    }
}
