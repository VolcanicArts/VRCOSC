// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleListing : Container
{
    [Resolved]
    private VRCOSCConfigManager configManager { get; set; }

    private SearchContainer<ModuleListingGroup> moduleGroupFlow;

    public ModuleListing()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load(ModuleManager moduleManager, ModuleSelection moduleSelection)
    {
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
                    Padding = new MarginPadding(10),
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10)
                }
            }
        };

        moduleManager.ForEach(moduleGroup =>
        {
            var moduleListingGroup = new ModuleListingGroup(moduleGroup);

            var bitwise = configManager.Get<int>(VRCOSCSetting.Dropdowns);
            bool dropdownEnabled = (bitwise & (int)moduleGroup.Type) == (int)moduleGroup.Type;

            moduleListingGroup.State.Value = dropdownEnabled;
            moduleListingGroup.State.ValueChanged += _ => calculateBitwise();

            moduleGroupFlow.Add(moduleListingGroup);
        });

        moduleSelection.SearchString.ValueChanged += searchTerm => moduleGroupFlow.SearchTerm = searchTerm.NewValue;
    }

    private void calculateBitwise()
    {
        int bitwise = 0;

        moduleGroupFlow.ForEach(moduleListingGroup =>
        {
            if (moduleListingGroup.State.Value)
            {
                bitwise |= (int)moduleListingGroup.ModuleGroup.Type;
            }
        });

        configManager.SetValue(VRCOSCSetting.Dropdowns, bitwise);
    }
}
