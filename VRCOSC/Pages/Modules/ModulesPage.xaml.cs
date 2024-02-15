// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Pages.Modules;

public partial class ModulesPage
{
    private readonly ModuleViewModel moduleViewModel = new();

    public ModulesPage()
    {
        InitializeComponent();

        StackPanel.DataContext = moduleViewModel;

        AppManager.GetInstance().RegisterPage(PageLookup.Modules, this);
    }
}
