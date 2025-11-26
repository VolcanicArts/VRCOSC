// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using VRCOSC.App.Modules;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Modules;

public partial class ModulesView
{
    private WindowManager settingsWindowManager = null!;
    private WindowManager parametersWindowManager = null!;
    private WindowManager prefabsWindowManager = null!;

    public ModulesView()
    {
        InitializeComponent();

        DataContext = ModuleManager.GetInstance();
    }

    private void ModulesView_OnLoaded(object sender, RoutedEventArgs e)
    {
        settingsWindowManager = new WindowManager(this);
        parametersWindowManager = new WindowManager(this);
        prefabsWindowManager = new WindowManager(this);
    }

    private async void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var element = (FrameworkElement)sender;
            var module = (Module)element.Tag;

            var filePath = await Platform.PickFileAsync(".json");
            if (filePath is null) return;

            module.ImportConfig(filePath).Forget();
        }
        catch (Exception ex)
        {
            ExceptionHandler.Handle(ex);
        }
    }

    private void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        var filePath = AppManager.GetInstance().Storage.GetFullPath($"profiles/{ProfileManager.GetInstance().ActiveProfile.Value.ID}/modules/{module.FullID}.json");
        Platform.PresentFile(filePath);
    }

    private void ParametersButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        parametersWindowManager.TrySpawnChild(new ModuleParametersWindow(module));
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (module.SettingsWindowType is not null)
        {
            var window = (Window)Activator.CreateInstance(module.SettingsWindowType, module)!;
            window.Closing += (_, _) => module.Serialise();
            settingsWindowManager.TrySpawnChild((IManagedWindow)window);
        }
        else
        {
            settingsWindowManager.TrySpawnChild(new ModuleSettingsWindow(module));
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

        prefabsWindowManager.TrySpawnChild(new ModulePrefabsWindow(module));
    }
}