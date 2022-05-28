// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleListing : Container
{
    private const float header_height = 80;
    private const float footer_height = 60;

    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    [Resolved]
    private ModuleSelection ModuleSelection { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<ModuleCard> moduleListingFlow;
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, header_height),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, footer_height)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new ModuleListingHeader
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(10),
                            Child = new BasicScrollContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                ClampExtension = 20,
                                ScrollbarVisible = false,
                                Child = moduleListingFlow = new FillFlowContainer<ModuleCard>
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Full,
                                    Spacing = new Vector2(5, 10)
                                }
                            }
                        }
                    },
                    new Drawable[]
                    {
                        new ModuleListingFooter
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            }
        };

        ModuleSelection.SelectedType.BindValueChanged(e =>
        {
            var newType = e.NewValue;

            moduleListingFlow.Clear();

            ModuleManager.Modules.ForEach(pair =>
            {
                var (moduleType, modules) = pair;

                if (!moduleType.Equals(newType)) return;

                modules.ForEach(module =>
                {
                    moduleListingFlow.Add(new ModuleCard
                    {
                        SourceModule = module
                    });
                });
            });
        }, true);
    }
}
