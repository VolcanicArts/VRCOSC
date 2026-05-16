// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VRCOSC.App.UI.Views.Nodes.Controls;

public partial class GridBackground : Control
{
    public static readonly DependencyProperty CellSizeProperty =
        DependencyProperty.Register(nameof(CellSize), typeof(double), typeof(GridBackground), new PropertyMetadata(25d));

    public double CellSize
    {
        get => (double)GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public static readonly DependencyProperty MajorLineIntervalProperty =
        DependencyProperty.Register(nameof(MajorLineInterval), typeof(int), typeof(GridBackground), new PropertyMetadata(20));

    public int MajorLineInterval
    {
        get => (int)GetValue(MajorLineIntervalProperty);
        set => SetValue(MajorLineIntervalProperty, value);
    }

    public static readonly DependencyProperty LineThicknessProperty =
        DependencyProperty.Register(nameof(LineThickness), typeof(int), typeof(GridBackground), new PropertyMetadata(1));

    public int LineThickness
    {
        get => (int)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(GridBackground), new PropertyMetadata(Brushes.Transparent));

    public Brush BackgroundBrush
    {
        get => (Brush)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty MinorLineBrushProperty =
        DependencyProperty.Register(nameof(MinorLineBrush), typeof(Brush), typeof(GridBackground), new PropertyMetadata(Brushes.Black));

    public Brush MinorLineBrush
    {
        get => (Brush)GetValue(MinorLineBrushProperty);
        set => SetValue(MinorLineBrushProperty, value);
    }

    public static readonly DependencyProperty MajorLineBrushProperty =
        DependencyProperty.Register(nameof(MajorLineBrush), typeof(Brush), typeof(GridBackground), new PropertyMetadata(Brushes.White));

    public Brush MajorLineBrush
    {
        get => (Brush)GetValue(MajorLineBrushProperty);
        set => SetValue(MajorLineBrushProperty, value);
    }

    private DrawingBrush? _cachedBrush;

    protected override void OnRender(DrawingContext context)
    {
        _cachedBrush ??= createGridBrush();
        context.DrawRectangle(_cachedBrush, null, new Rect(RenderSize));
    }

    private DrawingBrush createGridBrush()
    {
        var tileSize = CellSize * MajorLineInterval;
        var lineOffset = LineThickness / 2d;

        var group = new DrawingGroup();

        // background
        group.Children.Add(new GeometryDrawing
        {
            Brush = BackgroundBrush,
            Geometry = new RectangleGeometry(new Rect(0, 0, tileSize, tileSize))
        });

        // minor lines
        var minorGeom = new GeometryGroup();

        for (var i = 1; i < MajorLineInterval; i++)
        {
            var p = i * CellSize - lineOffset;
            minorGeom.Children.Add(new LineGeometry(new Point(p, 0), new Point(p, tileSize)));
            minorGeom.Children.Add(new LineGeometry(new Point(0, p), new Point(tileSize, p)));
        }

        group.Children.Add(new GeometryDrawing
        {
            Pen = new Pen(MinorLineBrush, LineThickness),
            Geometry = minorGeom
        });

        // major lines
        var majorGeom = new GeometryGroup();
        majorGeom.Children.Add(new LineGeometry(new Point(lineOffset, 0), new Point(lineOffset, tileSize)));
        majorGeom.Children.Add(new LineGeometry(new Point(0, lineOffset), new Point(tileSize, lineOffset)));

        group.Children.Add(new GeometryDrawing
        {
            Pen = new Pen(MajorLineBrush, LineThickness),
            Geometry = majorGeom
        });

        // outer border
        var outerRect = new RectangleGeometry(new Rect(lineOffset, lineOffset, tileSize - LineThickness, tileSize - LineThickness));

        group.Children.Add(new GeometryDrawing
        {
            Pen = new Pen(MajorLineBrush, LineThickness),
            Geometry = outerRect
        });

        return new DrawingBrush
        {
            Drawing = group,
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, tileSize, tileSize),
            ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, tileSize, tileSize),
            ViewboxUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top
        };
    }
}