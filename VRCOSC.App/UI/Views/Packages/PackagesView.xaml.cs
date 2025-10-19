// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.Actions;
using VRCOSC.App.Packages;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.UI.Windows.Packages;
using VRCOSC.App.Utils;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace VRCOSC.App.UI.Views.Packages;

public sealed partial class PackagesView
{
    private WindowManager windowManager = null!;

    public PackagesView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;

        SearchTextBox.TextChanged += (_, _) => filterDataGrid(SearchTextBox.Text);
        filterDataGrid(string.Empty);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        windowManager = new WindowManager(this);
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            var app = AppManager.GetInstance();

            if (app.State.Value is AppManagerState.Starting or AppManagerState.Started)
                await app.StopAsync();

            await MainWindow.GetInstance().ShowLoadingOverlay(
                new CallbackProgressAction("Refreshing All Packages",
                    () => PackageManager.GetInstance().RefreshAllSources(true)));
        }
    }

    private void filterDataGrid(string filterText)
    {
        var packageManager = PackageManager.GetInstance();

        if (string.IsNullOrEmpty(filterText))
        {
            PackageList.ItemsSource = PackageManager.GetInstance().Sources;
            return;
        }

        var filteredItems = packageManager.Sources.Where(item => item.DisplayName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase)).ToList();

        PackageList.ItemsSource = filteredItems;
    }

    public void Refresh() => Dispatcher.Invoke(() =>
    {
        PackageList.ItemsSource = null;
        filterDataGrid(SearchTextBox.Text);
    });

    private void InstallButton_OnClick(object sender, RoutedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            var element = (FrameworkElement)sender;
            var packageSource = (PackageSource)element.Tag;

            await MainWindow.GetInstance().ShowLoadingOverlay(new CallbackProgressAction($"Installing {packageSource.DisplayName}", () => PackageManager.GetInstance().InstallPackage(packageSource)));
        }
    }

    private void UninstallButton_OnClick(object sender, RoutedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            var element = (FrameworkElement)sender;
            var packageSource = (PackageSource)element.Tag;

            var result = MessageBox.Show($"Are you sure you want to uninstall {packageSource.DisplayName}?", "Uninstall Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await MainWindow.GetInstance().ShowLoadingOverlay(new CallbackProgressAction($"Uninstalling {packageSource.DisplayName}", () => PackageManager.GetInstance().UninstallPackage(packageSource)));
        }
    }

    private void InstalledVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        run().Forget();
        return;

        async Task run()
        {
            var element = (ComboBox)sender;
            var packageSource = (PackageSource)element.Tag;
            var packageRelease = packageSource.FilteredReleases[element.SelectedIndex];

            await MainWindow.GetInstance().ShowLoadingOverlay(new CallbackProgressAction($"Installing {packageSource.DisplayName} - {packageRelease.Version}", () => PackageManager.GetInstance().InstallPackage(packageSource, packageRelease)));
        }
    }

    private void InstalledVersion_LostMouseCapture(object sender, MouseEventArgs e)
    {
        var comboBox = (WatermarkComboBox)sender;

        if (!comboBox.IsDropDownOpen)
            FocusTaker.Focus();
    }

    private void InfoButton_ButtonClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var packageSource = (PackageSource)element.Tag;

        var infoWindow = new PackageInfoWindow(packageSource);
        windowManager.TrySpawnChild(infoWindow);
    }
}

public class PackageReleaseVersionColourConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PackageRelease packageRelease)
        {
            return packageRelease.IsPreRelease ? Brushes.Chocolate : Brushes.Black;
        }

        return Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}