// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class ModuleInfoHeader : BaseHeader
{
    [Resolved(name: "InfoModule")]
    private Bindable<Module?>? infoModule { get; set; }

    protected override string Title => infoModule?.Value?.Title ?? string.Empty;
    protected override string SubTitle => infoModule?.Value?.Description ?? string.Empty;

    protected override void LoadComplete()
    {
        base.LoadComplete();

        infoModule!.BindValueChanged(e =>
        {
            if (e.NewValue is not null) GenerateText();
        }, true);
    }
}
