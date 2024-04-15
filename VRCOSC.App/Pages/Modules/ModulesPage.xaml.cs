// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.Modules;
using VRCOSC.App.Pages.Modules.Parameters;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Modules;

public partial class ModulesPage
{
    private ModuleSettingsWindow? moduleSettingsWindow;
    private ModuleParametersWindow? moduleParametersWindow;

    public ModulesPage()
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

        var filePath = AppManager.GetInstance().Storage.GetFullPath($"profiles/{ProfileManager.GetInstance().ActiveProfile.Value.ID}/modules/{module.SerialisedName}.json");
        WinForms.PresentFile(filePath);
    }

    private void ParametersButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (moduleParametersWindow is null)
        {
            moduleParametersWindow = new ModuleParametersWindow(module);

            moduleParametersWindow.Closed += (_, _) =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow is null) return;

                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();

                moduleParametersWindow = null;
            };

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

            moduleSettingsWindow.Closed += (_, _) =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow is null) return;

                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();

                moduleSettingsWindow = null;
            };

            moduleSettingsWindow.Show();
        }
        else
        {
            moduleSettingsWindow.Focus();
        }
    }
}

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = System.Convert.ToInt32(value);
        return index % 2 == 0 ? Application.Current.Resources["CBackground3"] : Application.Current.Resources["CBackground4"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
