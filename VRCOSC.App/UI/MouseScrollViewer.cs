// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace VRCOSC.App.UI;

public class MouseScrollViewer : ScrollViewer
{
    public MouseScrollViewer()
    {
        PreviewMouseWheel += MouseScrollViewer_PreviewMouseWheel;
    }

    private void MouseScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.MiddleButton == MouseButtonState.Pressed)
        {
            if (e.Delta > 0)
            {
                LineUp();
            }
            else
            {
                LineDown();
            }
        }
    }
}

public sealed class IgnoreMouseWheelBehavior : Behavior<UIElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
        base.OnDetaching();
    }

    private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        };
        AssociatedObject.RaiseEvent(e2);
    }
}
