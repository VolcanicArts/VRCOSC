// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace VRCOSC.App.Utils;

public static class WPFUtils
{
    public static T? FindVisualParent<T>(this DependencyObject element, string name) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(element);

        if (parent is T parentAsType && ((FrameworkElement)parent).Name == name)
            return parentAsType;

        return FindVisualParent<T>(parent, name);
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

    public static void PositionWindow(Window window, DependencyObject parent, ScreenChoice screenChoice, HorizontalPosition horizontal, VerticalPosition vertical)
    {
        PositionWindow(window, Window.GetWindow(parent), screenChoice, horizontal, vertical);
    }

    public static void PositionWindow(Window window, Window? parent, ScreenChoice screenChoice, HorizontalPosition horizontal, VerticalPosition vertical)
    {
        var targetScreen = screenChoice switch
        {
            ScreenChoice.SameAsParent => Screen.FromHandle(new WindowInteropHelper(parent!).Handle),
            ScreenChoice.PrimaryScreen => Screen.PrimaryScreen,
            _ => throw new ArgumentOutOfRangeException()
        };

        Debug.Assert(targetScreen is not null);

        var workingArea = targetScreen.WorkingArea;

        double x = horizontal switch
        {
            HorizontalPosition.Left => workingArea.Left,
            HorizontalPosition.Center => workingArea.Left + (workingArea.Width - window.Width) / 2,
            HorizontalPosition.Right => workingArea.Right - window.Width,
            _ => throw new ArgumentOutOfRangeException()
        };

        double y = vertical switch
        {
            VerticalPosition.Top => workingArea.Top,
            VerticalPosition.Center => workingArea.Top + (workingArea.Height - window.Height) / 2,
            VerticalPosition.Bottom => workingArea.Bottom - window.Height,
            _ => throw new ArgumentOutOfRangeException()
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
    PrimaryScreen
}