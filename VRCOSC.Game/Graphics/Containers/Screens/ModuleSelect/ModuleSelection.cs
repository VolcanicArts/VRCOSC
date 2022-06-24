// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect.SidePanel;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

[Cached]
public sealed class ModuleSelection : GridContainer
{
    public Bindable<string> SearchString = new();

    public ModuleSelection()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ColumnDimensions = new[]
        {
            new Dimension(GridSizeMode.Absolute, 300),
            new Dimension()
        };
        Content = new[]
        {
            new Drawable[]
            {
                new ModuleSelectSidePanel(),
                new ModuleListing()
            }
        };
    }
}
