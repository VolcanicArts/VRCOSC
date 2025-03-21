// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.UI.Core;

public static class Extensions
{
    public static T? FindVisualParent<T>(this DependencyObject element) where T : FrameworkElement
    {
        while (true)
        {
            var parent = VisualTreeHelper.GetParent(element);

            switch (parent)
            {
                case null:
                    return null;

                case T parentAsType:
                    return parentAsType;

                default:
                    element = parent;
                    break;
            }
        }
    }

    public static T? FindVisualParent<T>(this DependencyObject element, string name) where T : FrameworkElement
    {
        while (true)
        {
            var parent = VisualTreeHelper.GetParent(element);

            switch (parent)
            {
                case null:
                    return null;

                case T parentAsType when parentAsType.Name == name:
                    return parentAsType;

                default:
                    element = parent;
                    break;
            }
        }
    }

    public static T? FindVisualParentFuzzy<T>(this DependencyObject element, string name) where T : FrameworkElement
    {
        while (true)
        {
            var parent = VisualTreeHelper.GetParent(element);

            switch (parent)
            {
                case null:
                    return null;

                case T parentAsType when parentAsType.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase):
                    return parentAsType;

                default:
                    element = parent;
                    break;
            }
        }
    }

    public static IEnumerable<T> FindVisualChildrenWhere<T>(this DependencyObject element, Func<T, bool> callback) where T : FrameworkElement
    {
        // TODO: Depth?

        var childrenOfType = new List<T>();

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);

            if (child is T childAsType && callback.Invoke(childAsType))
                childrenOfType.Add(childAsType);

            childrenOfType.AddRange(FindVisualChildrenWhere(child, callback));
        }

        return childrenOfType;
    }

    public static T? FindVisualChildWhere<T>(this DependencyObject element, Func<T, bool> callback) where T : FrameworkElement => element.FindVisualChildrenWhere(callback).FirstOrDefault();
    public static T? FindVisualChild<T>(this DependencyObject element, string name) where T : FrameworkElement => element.FindVisualChildWhere<T>(o => o.Name == name);
    public static T? FindVisualChildFuzzy<T>(this DependencyObject element, string name) where T : FrameworkElement => element.FindVisualChildWhere<T>(o => o.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));

    public static void FadeInFromZero(this FrameworkElement element, double durationMilliseconds, Action? onCompleted = null)
    {
        element.Opacity = 0;
        FadeIn(element, durationMilliseconds, onCompleted);
    }

    public static void FadeIn(this FrameworkElement element, double durationMilliseconds, Action? onCompleted = null)
    {
        element.Visibility = Visibility.Visible;

        var fadeInAnimation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 1d,
            Duration = TimeSpan.FromMilliseconds(durationMilliseconds)
        };

        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Completed += (_, _) => onCompleted?.Invoke();
        storyboard.Begin(element);
    }

    public static void FadeOutFromOne(this FrameworkElement element, double durationMilliseconds, Action? onCompleted = null)
    {
        element.Opacity = 1;
        FadeOut(element, durationMilliseconds, onCompleted);
    }

    public static void FadeOut(this FrameworkElement element, double durationMilliseconds, Action? onCompleted = null)
    {
        var fadeOutAnimation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 0d,
            Duration = TimeSpan.FromMilliseconds(durationMilliseconds)
        };

        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeOutAnimation);

        storyboard.Completed += (_, _) =>
        {
            element.Visibility = Visibility.Collapsed;
            onCompleted?.Invoke();
        };
        storyboard.Begin(element);
    }

    public static void AnimateHeight(this FrameworkElement element, double targetHeight, double durationMilliseconds, IEasingFunction easingFunction, Action? onCompleted = null)
    {
        var currentHeight = element.Height;

        element.Height = currentHeight;
        element.UpdateLayout();

        var heightAnimation = new DoubleAnimation
        {
            From = currentHeight,
            To = targetHeight,
            Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
            EasingFunction = easingFunction
        };

        Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(heightAnimation);

        storyboard.Completed += (_, _) =>
        {
            if (double.IsNaN(targetHeight)) element.Height = double.NaN;
            onCompleted?.Invoke();
        };

        storyboard.Begin(element);
    }
}