﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace VRCOSC.App.Utils;

public static class WPFUtils
{
    public static T? FindVisualParent<T>(this DependencyObject element, string name) where T : DependencyObject
    {
        while (true)
        {
            var parent = VisualTreeHelper.GetParent(element);

            if (parent is null) return null;
            if (parent is T parentAsType && ((FrameworkElement)parent).Name == name) return parentAsType;

            element = parent;
        }
    }

    public static T? FindVisualChild<T>(this DependencyObject element, string name) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);

            if (child is T childAsType && ((FrameworkElement)child).Name == name)
                return childAsType;

            var childOfChild = FindVisualChild<T>(child, name);

            if (childOfChild is not null)
                return childOfChild;
        }

        return null;
    }

    public static void SetPosition(this Window window, DependencyObject? parent, ScreenChoice screenChoice, HorizontalPosition horizontal, VerticalPosition vertical)
    {
        if (screenChoice == ScreenChoice.SameAsParent)
        {
            if (parent is null) throw new InvalidOperationException("Cannot copy parent's window's screen if parent is null");

            var parentWindow = Window.GetWindow(parent);
            window.SetPosition(parentWindow, screenChoice, horizontal, vertical);
        }
        else
        {
            window.SetPosition((Window?)null, screenChoice, horizontal, vertical);
        }
    }

    public static void SetPosition(this Window window, Window? parentWindow, ScreenChoice screenChoice, HorizontalPosition horizontal, VerticalPosition vertical)
    {
        Screen targetScreen;

        if (screenChoice == ScreenChoice.SameAsParent)
        {
            if (parentWindow is null) throw new InvalidOperationException("Cannot copy parent's screen if parent is null");

            targetScreen = Screen.FromHandle(new WindowInteropHelper(parentWindow).Handle);
        }
        else
        {
            targetScreen = Screen.PrimaryScreen!;
        }

        using var g = Graphics.FromHwnd(IntPtr.Zero);
        var dpiScale = g.DpiX / 96f;

        var workingArea = targetScreen.WorkingArea;

        var x = horizontal switch
        {
            HorizontalPosition.Left => workingArea.Left / dpiScale,
            HorizontalPosition.Center => (workingArea.Left + (workingArea.Width - window.Width * dpiScale) / 2) / dpiScale,
            HorizontalPosition.Right => (workingArea.Right - window.Width * dpiScale) / dpiScale,
            _ => throw new ArgumentOutOfRangeException(nameof(horizontal), "Invalid horizontal position")
        };

        var y = vertical switch
        {
            VerticalPosition.Top => workingArea.Top / dpiScale,
            VerticalPosition.Center => (workingArea.Top + (workingArea.Height - window.Height * dpiScale) / 2) / dpiScale,
            VerticalPosition.Bottom => (workingArea.Bottom - window.Height * dpiScale) / dpiScale,
            _ => throw new ArgumentOutOfRangeException(nameof(vertical), "Invalid vertical position")
        };

        window.Left = x;
        window.Top = y;
    }
}

public enum HorizontalPosition
{
    Left,
    Center,
    Right
}

public enum VerticalPosition
{
    Top,
    Center,
    Bottom
}

public enum ScreenChoice
{
    SameAsParent,
    Primary
}