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

    private readonly List<Key> Modifiers = [];
    private readonly List<Key> Keys = [];
    private int keyDownCount;

    static KeybindPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KeybindPicker), new FrameworkPropertyMetadata(typeof(KeybindPicker)));
    }

    public KeybindPicker()
    {
        MouseDown += KeybindPicker_OnMouseDown;
        MouseUp += KeybindPicker_OnMouseUp;
        PreviewKeyDown += KeybindPicker_OnPreviewKeyDown;
        PreviewKeyUp += KeybindPicker_OnPreviewKeyUp;
    }

    private void KeybindPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        e.Handled = true;

        Modifiers.Clear();
        Keys.Clear();
        keyDownCount = 0;
    }

    private void KeybindPicker_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void KeybindPicker_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        keyDownCount--;

        if (keyDownCount != 0) return;

        if (Modifiers.Count == 0 && Keys.Count == 1 && Keys[0] == Key.Escape)
        {
            Keybind = new Keybind();
        }
        else
        {
            Keybind = new Keybind
            {
                Modifiers = Modifiers.ToList(),
                Keys = Keys.ToList()
            };
        }

        Keyboard.ClearFocus();
    }

    private void KeybindPicker_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (e.IsRepeat) return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LWin or Key.RWin) return;

        keyDownCount++;

        if (Modifiers.Contains(key) || Keys.Contains(key)) return;

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt)
        {
            Modifiers.Add(key);
        }
        else
        {
            Keys.Add(key);
        }
    }
}

public class KeybindToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Keybind keybind)
        {
            if (keybind.Modifiers.Count == 0 && keybind.Keys.Count == 0) return "None";

            return string.Join(" + ", keybind.Modifiers.Concat(keybind.Keys).Select(key => key.ToReadableString()));
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}