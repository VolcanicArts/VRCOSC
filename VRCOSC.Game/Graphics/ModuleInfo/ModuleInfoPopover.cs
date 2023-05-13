// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class ModuleInfoPopover : PopoverScreen
{
    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new ModuleInfoScreen
        {
            RelativeSizeAxes = Axes.Both
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        infoModule.ValueChanged += e =>
        {
            if (e.NewValue is null)
                Hide();
            else
                Show();
        };
    }

    protected override void Close()
    {
        infoModule.Value = null;
    }
}
