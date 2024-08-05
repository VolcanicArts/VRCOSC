// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.Modules;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.UI.Windows.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Modules;

public partial class ModulesView
{
    private ModuleSettingsWindow? moduleSettingsWindow;
    private ModuleParametersWindow? moduleParametersWindow;

    public ModulesView()
    {
        InitializeComponent();

        DataContext = ModuleManager.GetInstance();
    }

    private void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        WinForms.OpenFile("module.json|*.json", filePath => Dispatcher.Invoke(() => module.ImportConfig(filePath)));
    }

    private void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        var filePath = AppManager.GetInstance().Storage.GetFullPath($"profiles/{ProfileManager.GetInstance().ActiveProfile.Value.ID}/modules/{module.FullID}.json");
        WinForms.PresentFile(filePath);
    }

    private void ParametersButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (moduleParametersWindow is null)
        {
            moduleParametersWindow = new ModuleParametersWindow(module);
            moduleParametersWindow.Closed += (_, _) => moduleParametersWindow = null;
            moduleParametersWindow.Show();
        }
        else
        {
            moduleParametersWindow.Focus();
        }
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (moduleSettingsWindow is null)
        {
            moduleSettingsWindow = new ModuleSettingsWindow(module);
            moduleSettingsWindow.Closed += (_, _) => moduleSettingsWindow = null;
            moduleSettingsWindow.Show();
        }
        else
        {
            moduleSettingsWindow.Focus();
        }
    }

    private void InfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        module.InfoUrl.OpenExternally();
    }

    private void PrefabButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        new ModulePrefabsWindow(module).Show();
    }
}
