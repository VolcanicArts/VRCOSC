// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI;

public class SpacedStackPanel : StackPanel
{
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SpacedStackPanel), new PropertyMetadata(5.0));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var sizeAvailable = base.MeasureOverride(constraint);

        if (Orientation == Orientation.Horizontal)
        {
            double totalChildWidth = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    child.Measure(constraint);
                    totalChildWidth += child.DesiredSize.Width;
                }
            }

            totalChildWidth += Spacing * (InternalChildren.Cast<UIElement>().Count(c => c.Visibility == Visibility.Visible) - 1);

            totalChildWidth = Math.Max(totalChildWidth, 0);

            return new Size(totalChildWidth, sizeAvailable.Height);
        }
        else // Vertical Orientation
        {
            double totalChildHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    child.Measure(new Size(constraint.Width, double.PositiveInfinity));
                    totalChildHeight += child.DesiredSize.Height;
                }
            }

            totalChildHeight += Spacing * (InternalChildren.Cast<UIElement>().Count(c => c.Visibility == Visibility.Visible) - 1);

            return new Size(sizeAvailable.Width, totalChildHeight);
        }
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Orientation == Orientation.Horizontal)
        {
            double x = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    var childRect = new Rect(x, 0, child.DesiredSize.Width, arrangeSize.Height);
                    child.Arrange(childRect);
                    x += child.DesiredSize.Width + Spacing;
                }
            }

            return arrangeSize;
        }
        else // Vertical Orientation
        {
            double y = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    var childRect = new Rect(0, y, arrangeSize.Width, child.DesiredSize.Height);
                    child.Arrange(childRect);
                    y += child.DesiredSize.Height + Spacing;
                }
            }

            return arrangeSize;
        }
    }
}
