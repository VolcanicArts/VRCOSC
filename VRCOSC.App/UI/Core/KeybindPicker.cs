// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VRCOSC.App.SDK.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class KeybindPicker : UserControl
{
    public static readonly DependencyProperty KeybindProperty =
        DependencyProperty.Register(nameof(Keybind), typeof(Keybind), typeof(KeybindPicker), new PropertyMetadata(new Keybind()));

    public Keybind Keybind
    {
        get => (Keybind)GetValue(KeybindProperty);
        set => SetValue(KeybindProperty, value);
    }

    private readonly List<Key> Modifiers = [];
    private readonly List<Key> Keys = [];

    static KeybindPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KeybindPicker), new FrameworkPropertyMetadata(typeof(KeybindPicker)));
    }

    public KeybindPicker()
    {
        MouseDown += KeybindPicker_OnMouseDown;
        PreviewKeyDown += KeybindPicker_OnPreviewKeyDown;
        PreviewKeyUp += KeybindPicker_OnPreviewKeyUp;
    }

    private void KeybindPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        e.Handled = true;

        Modifiers.Clear();
        Keys.Clear();
    }

    private void KeybindPicker_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        Modifiers.Remove(e.Key);
        Keys.Remove(e.Key);
    }

    private void KeybindPicker_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.IsRepeat) return;

        if (e.Key == Key.Escape && IsKeyboardFocused)
        {
            Keyboard.ClearFocus();
            return;
        }

        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin)
        {
            Modifiers.Add(e.Key);
            return;
        }

        Keys.Add(e.Key);

        var keybind = new Keybind
        {
            Modifiers = Modifiers,
            Keys = Keys
        };

        Keybind = keybind;

        e.Handled = true;
        Keyboard.ClearFocus();
    }
}

public class KeybindToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Keybind keybind)
        {
            if (keybind.Modifiers.Count == 0 && keybind.Keys.Count == 0) return "None";

            return string.Join(" + ", keybind.Modifiers.Concat(keybind.Keys));
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}