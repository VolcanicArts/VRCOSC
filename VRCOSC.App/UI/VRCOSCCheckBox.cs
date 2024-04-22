// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI;

public class VRCOSCCheckBox : CheckBox
{
    public static readonly DependencyProperty IconMarginProperty =
        DependencyProperty.Register(nameof(IconMargin), typeof(Thickness), typeof(VRCOSCCheckBox), new PropertyMetadata(new Thickness(5f)));

    public Thickness IconMargin
    {
        get => (Thickness)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(VRCOSCCheckBox), new PropertyMetadata(new CornerRadius(5f)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
