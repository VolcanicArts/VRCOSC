// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class SpacedStackPanel : StackPanel
{
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SpacedStackPanel), new PropertyMetadata(0d, SpacingChanged));

    private static void SpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => Application.Current.Dispatcher.Invoke(() =>
    {
        if (d is SpacedStackPanel control) control.InvalidateMeasure();
    });

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var sizeAvailable = base.MeasureOverride(constraint);

        var visibleChildCount = 0;

        if (Orientation == Orientation.Horizontal)
        {
            var totalChildWidth = 0d;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible) continue;

                visibleChildCount++;
                child.Measure(constraint);
                totalChildWidth += child.DesiredSize.Width;
            }

            var totalSpacing = visibleChildCount > 1 ? Spacing * (visibleChildCount - 1) : 0;

            return new Size(totalChildWidth + totalSpacing, sizeAvailable.Height);
        }
        else
        {
            var totalChildHeight = 0d;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible) continue;

                visibleChildCount++;
                child.Measure(new Size(constraint.Width, double.PositiveInfinity));
                totalChildHeight += child.DesiredSize.Height;
            }

            var totalSpacing = visibleChildCount > 1 ? Spacing * (visibleChildCount - 1) : 0;

            return new Size(sizeAvailable.Width, totalChildHeight + totalSpacing);
        }
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Orientation == Orientation.Horizontal)
        {
            var x = 0d;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible) continue;

                var childRect = new Rect(x, 0, child.DesiredSize.Width, arrangeSize.Height);
                child.Arrange(childRect);
                x += child.DesiredSize.Width + Spacing;
            }
        }
        else
        {
            var y = 0d;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible) continue;

                var childRect = new Rect(0, y, arrangeSize.Width, child.DesiredSize.Height);
                child.Arrange(childRect);
                y += child.DesiredSize.Height + Spacing;
            }
        }

        return arrangeSize;
    }
}