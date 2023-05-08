// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed partial class ModuleEditingPopover : PopoverScreen
{
    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    public ModuleEditingPopover()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Mid]
            },
            new ModuleEditingContent
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Vertical = 2.5f
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        editingModule.BindValueChanged(e =>
        {
            if (e.NewValue is null)
            {
                if (e.OldValue is not null)
                    gameManager.ModuleManager.Save(e.OldValue);

                Hide();
            }
            else
            {
                Show();
            }
        }, true);
    }

    protected override void Close()
    {
        editingModule.Value = null;
    }
}
