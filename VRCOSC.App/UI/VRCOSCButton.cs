// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI;

public enum ButtonColour
{
    Green,
    Blue,
    Red,
    Gray,
    None
}

public class VRCOSCButton : Button
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(VRCOSCButton), new PropertyMetadata(new CornerRadius(5)));

    public static readonly DependencyProperty ButtonColourProperty =
        DependencyProperty.Register(nameof(ButtonColour), typeof(ButtonColour), typeof(VRCOSCButton), new PropertyMetadata(ButtonColour.None));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public ButtonColour ButtonColour
    {
        get => (ButtonColour)GetValue(ButtonColourProperty);
        set => SetValue(ButtonColourProperty, value);
    }

    public object ButtonBackgroundNormal => buttonColourToBrush(false);
    public object ButtonBackgroundLight => buttonColourToBrush(true);

    static VRCOSCButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(VRCOSCButton), new FrameworkPropertyMetadata(typeof(VRCOSCButton)));
    }

    public VRCOSCButton()
    {
        Style = (Style)FindResource("VRCOSCButtonStyle");
        IsEnabledChanged += OnIsEnabledChanged;
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Opacity = (bool)e.NewValue ? 1d : 0.5d;
    }

    private Brush buttonColourToBrush(bool isLight) => ButtonColour switch
    {
        ButtonColour.Green => isLight ? (Brush)FindResource("CGreenL") : (Brush)FindResource("CGreen"),
        ButtonColour.Blue => isLight ? (Brush)FindResource("CBlueL") : (Brush)FindResource("CBlue"),
        ButtonColour.Red => isLight ? (Brush)FindResource("CRedL") : (Brush)FindResource("CRed"),
        ButtonColour.Gray => isLight ? (Brush)FindResource("CGrayL") : (Brush)FindResource("CGray"),
        ButtonColour.None => Brushes.Transparent,
        _ => throw new ArgumentOutOfRangeException(nameof(ButtonColour), ButtonColour, null)
    };
}
