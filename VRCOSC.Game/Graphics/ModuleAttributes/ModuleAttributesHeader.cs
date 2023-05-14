// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public partial class ModuleAttributesHeader : BaseHeader
{
    [Resolved(name: "EditingModule")]
    private Bindable<Module?>? editingModule { get; set; }

    protected override string Title => editingModule?.Value?.Title ?? string.Empty;
    protected override string SubTitle => "Change this module's behaviour. Only edit the parameter names if you know what you're doing!";

    protected override void LoadComplete()
    {
        base.LoadComplete();

        editingModule!.BindValueChanged(e =>
        {
            if (e.NewValue is not null) GenerateText();
        }, true);
    }
}
