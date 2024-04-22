// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace VRCOSC.App.UI;

public static class Extensions
{
    public static void FadeInFromZero(this FrameworkElement element, double durationMilliseconds)
    {
        element.Opacity = 0;
        FadeIn(element, durationMilliseconds);
    }

    public static void FadeIn(this FrameworkElement element, double durationMilliseconds)
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
        storyboard.Begin(element);
    }

    public static void FadeOutFromOne(this FrameworkElement element, double durationMilliseconds)
    {
        element.Opacity = 1;
        FadeOut(element, durationMilliseconds);
    }

    public static void FadeOut(this FrameworkElement element, double durationMilliseconds)
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
        storyboard.Completed += (_, _) => element.Visibility = Visibility.Collapsed;
        storyboard.Begin(element);
    }
}
