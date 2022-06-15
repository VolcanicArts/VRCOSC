// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleSelectionSidePanelContent : GridContainer
{
    public ModuleSelectionSidePanelContent()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RowDimensions = new[]
        {
            new Dimension(GridSizeMode.Absolute, 50),
            new Dimension()
        };
        Content = new[]
        {
            new Drawable[]
            {
                new ModuleSearchBar()
            },
            new Drawable[]
            {
                new ModuleOptionContainer()
            }
        };
    }
}
