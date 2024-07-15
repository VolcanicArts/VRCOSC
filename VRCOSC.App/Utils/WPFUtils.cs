// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
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
}
