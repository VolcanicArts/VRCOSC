// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Windows;
using VRCOSC.App.Packages;

namespace VRCOSC.App.Pages.Packages;

public partial class PackagePage : IVRCOSCPage
{
    public PackagePage()
    {
        InitializeComponent();

        var packageManager = PackageManager.GetInstance();

        PackageGrid.DataContext = packageManager;
        SizeChanged += OnSizeChanged;

        SearchTextBox.TextChanged += (_, _) => filterDataGrid(SearchTextBox.Text);
        filterDataGrid(string.Empty);

        AppManager.GetInstance().RegisterPage(PageLookup.Packages, this);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private int itemsCount;

    private void evaluateContentHeight()
    {
        var packageManager = PackageManager.GetInstance();

        var contentHeight = itemsCount * 50;
        var targetHeight = GridContainer.ActualHeight - 55;
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

        if (!filteredItems.Any())
        {
            showDefaultItemsSource();
            return;
        }

        PackageGrid.ItemsSource = filteredItems;
        itemsCount = filteredItems.Count;
        evaluateContentHeight();
    }

    private void showDefaultItemsSource()
    {
        var packageManager = PackageManager.GetInstance();

        PackageGrid.ItemsSource = packageManager.Sources;
        itemsCount = packageManager.Sources.Count;
        evaluateContentHeight();
    }

    public void Refresh()
    {
        PackageGrid.ItemsSource = null;
        filterDataGrid(SearchTextBox.Text);
    }
}
