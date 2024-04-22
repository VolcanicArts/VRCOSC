// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Packages;
using VRCOSC.App.UI;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Packages;

public partial class PackagePage
{
    public PackagePage()
    {
        InitializeComponent();

        var packageManager = PackageManager.GetInstance();

        DataContext = packageManager;
        SizeChanged += OnSizeChanged;

        SearchTextBox.TextChanged += (_, _) => filterDataGrid(SearchTextBox.Text);
        filterDataGrid(string.Empty);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private int itemsCount;

    private void evaluateContentHeight()
    {
        var packageManager = PackageManager.GetInstance();

        if (itemsCount == 0)
        {
            packageManager.PackageScrollViewerHeight = 0;
            return;
        }

        var contentHeight = PackageList.ActualHeight;
        var targetHeight = GridContainer.ActualHeight - 45;
        packageManager.PackageScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
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

        Process.Start(new ProcessStartInfo(packageSource.URL) { UseShellExecute = true });
    }

    private void InfoOverlayExit_ButtonClick(object sender, RoutedEventArgs e)
    {
        InfoOverlay.FadeOutFromOne(150);
    }
}
