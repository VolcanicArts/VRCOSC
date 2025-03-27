﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

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

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ButtonColourProperty =
        DependencyProperty.Register(nameof(ButtonColour), typeof(ButtonColour), typeof(VRCOSCButton), new PropertyMetadata(ButtonColour.None));

    public ButtonColour ButtonColour
    {
        get => (ButtonColour)GetValue(ButtonColourProperty);
        set => SetValue(ButtonColourProperty, value);
    }

    public Brush ButtonBackgroundNormal => buttonColourToBrush(false);
    public Brush ButtonBackgroundLight => buttonColourToBrush(true);

    public VRCOSCButton()
    {
        IsEnabledChanged += OnIsEnabledChanged;
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Opacity = (bool)e.NewValue ? 1d : 0.35d;
    }

    private Brush buttonColourToBrush(bool isLight) => ButtonColour switch
    {
        ButtonColour.Green => isLight ? (Brush)FindResource("CGreenL") : (Brush)FindResource("CGreen"),
        ButtonColour.Blue => isLight ? (Brush)FindResource("CBlueL") : (Brush)FindResource("CBlue"),
        ButtonColour.Red => isLight ? (Brush)FindResource("CRedL") : (Brush)FindResource("CRed"),
        ButtonColour.Gray => isLight ? (Brush)FindResource("CBackground8") : (Brush)FindResource("CBackground6"),
        ButtonColour.None => Brushes.Transparent,
        _ => throw new ArgumentOutOfRangeException(nameof(ButtonColour), ButtonColour, null)
    };
}

internal class ButtonBackgroundColourConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [VRCOSCButton button, bool isMouseOver, Brush]) return Brushes.Aqua;

        return isMouseOver ? button.ButtonBackgroundLight : button.ButtonBackgroundNormal;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [];
}