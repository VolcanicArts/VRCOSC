// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.ModuleAttributes;
using VRCOSC.Game.Graphics.ModuleInfo;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ModuleListing;

[Cached]
public sealed partial class ModulesScreen : Container
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private FillFlowContainer<ModuleCard> moduleCardFlow = null!;

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
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new ModulesHeader
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
                                                ScrollContent =
                                                {
                                                    Child = moduleCardFlow = new FillFlowContainer<ModuleCard>
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Padding = new MarginPadding(5),
                                                        Direction = FillDirection.Full,
                                                        Spacing = new Vector2(0, 5),
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
            },
            new ModuleAttributesPopover(),
            new ModuleInfoPopover()
        };
    }

    protected override void LoadComplete()
    {
        gameManager.ModuleManager.ForEach(module => moduleCardFlow.Add(new ModuleCard(module)));
    }
}
