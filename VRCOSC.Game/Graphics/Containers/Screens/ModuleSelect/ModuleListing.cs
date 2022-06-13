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

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public class ModuleListing : Container
{
    [Resolved]
    private ModuleSelection moduleSelection { get; set; }

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        SearchContainer<ModuleListingGroup> moduleGroupFlow;

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
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5)
                }
            }
        };

        moduleManager.ForEach(moduleGroup => moduleGroupFlow.Add(new ModuleListingGroup(moduleGroup)));
        moduleSelection.SearchString.ValueChanged += (searchTerm) => moduleGroupFlow.SearchTerm = searchTerm.NewValue;
    }
}
