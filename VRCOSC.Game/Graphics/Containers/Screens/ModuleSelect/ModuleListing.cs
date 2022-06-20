// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Drawables.Triangles;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleListing : Container
{
    public ModuleListing()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load(ModuleManager moduleManager, ModuleSelection moduleSelection)
    {
        SearchContainer<ModuleListingGroup> moduleGroupFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 20,
                ScrollbarVisible = false,
                Child = moduleGroupFlow = new SearchContainer<ModuleListingGroup>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical
                }
            }
        };

        moduleManager.ForEach(moduleGroup => moduleGroupFlow.Add(new ModuleListingGroup(moduleGroup)));
        moduleSelection.SearchString.ValueChanged += searchTerm => moduleGroupFlow.SearchTerm = searchTerm.NewValue;
    }
}
