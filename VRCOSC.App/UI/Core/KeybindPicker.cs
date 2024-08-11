// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

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

    static KeybindPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KeybindPicker), new FrameworkPropertyMetadata(typeof(KeybindPicker)));
    }

    public KeybindPicker()
    {
        MouseDown += KeybindPicker_OnMouseDown;
        PreviewKeyDown += KeybindPicker_OnPreviewKeyDown;
    }

    private void KeybindPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        e.Handled = true;
    }

    private void KeybindPicker_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsKeyboardFocused)
        {
            Keyboard.ClearFocus();
            return;
        }

        if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
            e.Key != Key.LeftShift && e.Key != Key.RightShift &&
            e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
            e.Key != Key.LWin && e.Key != Key.RWin)
        {
            var key = e.Key;

            var keybind = new Keybind
            {
                Modifiers = Keyboard.Modifiers,
                Key = key
            };

            Keybind = keybind;

            e.Handled = true;
            Keyboard.ClearFocus();
        }
    }
}

public class KeybindToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Keybind keybind)
        {
            var modifiersString = string.Empty;

            if (keybind.Modifiers.HasFlag(ModifierKeys.Control))
                modifiersString += "Ctrl + ";

            if (keybind.Modifiers.HasFlag(ModifierKeys.Shift))
                modifiersString += "Shift + ";

            if (keybind.Modifiers.HasFlag(ModifierKeys.Alt))
                modifiersString += "Alt + ";

            if (keybind.Modifiers.HasFlag(ModifierKeys.Windows))
                modifiersString += "Win + ";

            return modifiersString + keybind.Key.ToReadableString();
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
