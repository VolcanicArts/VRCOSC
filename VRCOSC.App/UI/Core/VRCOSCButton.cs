// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;

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
        DependencyProperty.Register(nameof(ButtonColour), typeof(ButtonColour), typeof(VRCOSCButton),
            new FrameworkPropertyMetadata(ButtonColour.None, FrameworkPropertyMetadataOptions.AffectsRender));

    public ButtonColour ButtonColour
    {
        get => (ButtonColour)GetValue(ButtonColourProperty);
        set => SetValue(ButtonColourProperty, value);
    }

    public VRCOSCButton()
    {
        IsEnabledChanged += OnIsEnabledChanged;
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Opacity = (bool)e.NewValue ? 1d : 0.35d;
    }
}