// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterHeader : Container
{
    private const string vrcosc_router_wiki_url = @"https://github.com/VolcanicArts/VRCOSC/wiki/VRCOSC-Router";

    [Resolved]
    private GameHost host { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        TextFlowContainer textFlow;

        Children = new Drawable[]
        {
            new GridContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Relative, 0.8f),
                    new Dimension()
                },
                Content = new[]
                {
                    new[]
                    {
                        null,
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            CornerRadius = 10,
                            BorderThickness = 2,
                            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = ThemeManager.Current[ThemeAttribute.Dark],
                                    RelativeSizeAxes = Axes.Both,
                                },
                                textFlow = new TextFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding
                                    {
                                        Bottom = 5,
                                        Horizontal = 10
                                    },
                                    TextAnchor = Anchor.TopCentre
                                }
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Padding = new MarginPadding(5),
                            Child = new IconButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Circular = true,
                                IconShadow = true,
                                Icon = FontAwesome.Solid.Question,
                                BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                                Action = () => host.OpenUrlExternally(vrcosc_router_wiki_url)
                            }
                        },
                    }
                }
            }
        };

        textFlow.AddText("Router", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 40);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.AddParagraph("Define port routing to route other programs through VRCOSC to VRChat and vice versa", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }
}
