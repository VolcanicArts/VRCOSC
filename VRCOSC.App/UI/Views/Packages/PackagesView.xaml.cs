// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.Actions;
using VRCOSC.App.Packages;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace VRCOSC.App.UI.Views.Packages;

public sealed partial class PackagesView
{
    public PackagesView()
    {
        InitializeComponent();

        DataContext = this;

        SearchTextBox.TextChanged += (_, _) => filterDataGrid(SearchTextBox.Text);
        filterDataGrid(string.Empty);
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (AppManager.GetInstance().State.Value is AppManagerState.Starting or AppManagerState.Started)
        {
            await AppManager.GetInstance().StopAsync();
        }

        _ = MainWindow.GetInstance().ShowLoadingOverlay("Refreshing All Packages", new DynamicAsyncProgressAction("Refreshing Packages", () => PackageManager.GetInstance().RefreshAllSources(true)));
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

    private void InfoButton_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var packageSource = (PackageSource)button.Tag;

        InfoImageContainer.Visibility = Visibility.Collapsed;

        ImageLoader.RetrieveFromURL(packageSource.CoverURL, (bitmapImage, cached) =>
        {
            InfoImage.ImageSource = bitmapImage;
            InfoImageContainer.FadeInFromZero(cached ? 0 : 150);
        });

        InfoOverlay.DataContext = packageSource;
        InfoOverlay.FadeInFromZero(150);
    }

    private void PackageGithub_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var packageSource = (PackageSource)button.Tag;

        new Uri(packageSource.URL).OpenExternally();
    }

    private void InfoOverlayExit_ButtonClick(object sender, RoutedEventArgs e)
    {
        InfoOverlay.FadeOutFromOne(150);
    }

    private void InstallButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var packageSource = (PackageSource)element.Tag;

        var action = PackageManager.GetInstance().InstallPackage(packageSource);
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Installing {packageSource.DisplayName}", action);
    }

    private void UninstallButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var package = (PackageSource)element.Tag;

        var result = MessageBox.Show($"Are you sure you want to uninstall {package.DisplayName}?", "Uninstall Warning", MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes) return;

        var action = PackageManager.GetInstance().UninstallPackage(package);
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Uninstalling {package.DisplayName}", action);
    }

    private void InstalledVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var element = (ComboBox)sender;
        var packageSource = (PackageSource)element.Tag;
        var packageRelease = packageSource.FilteredReleases[element.SelectedIndex];

        var action = PackageManager.GetInstance().InstallPackage(packageSource, packageRelease);
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Installing {packageSource.DisplayName} - {packageRelease.Version}", action);
    }

    private void InstalledVersion_LostMouseCapture(object sender, MouseEventArgs e)
    {
        var comboBox = (WatermarkComboBox)sender;

        if (!comboBox.IsDropDownOpen)
            FocusTaker.Focus();
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