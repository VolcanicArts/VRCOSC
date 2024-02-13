// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;

namespace VRCOSC.Pages.Packages;

public partial class PackagePage
{
    private readonly PackageViewModel packageViewModel = new();

    public PackagePage()
    {
        InitializeComponent();

        PackageGrid.DataContext = packageViewModel;
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var contentHeight = packageViewModel.Packages.Count * 50;
        var targetHeight = GridContainer.ActualHeight - 55;
        packageViewModel.ScrollHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }
}
