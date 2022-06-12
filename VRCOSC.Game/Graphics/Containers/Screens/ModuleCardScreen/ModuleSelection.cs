// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

[Cached]
public sealed class ModuleSelection : Container
{
    public Bindable<ModuleType> SelectedType = new();
    public BindableBool ShowExperimental = new();
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
        Children = new Drawable[]
        {
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 300),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new ModuleFilter
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MinValue
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
