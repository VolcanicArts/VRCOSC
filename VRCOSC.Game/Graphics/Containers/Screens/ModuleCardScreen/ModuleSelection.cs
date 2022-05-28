// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

[Cached]
public class ModuleSelection : Container
{
    private const int module_group_selection_width = 200;

    public Bindable<ModuleType?> SelectedType = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, module_group_selection_width),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new ModuleGroupSelection
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        },
                        new ModuleListing
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both
                        },
                    }
                }
            }
        };
    }
}
