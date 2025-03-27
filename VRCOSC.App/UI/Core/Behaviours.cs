// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace VRCOSC.App.UI.Core;

public partial class IpPortValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var input = value?.ToString() ?? string.Empty;
        return isValidIpPort(input) ? ValidationResult.ValidResult : new ValidationResult(false, "Invalid IP:Port format");
    }

    private bool isValidIpPort(string input) => ipPortRegex().IsMatch(input);

    [GeneratedRegex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):([0-9]{1,5})$")]
    private static partial Regex ipPortRegex();
}

public class IgnoreMouseWheelBehavior : Behavior<UIElement>
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

public class InterceptManipulationEventsBehavior : Behavior<UIElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.ManipulationStarting += OnPreviewManipulationStarting;
        AssociatedObject.ManipulationDelta += OnPreviewManipulationDelta;
        AssociatedObject.ManipulationInertiaStarting += OnPreviewManipulationInertiaStarting;
        AssociatedObject.ManipulationCompleted += OnPreviewManipulationCompleted;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.ManipulationStarting -= OnPreviewManipulationStarting;
        AssociatedObject.ManipulationDelta -= OnPreviewManipulationDelta;
        AssociatedObject.ManipulationInertiaStarting -= OnPreviewManipulationInertiaStarting;
        AssociatedObject.ManipulationCompleted -= OnPreviewManipulationCompleted;
        base.OnDetaching();
    }

    private void OnPreviewManipulationStarting(object? sender, ManipulationStartingEventArgs e)
    {
        if (sender is TextBox)
        {
            e.Handled = true;
        }
    }

    private void OnPreviewManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
    {
        if (sender is TextBox)
        {
            e.Handled = true;
        }
    }

    private void OnPreviewManipulationInertiaStarting(object? sender, ManipulationInertiaStartingEventArgs e)
    {
        if (sender is TextBox)
        {
            e.Handled = true;
        }
    }

    private void OnPreviewManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
    {
        if (sender is TextBox)
        {
            e.Handled = true;
        }
    }
}