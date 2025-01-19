// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using VRCOSC.App.Packages;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Packages;

public partial class PackageInfoWindow : IManagedWindow
{
    private readonly PackageSource packageSource;

    public PackageInfoWindow(PackageSource packageSource)
    {
        InitializeComponent();
        DataContext = packageSource;
        this.packageSource = packageSource;

        Title = $"{packageSource.DisplayName.Pluralise()} Info";

        ImageLoader.RetrieveFromURL(packageSource.CoverURL, (bitmapImage, cached) =>
        {
            InfoImage.ImageSource = bitmapImage;
            InfoImageContainer.FadeInFromZero(cached ? 0 : 150);
        });
    }

    private void PackageGithub_ButtonClick(object sender, RoutedEventArgs e)
    {
        new Uri(packageSource.URL).OpenExternally();
    }

    public object GetComparer() => packageSource;
}