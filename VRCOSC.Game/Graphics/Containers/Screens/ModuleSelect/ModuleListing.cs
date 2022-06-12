// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Drawables.Triangles;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public class ModuleListing : Container
{
    private SearchContainer moduleCardFlow;

    [Resolved]
    private ModuleSelection moduleSelection { get; set; }

    [Resolved]
    private ModuleManager moduleManager { get; set; }

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
            moduleCardFlow = new SearchContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            }
        };

        populateModules();
        moduleSelection.SearchString.BindValueChanged(e => moduleCardFlow.SearchTerm = e.NewValue, true);
    }

    private void populateModules()
    {
        moduleManager.ForEach(moduleGroup => moduleGroup.ForEach(moduleContainer => moduleCardFlow.Add(new ModuleCard(moduleContainer.Module))));
    }
}
