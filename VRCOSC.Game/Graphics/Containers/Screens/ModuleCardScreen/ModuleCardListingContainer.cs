// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleCardListingContainer : Container
{
    private const float footer_height = 60;

    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        VRCOSCScrollContainer<ModuleCardGroupContainer> moduleCardGroupScroll;

        InternalChildren = new Drawable[]
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
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, footer_height)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        moduleCardGroupScroll = new VRCOSCScrollContainer<ModuleCardGroupContainer>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    new Drawable[]
                    {
                        new ModuleCardListingFooter
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            }
        };

        ModuleManager.Modules.ForEach(pair =>
        {
            var (moduleType, modules) = pair;

            moduleCardGroupScroll.Add(new ModuleCardGroupContainer
            {
                ModuleType = moduleType,
                Modules = modules
            });
        });
    }
}
