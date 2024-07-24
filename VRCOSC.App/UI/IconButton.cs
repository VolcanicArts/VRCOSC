// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using FontAwesome6;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI;

public class IconButton : VRCOSCButton
{
    public static readonly DependencyProperty IconMarginProperty =
        DependencyProperty.Register(nameof(IconMargin), typeof(Thickness), typeof(IconButton), new PropertyMetadata(new Thickness(5f)));

    public Thickness IconMargin
    {
        get => (Thickness)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(EFontAwesomeIcon), typeof(IconButton), new PropertyMetadata(EFontAwesomeIcon.None));

    public EFontAwesomeIcon Icon
    {
        get => (EFontAwesomeIcon)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }
}
