// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCOSC.App.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public partial class ConnectionViewModel : GraphElementViewModel
{
    public IConnection Connection { get; }

    [ObservableProperty]
    private Path? path;

    [ObservableProperty]
    private bool isVisible;

    [ObservableProperty]
    private Point startPoint;

    [ObservableProperty]
    private Point endPoint;

    public ConnectionViewModel(IConnection connection)
    {
        Connection = connection;
        ZIndex = -1;
    }

    public void CreatePath()
    {
        var relativeStart = StartPoint;
        var relativeEnd = EndPoint;

        var startColor = Connection is IValueConnection valueConnection1
            ? valueConnection1.OutputType.GetTypeColor()
            : Brushes.DeepSkyBlue.Color;

        var endColor = Connection is IValueConnection valueConnection2
            ? valueConnection2.InputType.GetTypeColor()
            : Brushes.DeepSkyBlue.Color;

        var (controlPoint1, controlPoint2) = getBezierControlPoints(relativeStart, relativeEnd);

        var pathFigure = new PathFigure
        {
            StartPoint = relativeStart,
            Segments = { new BezierSegment(controlPoint1, controlPoint2, relativeEnd, true) }
        };

        Path = new Path
        {
            Data = new PathGeometry { Figures = { pathFigure } },
            StrokeThickness = 2,
            Stroke = createGradientBrush(relativeStart, relativeEnd, startColor, endColor)
        };
    }

    private static (Point cp1, Point cp2) getBezierControlPoints(Point startPoint, Point endPoint)
    {
        var minDelta = double.Min(double.Abs(endPoint.Y - startPoint.Y) / 2d, 50d);
        var delta = double.Max(double.Abs(endPoint.X - startPoint.X) * 0.5d, minDelta);

        return (Point.Add(startPoint, new Vector(delta, 0)), Point.Add(endPoint, new Vector(-delta, 0)));
    }

    private static LinearGradientBrush createGradientBrush(Point startPoint, Point endPoint, System.Windows.Media.Color startColor, System.Windows.Media.Color endColor)
    {
        var x = double.Min(startPoint.X, endPoint.X);
        var y = double.Min(startPoint.Y, endPoint.Y);
        var width = double.Abs(endPoint.X - startPoint.X);
        var height = double.Abs(endPoint.Y - startPoint.Y);

        width = width == 0 ? 1 : width;
        height = height == 0 ? 1 : height;

        var bounds = new Rect(x, y, width, height);

        var relativeStart = new Point((startPoint.X - bounds.X) / bounds.Width,
            (startPoint.Y - bounds.Y) / bounds.Height);

        var relativeEnd = new Point((endPoint.X - bounds.X) / bounds.Width,
            (endPoint.Y - bounds.Y) / bounds.Height);

        return new LinearGradientBrush(startColor, endColor, relativeStart, relativeEnd)
        {
            MappingMode = BrushMappingMode.RelativeToBoundingBox
        };
    }
}