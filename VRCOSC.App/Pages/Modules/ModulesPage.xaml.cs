// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Modules;

namespace VRCOSC.App.Pages.Modules;

public partial class ModulesPage : IVRCOSCPage
{
    public ModulesPage()
    {
        InitializeComponent();

        StackPanel.DataContext = ModuleManager.GetInstance();

        AppManager.GetInstance().RegisterPage(PageLookup.Modules, this);
    }

    public void Refresh()
    {
    }
}
