// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;

namespace VRCOSC;

public class SpacedStackPanel : StackPanel
{
    public static readonly DependencyProperty SPACING_PROPERTY =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SpacedStackPanel), new PropertyMetadata(5.0));

    public double Spacing
    {
        get => (double)GetValue(SPACING_PROPERTY);
        set => SetValue(SPACING_PROPERTY, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var sizeAvailable = base.MeasureOverride(constraint);

        foreach (UIElement child in InternalChildren)
        {
            if (child != InternalChildren[^1])
            {
                child.Measure(new Size(child.DesiredSize.Width + Spacing, child.DesiredSize.Height));
            }
        }

        return sizeAvailable;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var x = 0d;

        foreach (UIElement child in InternalChildren)
        {
            if (child != InternalChildren[^1])
            {
                child.Arrange(new Rect(x, 0, child.DesiredSize.Width, arrangeSize.Height));
                x += child.DesiredSize.Width + Spacing;
            }
            else
            {
                child.Arrange(new Rect(x, 0, child.DesiredSize.Width, arrangeSize.Height));
            }
        }

        return arrangeSize;
    }
}
