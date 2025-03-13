// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public partial class VRCOSCImage
{
    public VRCOSCImage()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty UrlProperty =
        DependencyProperty.Register(nameof(Url), typeof(string), typeof(VRCOSCImage), new PropertyMetadata(string.Empty, onUrlChanged));

    private static void onUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((VRCOSCImage)d).updateImage((string)e.NewValue);
    }

    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    private void updateImage(string url)
    {
        ImageLoader.RetrieveFromURL(url, (bitmapImage, cached) =>
        {
            Image.Source = bitmapImage;
            Image.FadeInFromZero(cached ? 0 : 150);
        });
    }
}