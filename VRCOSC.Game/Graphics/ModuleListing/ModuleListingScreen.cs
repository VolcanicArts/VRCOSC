// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.ModuleEditing;
using VRCOSC.Game.Graphics.ModuleRun;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

[Cached]
public sealed class ModuleListingScreen : Container
{
    public Bindable<string> SearchTermFilter = new(string.Empty);
    public Bindable<ModuleType?> TypeFilter = new();

    // for some reason we can't use a nullable bindable here
    public Module? EditingModule;
    public Action<Module?>? OnEditingModuleChange;

    public ModuleListingScreen()
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
            new ModuleListing(),
            new ModuleEditingScreen(),
            new RunningPopover()
        };
    }

    public void EditModule(Module? module)
    {
        EditingModule = module;
        OnEditingModuleChange?.Invoke(module);
    }

    private sealed class ModuleListing : Container
    {
        public ModuleListing()
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
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray5
                },
                new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 50),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 50)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Header
                            {
                                Depth = float.MinValue
                            }
                        },
                        new Drawable[]
                        {
                            new Listing()
                        },
                        new Drawable[]
                        {
                            new Footer()
                        }
                    }
                }
            };
        }
    }
}
