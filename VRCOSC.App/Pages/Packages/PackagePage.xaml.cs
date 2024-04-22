// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using VRCOSC.App.Packages;
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
            fadeIn(InfoImageContainer, cached ? 0 : 150);
        });

        InfoOverlay.DataContext = packageSource;
        fadeIn(InfoOverlay, 150);
    }

    private void PackageGithub_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var packageSource = (PackageSource)button.Tag;

        Process.Start(new ProcessStartInfo(packageSource.URL) { UseShellExecute = true });
    }

    private void InfoOverlayExit_ButtonClick(object sender, RoutedEventArgs e)
    {
        fadeOut(InfoOverlay, 150);
    }

    private void fadeIn(FrameworkElement grid, double fadeInTimeMilli) => Dispatcher.Invoke(() =>
    {
        grid.Visibility = Visibility.Visible;
        grid.Opacity = 0;

        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(fadeInTimeMilli)
        };

        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Begin(grid);
    });

    private void fadeOut(FrameworkElement grid, double fadeOutTime) => Dispatcher.Invoke(() =>
    {
        grid.Opacity = 1;

        var fadeOutAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(fadeOutTime)
        };

        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeOutAnimation);
        storyboard.Completed += (_, _) => grid.Visibility = Visibility.Collapsed;
        storyboard.Begin(grid);
    });
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
