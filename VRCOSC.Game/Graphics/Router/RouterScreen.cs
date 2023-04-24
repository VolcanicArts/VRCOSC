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

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterScreen : Container
{
    private const string vrcosc_router_wiki_url = @"https://github.com/VolcanicArts/VRCOSC/wiki/VRCOSC-Router";

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private RouterManager routerManager { get; set; } = null!;

    private FillFlowContainer instanceFlow = null!;

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
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 20,
                ScrollbarVisible = false,
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5f),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = FrameworkFont.Regular.With(size: 60),
                            Text = "Router",
                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                        },
                        new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 20))
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Text = "This screen allows you to route data from other applications through VRCOSC to fix the port binding issues\n"
                                   + "This is functionally similar to OSCRouter, but has proper support for apps such as VRCFaceTracking",
                            Colour = ThemeManager.Current[ThemeAttribute.SubText]
                        },
                        instanceFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5)
                        },
                        new IconButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.3f,
                            Height = 30,
                            Icon = FontAwesome.Solid.Plus,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Darker],
                            CornerRadius = 10,
                            Action = addInstance
                        }
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(70),
                Padding = new MarginPadding(10),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.Question,
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                    IconShadow = true,
                    Masking = true,
                    CornerRadius = 25,
                    IconPadding = 6,
                    Action = () => host.OpenUrlExternally(vrcosc_router_wiki_url)
                }
            }
        };

        routerManager.Store.ForEach(routerData =>
        {
            instanceFlow.Add(new RouterInstance(routerData)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Width = 0.5f
            });
        });
    }

    private void addInstance()
    {
        var instance = routerManager.Create();

        instanceFlow.Add(new RouterInstance(instance)
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Width = 0.5f
        });
    }
}
