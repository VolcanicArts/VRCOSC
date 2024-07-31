using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRCOSC.App.Actions.Packages;
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
        SizeChanged += OnSizeChanged;

        SearchTextBox.TextChanged += (_, _) => filterDataGrid(SearchTextBox.Text);
        filterDataGrid(string.Empty);
    }

    private double packageScrollViewerHeight = double.NaN;

    public double PackageScrollViewerHeight
    {
        get => packageScrollViewerHeight;
        set
        {
            packageScrollViewerHeight = value;
            OnPropertyChanged();
        }
    }

    public Visibility UpdateAllButtonVisibility => PackageManager.GetInstance().Sources.Any(packageSource => packageSource.IsUpdateAvailable()) ? Visibility.Visible : Visibility.Collapsed;

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (AppManager.GetInstance().State.Value is AppManagerState.Starting or AppManagerState.Started)
        {
            await AppManager.GetInstance().StopAsync();
        }

        _ = MainWindow.GetInstance().ShowLoadingOverlay("Refreshing All Packages", PackageManager.GetInstance().RefreshAllSources(true));
    }

    private void UpdateAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        var updateAllPackagesAction = new UpdateAllPackagesAction();

        foreach (var packageSource in PackageManager.GetInstance().Sources.Where(packageSource => packageSource.IsUpdateAvailable()))
        {
            updateAllPackagesAction.AddAction(PackageManager.GetInstance().InstallPackage(packageSource));
        }

        _ = MainWindow.GetInstance().ShowLoadingOverlay("Updating All Packages", updateAllPackagesAction);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private int itemsCount;

    private void evaluateContentHeight()
    {
        if (itemsCount == 0)
        {
            PackageScrollViewerHeight = 0;
            return;
        }

        var contentHeight = PackageList.ActualHeight;
        var targetHeight = GridContainer.ActualHeight - 45;
        PackageScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }

    private void filterDataGrid(string filterText)
    {
        var packageManager = PackageManager.GetInstance();

        if (string.IsNullOrEmpty(filterText))
        {
            showDefaultItemsSource();
            return;
        }

        var filteredItems = packageManager.Sources.Where(item => item.DisplayName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase)).ToList();

        PackageList.ItemsSource = filteredItems;
        itemsCount = filteredItems.Count;
        evaluateContentHeight();
    }

    private void showDefaultItemsSource()
    {
        var packageManager = PackageManager.GetInstance();

        PackageList.ItemsSource = packageManager.Sources;
        itemsCount = packageManager.Sources.Count;
        evaluateContentHeight();
    }

    public void Refresh() => Dispatcher.Invoke(() =>
    {
        OnPropertyChanged(nameof(UpdateAllButtonVisibility));
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
        action.OnComplete += Refresh;
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Installing {packageSource.DisplayName} - {packageSource.LatestVersion}", action);
    }

    private void UninstallButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var package = (PackageSource)element.Tag;

        var result = MessageBox.Show($"Are you sure you want to uninstall {package.DisplayName}?", "Uninstall Warning", MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes) return;

        var action = PackageManager.GetInstance().UninstallPackage(package);
        action.OnComplete += Refresh;
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Uninstalling {package.DisplayName}", action);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void InstalledVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var element = (ComboBox)sender;
        var packageSource = (PackageSource)element.Tag;
        var packageRelease = packageSource.FilteredReleases[element.SelectedIndex];

        var action = PackageManager.GetInstance().InstallPackage(packageSource, packageRelease);
        action.OnComplete += Refresh;
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Installing {packageSource.DisplayName} - {packageRelease.Version}", action);
    }

    private void InstalledVersion_LostMouseCapture(object sender, MouseEventArgs e)
    {
        var comboBox = (WatermarkComboBox)sender;

        if (!comboBox.IsDropDownOpen)
            FocusTaker.Focus();
    }
}

