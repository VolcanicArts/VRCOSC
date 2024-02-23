// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Modules;

namespace VRCOSC.App.Pages.Modules;

public partial class ModuleSettingsWindow
{
    private readonly Module module;

    public ModuleSettingsWindow(Module module)
    {
        InitializeComponent();

        this.module = module;
        DataContext = module;
    }
}
