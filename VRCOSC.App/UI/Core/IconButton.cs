// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using FontAwesome6;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class IconButton : VRCOSCButton
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(EFontAwesomeIcon), typeof(IconButton), new PropertyMetadata(EFontAwesomeIcon.None));

    public EFontAwesomeIcon Icon
    {
        get => (EFontAwesomeIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
