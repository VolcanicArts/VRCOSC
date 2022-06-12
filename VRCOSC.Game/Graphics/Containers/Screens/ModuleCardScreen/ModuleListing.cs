// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Drawables.Triangles;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleListing : Container
{
    private const float header_height = 80;
    private const float footer_height = 60;

    private FillFlowContainer<ModuleCard> moduleListingFlow;

    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    [Resolved]
    private ModuleSelection ModuleSelection { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = VRCOSCColour.Gray4.Lighten(0.25f),
                ColourDark = VRCOSCColour.Gray4,
                TriangleScale = 5,
                Velocity = 0.5f
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
                    }
                }
            }
        };

        ModuleSelection.SelectedType.BindValueChanged(e =>
        {
            var newType = e.NewValue;
            updateWithType(newType);
        }, true);

        ModuleSelection.ShowExperimental.BindValueChanged(_ =>
        {
            moduleListingFlow.ForEach(moduleCard =>
            {
                if (!ModuleSelection.ShowExperimental.Value && moduleCard.SourceModule.Experimental)
                {
                    moduleCard.Hide();
                }
                else
                {
                    moduleCard.Show();
                }
            });
        });
    }

    private void updateWithType(ModuleType type)
    {
        moduleListingFlow.Clear();

        ModuleManager.ForEach(moduleGroup =>
        {
            if (!moduleGroup.Type.Equals(type)) return;

            moduleGroup.ForEach(moduleContainer =>
            {
                ModuleCard moduleCard;
                moduleListingFlow.Add(moduleCard = new ModuleCard
                {
                    SourceModule = moduleContainer.Module
                });

                if (!ModuleSelection.ShowExperimental.Value && moduleContainer.Module.Experimental) moduleCard.Hide();
            });
        });
    }
}
