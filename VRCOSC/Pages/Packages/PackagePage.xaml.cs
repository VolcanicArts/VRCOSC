// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.Packages;

namespace VRCOSC.Pages.Packages;

public partial class PackagePage
{
    public PackagePage()
    {
        InitializeComponent();

        var packageManager = PackageManager.GetInstance();
        packageManager.RefreshPackagePage += () => NavigationService?.Refresh();

        PackageGrid.DataContext = packageManager;
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var packageManager = PackageManager.GetInstance();

        var contentHeight = packageManager.Sources.Count * 50;
        var targetHeight = GridContainer.ActualHeight - 55;
        packageManager.PackageScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }
}
