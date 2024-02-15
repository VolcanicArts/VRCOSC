// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Windows;
using VRCOSC.Packages;

namespace VRCOSC.Pages.Packages;

public partial class PackagePage
{
    public PackagePage()
    {
        InitializeComponent();

        var packageManager = PackageManager.GetInstance();

        PackageGrid.DataContext = packageManager;
        SizeChanged += OnSizeChanged;

        SearchTextBox.TextChanged += (_, _) =>
        {
            var text = SearchTextBox.Text;
            filterDataGrid(text);
        };

        filterDataGrid(string.Empty);

        AppManager.GetInstance().RegisterPage(PageLookup.Packages, this);
    }

    private int itemCount;

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private void evaluateContentHeight()
    {
        var packageManager = PackageManager.GetInstance();

        var contentHeight = itemCount * 50;
        var targetHeight = GridContainer.ActualHeight - 55;
        packageManager.PackageScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }

    private void filterDataGrid(string filterText)
    {
        var filteredItems = string.IsNullOrEmpty(filterText) ? PackageManager.GetInstance().Sources.ToList() : PackageManager.GetInstance().Sources.Where(item => item.DisplayName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase)).ToList();

        itemCount = filteredItems.Count;
        PackageGrid.ItemsSource = filteredItems.Any() ? filteredItems : PackageManager.GetInstance().Sources;
        evaluateContentHeight();
    }
}
