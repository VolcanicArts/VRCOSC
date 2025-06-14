// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Media;

namespace VRCOSC.App.UI.Views.Nodes;

public class GridBackgroundVisualHost : FrameworkElement
{
    private readonly double width;
    private readonly double height;
    private readonly double snapDistance;
    private readonly double significantSnapDistance;
    private readonly VisualCollection children;

    public GridBackgroundVisualHost(double width, double height, double snapDistance, double significantSnapDistance)
    {
        this.width = width;
        this.height = height;
        this.snapDistance = snapDistance;
        this.significantSnapDistance = significantSnapDistance;
        children = new VisualCollection(this);
        createLines();
    }

    private void createLines()
    {
        var visual = new DrawingVisual
        {
            CacheMode = new BitmapCache()
        };

        const double line_thickness = 1;

        using (var dc = visual.RenderOpen())
        {
            for (var i = 0; i <= width / snapDistance; i++)
            {
                var significant = i % (significantSnapDistance / snapDistance) == 0;
                var xPos = i * snapDistance;
                xPos += line_thickness / 2d;

                var brush = significant ? (Brush)FindResource("CForeground2") : (Brush)FindResource("CBackground1");
                brush = brush.Clone();
                brush.Opacity = significant ? 0.5f : 1f;
                brush.Freeze();

                dc.DrawLine(new Pen(brush, line_thickness), new Point(xPos, 0), new Point(xPos, height));
            }

            for (var i = 0; i <= height / snapDistance; i++)
            {
                var significant = i % (significantSnapDistance / snapDistance) == 0;
                var yPos = i * snapDistance;
                yPos += line_thickness / 2d;

                var brush = significant ? (Brush)FindResource("CForeground2") : (Brush)FindResource("CBackground1");
                brush = brush.Clone();
                brush.Opacity = significant ? 0.5f : 1f;
                brush.Freeze();

                dc.DrawLine(new Pen(brush, line_thickness), new Point(0, yPos), new Point(width, yPos));
            }
        }

        children.Add(visual);
    }

    protected override int VisualChildrenCount => children.Count;
    protected override Visual GetVisualChild(int index) => children[index];
}