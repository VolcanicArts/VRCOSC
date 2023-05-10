// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterScreen : Container
{
    private const string vrcosc_router_wiki_url = @"https://github.com/VolcanicArts/VRCOSC/wiki/VRCOSC-Router";

    [Resolved]
    private GameHost host { get; set; } = null!;

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
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = ThemeManager.Current[ThemeAttribute.Dark],
                            RelativeSizeAxes = Axes.Both
                        },
                        new BasicScrollContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ClampExtension = 0,
                            ScrollbarVisible = false,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Size = new Vector2(75),
                                    Padding = new MarginPadding(10),
                                    Child = new IconButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        Circular = true,
                                        Icon = FontAwesome.Solid.Question,
                                        BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                                        Action = () => host.OpenUrlExternally(vrcosc_router_wiki_url)
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5),
                                    Padding = new MarginPadding(10),
                                    Children = new Drawable[]
                                    {
                                        new RouterHeader
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre
                                        },
                                        routerDataFlow = new FillFlowContainer<RouterDataFlowEntry>
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Full
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

        routerManager.Store.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (RouterData newRouterData in e.NewItems)
                {
                    var drawableRouterData = new DrawableRouterData(newRouterData);

                    if (routerDataFlow.Count > 1)
                    {
                        drawableRouterData.Position = routerDataFlow[^1].Position;
                    }

                    routerDataFlow.Add(drawableRouterData);
                    routerDataFlow.SetLayoutPosition(drawableRouterData, 0);
                }
            }

            routerDataFlow.LayoutEasing = Easing.OutQuad;
            routerDataFlow.LayoutDuration = 150;

            Scheduler.AddDelayed(() =>
            {
                routerDataFlow.LayoutEasing = Easing.None;
                routerDataFlow.LayoutDuration = 0;
            }, 150);
        }, true);
    }
}
