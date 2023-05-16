// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public sealed partial class ModuleAttributesPopover : PopoverScreen
{
    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

    public ModuleAttributesPopover()
    {
        Child = new ModuleAttributesScreen
        {
            RelativeSizeAxes = Axes.Both
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        editingModule.BindValueChanged(e =>
        {
            if (e.NewValue is null)
                Hide();
            else
                Show();
        }, true);
    }

    protected override void Close()
    {
        editingModule.Value = null;
    }
}
