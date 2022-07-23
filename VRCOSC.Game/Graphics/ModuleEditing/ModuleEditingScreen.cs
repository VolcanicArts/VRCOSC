// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Input;
using VRCOSC.Game.Graphics.ModuleListing;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed class ModuleEditingScreen : Container
{
    [Resolved]
    private ModuleListingScreen moduleListingScreen { get; set; }

    [Cached]
    public Bindable<Module?> SourceModule { get; } = new();

    public ModuleEditingScreen()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;
        X = 1;
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
                Colour = VRCOSCColour.Gray4
            },
            new ModuleEditingContent
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            }
        };

        moduleListingScreen.OnEditingModuleChange += module =>
        {
            if (module == null)
                this.MoveToX(1, 1000, Easing.InQuint);
            else
                this.MoveToX(0, 1000, Easing.OutQuint);

            SourceModule.Value = module;
        };
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key != Key.Escape) return base.OnKeyDown(e);

        moduleListingScreen.EditModule(null);
        return true;
    }
}
