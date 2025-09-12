// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows.Nodes;
using VRCOSC.App.Utils;
using Expression = org.mariuszgromada.math.mxparser.Expression;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using RichTextBox = Xceed.Wpf.Toolkit.RichTextBox;
using TextBox = System.Windows.Controls.TextBox;
using Vector = System.Windows.Vector;

// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Global

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodeGraphView : INotifyPropertyChanged
{
    public const int SNAP_DISTANCE = 25;
    public const int SIGNIFICANT_SNAP_STEP = 20;
    public Padding GroupPadding { get; } = new(30, 55, 30, 30);
    public Padding SelectionPadding { get; } = new((int)(SNAP_DISTANCE * 0.75), (int)(SNAP_DISTANCE * 0.75), (int)(SNAP_DISTANCE * 0.75), (int)(SNAP_DISTANCE * 0.75));

    public const MouseButton GRAPH_DRAG_BUTTON = MouseButton.Middle;
    public const MouseButton GRAPH_ITEM_DRAG_BUTTON = MouseButton.Left;

    public NodeGraph Graph { get; }

    public ObservableCollection<GraphItem> GraphItems { get; } = [];
    public ObservableCollection<ConnectionItem> ConnectionItems { get; } = [];

    private WindowManager nodeCreatorWindowManager = null!;
    private WindowManager presetCreatorWindowManager = null!;

    private bool hasLoaded;
    private Point graphContextMenuPosition;

    private GraphDrag? graphDrag;
    private GraphItemDrag? graphItemDrag;
    private NodeGroupGraphItemDrag? nodeGroupGraphItemGrab;
    private ConnectionDrag? connectionDrag;
    private SelectionCreate? selectionCreate;
    private SelectionDrag? selectionDrag;

    private GraphItemSelection? selection;
    private bool contentVisible = true;

    public bool ContentVisible
    {
        get => contentVisible;
        set
        {
            if (value == contentVisible) return;

            contentVisible = value;
            OnPropertyChanged();
        }
    }

    public NodeGraphView(NodeGraph graph)
    {
        Graph = graph;

        InitializeComponent();
        DataContext = this;

        Loaded += OnLoaded;
        KeyDown += OnKeyDown;

        Graph.OnMarkedDirty += OnGraphMarkedDirty;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();

        if (hasLoaded) return;

        nodeCreatorWindowManager = new WindowManager(this);
        presetCreatorWindowManager = new WindowManager(this);

        Task.Run(Graph.MarkDirty);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            resetCanvasPosition();
        }
    }

    private async Task OnGraphMarkedDirty()
    {
        if (!hasLoaded)
        {
            // let the UI update for a bit to ensure animations stay smooth
            await Task.Delay(150);
        }

        if (Graph.RemovedNodes.Count != 0)
            GraphItems.RemoveIf(item => item is NodeGraphItem nodeGraphItem && Graph.RemovedNodes.Contains(nodeGraphItem.Node));

        if (Graph.RemovedConnections.Count != 0)
            ConnectionItems.RemoveIf(item => Graph.RemovedConnections.Contains(item.Connection));

        if (Graph.RemovedGroups.Count != 0)
            GraphItems.RemoveIf(item => item is NodeGroupGraphItem nodeGroupGraphItem && Graph.RemovedGroups.Contains(nodeGroupGraphItem.Group));

        var addedNodes = Graph.AddedNodes.Select(node => new NodeGraphItem(node)).ToList();
        var addedConnections = Graph.AddedConnections;
        var addedGroups = Graph.AddedGroups.Select(group => new NodeGroupGraphItem(group)).ToList();

        var offset = GraphItems.Count;

        await Dispatcher.InvokeAsync(() =>
        {
            if (!hasLoaded)
                drawGraphBackground();

            RefreshContextMenu();
            GraphItems.AddRange(addedNodes);
            GraphItems.AddRange(addedGroups);
        });

        await Dispatcher.InvokeAsync(() =>
        {
            for (var i = 0; i < addedNodes.Count; i++)
            {
                var nodeGraphItem = addedNodes[i];
                var itemContainer = (FrameworkElement)GraphItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                var nodeContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                nodeGraphItem.Element = nodeContainer;
                populateNodeGraphItem(nodeGraphItem);
                updateGraphItemPosition(nodeGraphItem, nodeGraphItem.Node.NodePosition);
            }

            for (var i = 0; i < addedGroups.Count; i++)
            {
                var nodeGroupGraphItem = addedGroups[i];
                var itemContainer = (FrameworkElement)GraphItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + addedNodes.Count + i);
                var groupContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                nodeGroupGraphItem.Element = groupContainer;
                updateNodeGroupGraphItem(nodeGroupGraphItem);
            }

            drawNodeConnections(addedConnections);
        }, DispatcherPriority.Render);

        if (!hasLoaded)
        {
            Dispatcher.Invoke(() =>
            {
                hasLoaded = true;
                LoadingOverlay.FadeOut(250);
            });
        }
    }

    public void RefreshContextMenu()
    {
        ContextMenuBuilder.Refresh();
        GraphContainer.ContextMenu!.Items.Clear();
        GraphContainer.ContextMenu!.Items.Add(ContextMenuBuilder.GraphCreateNodeContextSubMenu.Value);
        GraphContainer.ContextMenu!.Items.Add(ContextMenuBuilder.GraphPresetContextSubMenu.Value);
    }

    #region Graph Background

    private void drawGraphBackground()
    {
        var backgroundVisual = createTiledGrid();

        var brush = new VisualBrush(backgroundVisual)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, SNAP_DISTANCE * SIGNIFICANT_SNAP_STEP, SNAP_DISTANCE * SIGNIFICANT_SNAP_STEP),
            ViewportUnits = BrushMappingMode.Absolute,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            Stretch = Stretch.None
        };

        GraphBackground.Background = brush;
    }

    private DrawingVisual createTiledGrid()
    {
        const double tile_size = SNAP_DISTANCE * SIGNIFICANT_SNAP_STEP;

        const double line_thickness = 1.0;
        const double offset = line_thickness / 2.0;

        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        var minorPen = new Pen((Brush)FindResource("CBackground1"), line_thickness);
        var majorPen = new Pen((Brush)FindResource("CBackground8"), line_thickness);

        for (var i = 0; i <= SIGNIFICANT_SNAP_STEP; i++)
        {
            if (i % SIGNIFICANT_SNAP_STEP == 0) continue;

            var x = i * SNAP_DISTANCE + offset;
            dc.DrawLine(minorPen, new Point(x, 0), new Point(x, tile_size));
        }

        for (var i = 0; i <= SIGNIFICANT_SNAP_STEP; i++)
        {
            if (i % SIGNIFICANT_SNAP_STEP == 0) continue;

            var y = i * SNAP_DISTANCE + offset;
            dc.DrawLine(minorPen, new Point(0, y), new Point(tile_size, y));
        }

        for (var i = 0; i <= SIGNIFICANT_SNAP_STEP; i++)
        {
            if (i % SIGNIFICANT_SNAP_STEP != 0) continue;

            var x = i * SNAP_DISTANCE + offset;
            dc.DrawLine(majorPen, new Point(x, 0), new Point(x, tile_size));
        }

        for (var i = 0; i <= SIGNIFICANT_SNAP_STEP; i++)
        {
            if (i % SIGNIFICANT_SNAP_STEP != 0) continue;

            var y = i * SNAP_DISTANCE + offset;
            dc.DrawLine(majorPen, new Point(0, y), new Point(tile_size, y));
        }

        return visual;
    }

    #endregion

    #region Utility

    private void snapPointToGrid(ref Point point)
    {
        point.X = Math.Round(point.X / SNAP_DISTANCE) * SNAP_DISTANCE;
        point.Y = Math.Round(point.Y / SNAP_DISTANCE) * SNAP_DISTANCE;
    }

    private void deselectGraphItems()
    {
        selection = null;
        SelectionVisual.Visibility = Visibility.Collapsed;
    }

    private void drawNodeConnections(Node node)
    {
        var connections = Graph.Connections.Values.Where(c => c.InputNodeId == node.Id || c.OutputNodeId == node.Id);
        drawNodeConnections(connections);
    }

    private void drawNodeConnections(IEnumerable<NodeConnection> connections)
    {
        foreach (var connection in connections)
        {
            try
            {
                var foundConnection = ConnectionItems.SingleOrDefault(c => c == new ConnectionItem(connection, new Path()));

                if (foundConnection is not null)
                    updateConnectionItemPath(foundConnection);
                else
                    ConnectionItems.Add(new ConnectionItem(connection, createConnectionPath(connection)));
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }
    }

    private void populateNodeGraphItem(NodeGraphItem item)
    {
        if (item.Node.Metadata.IsFlowInput)
        {
            item.FlowInputs = new FrameworkElement[1];

            for (var i = 0; i < item.FlowInputs.Length; i++)
            {
                var slotName = $"flow_input_{i}";
                var slotElement = item.Element.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == slotName);
                Debug.Assert(slotElement is not null);
                item.FlowInputs[i] = slotElement;
            }
        }

        if (item.Node.Metadata.IsFlowOutput)
        {
            item.FlowOutputs = new FrameworkElement[item.Node.Metadata.FlowCount];

            for (var i = 0; i < item.FlowOutputs.Length; i++)
            {
                var slotName = $"flow_output_{i}";
                var slotElement = item.Element.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == slotName);
                Debug.Assert(slotElement is not null);
                item.FlowOutputs[i] = slotElement;
            }
        }

        if (item.Node.Metadata.IsValueInput)
        {
            item.ValueInputs = new FrameworkElement[item.Node.VirtualValueInputCount()];

            for (var i = 0; i < item.ValueInputs.Length; i++)
            {
                var slotName = $"value_input_{i}";
                var slotElement = item.Element.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == slotName);
                Debug.Assert(slotElement is not null);
                item.ValueInputs[i] = slotElement;
            }
        }

        if (item.Node.Metadata.IsValueOutput)
        {
            item.ValueOutputs = new FrameworkElement[item.Node.VirtualValueOutputCount()];

            for (var i = 0; i < item.ValueOutputs.Length; i++)
            {
                var slotName = $"value_output_{i}";
                var slotElement = item.Element.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == slotName);
                Debug.Assert(slotElement is not null);
                item.ValueOutputs[i] = slotElement;
            }
        }

        populateNodeGraphItemSnapOffset(item);
    }

    private void populateNodeGraphItemSnapOffset(NodeGraphItem item)
    {
        var snapElement = getSnappingSlotElement(item);
        var snapElementPos = snapElement.TranslatePoint(new Point(0, 0), GraphContainer) + new Vector(snapElement.ActualWidth / 2d, snapElement.ActualHeight / 2d);

        item.SnapOffset = new Vector(snapElementPos.X - item.PosX, snapElementPos.Y - item.PosY);
    }

    private FrameworkElement getSnappingSlotElement(NodeGraphItem item)
    {
        if (item.Node.Metadata.IsFlowOutput)
            return item.FlowOutputs[0];

        if (item.Node.Metadata.IsFlowInput)
            return item.FlowInputs[0];

        if (item.Node.Metadata.IsValueOutput)
            return item.ValueOutputs[0];

        if (item.Node.Metadata.IsValueInput)
            return item.ValueInputs[0];

        throw new InvalidOperationException();
    }

    private FrameworkElement getOutputSlotElementForConnection(NodeConnection connection)
    {
        var nodeGraphItem = GraphItems.OfType<NodeGraphItem>().Single(item => item.Node.Id == connection.OutputNodeId);
        return connection.ConnectionType == ConnectionType.Flow ? nodeGraphItem.FlowOutputs[connection.OutputSlot] : nodeGraphItem.ValueOutputs[connection.OutputSlot];
    }

    private FrameworkElement getInputSlotElementForConnection(NodeConnection connection)
    {
        var nodeGraphItem = GraphItems.OfType<NodeGraphItem>().Single(item => item.Node.Id == connection.InputNodeId);
        return connection.ConnectionType == ConnectionType.Flow ? nodeGraphItem.FlowInputs[connection.InputSlot] : nodeGraphItem.ValueInputs[connection.InputSlot];
    }

    private void updateConnectionItemPath(ConnectionItem item)
    {
        var path = item.Path;
        var connection = item.Connection;

        var startPoint = getConnectionPointRelativeToGraph(getOutputSlotElementForConnection(connection));
        var endPoint = getConnectionPointRelativeToGraph(getInputSlotElementForConnection(connection));

        var (controlPoint1, controlPoint2) = getBezierControlPoints(startPoint, endPoint);

        var pathGeometry = (PathGeometry)path.Data;
        var pathFigure = pathGeometry.Figures[0];
        var bezierSegment = (BezierSegment)pathFigure.Segments[0];

        pathFigure.StartPoint = startPoint;
        bezierSegment.Point1 = controlPoint1;
        bezierSegment.Point2 = controlPoint2;
        bezierSegment.Point3 = endPoint;

        if (connection.ConnectionType == ConnectionType.Value)
        {
            var startColor = connection.OutputType!.GetTypeBrush().Color;
            var endColor = connection.InputType!.GetTypeBrush().Color;
            path.Stroke = createGradientBrush(startPoint, endPoint, startColor, endColor);
        }
    }

    private Path createConnectionPath(NodeConnection connection)
    {
        var startPoint = getConnectionPointRelativeToGraph(getOutputSlotElementForConnection(connection));
        var endPoint = getConnectionPointRelativeToGraph(getInputSlotElementForConnection(connection));

        var (controlPoint1, controlPoint2) = getBezierControlPoints(startPoint, endPoint);

        var pathFigure = new PathFigure
        {
            StartPoint = startPoint,
            Segments = { new BezierSegment(controlPoint1, controlPoint2, endPoint, true) }
        };

        var path = new Path
        {
            Data = new PathGeometry { Figures = { pathFigure } },
            StrokeThickness = 3
        };

        if (connection.ConnectionType == ConnectionType.Flow)
        {
            path.Stroke = Brushes.DeepSkyBlue;
        }
        else
        {
            var startColor = connection.OutputType!.GetTypeBrush().Color;
            var endColor = connection.InputType!.GetTypeBrush().Color;
            path.Stroke = createGradientBrush(startPoint, endPoint, startColor, endColor);
        }

        return path;
    }

    private Point getConnectionPointRelativeToGraph(FrameworkElement element)
    {
        var centerOffset = new Vector(element.Width / 2d, element.Height / 2d);
        return element.TranslatePoint(new Point(0, 0), GraphContainer) + centerOffset;
    }

    private (Point cp1, Point cp2) getBezierControlPoints(Point startPoint, Point endPoint)
    {
        var minDelta = Math.Min(Math.Abs(endPoint.Y - startPoint.Y) / 2d, 50d);
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.5d, minDelta);

        return (Point.Add(startPoint, new Vector(delta, 0)), Point.Add(endPoint, new Vector(-delta, 0)));
    }

    private LinearGradientBrush createGradientBrush(Point startPoint, Point endPoint, Color startColor, Color endColor)
    {
        var x = Math.Min(startPoint.X, endPoint.X);
        var y = Math.Min(startPoint.Y, endPoint.Y);
        var width = Math.Abs(endPoint.X - startPoint.X);
        var height = Math.Abs(endPoint.Y - startPoint.Y);

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

    private void updateNodeGroupGraphItem(NodeGroupGraphItem item)
    {
        var groupIndex = GraphItems.IndexOf(item);
        GraphItems.Move(groupIndex, GraphItems.Count - 1);

        var nodeGraphItems = GraphItems.OfType<NodeGraphItem>().Where(nodeGraphItem => item.Group.Nodes.Contains(nodeGraphItem.Node.Id)).ToList();

        foreach (var nodeGraphItem in nodeGraphItems)
        {
            var index = GraphItems.IndexOf(nodeGraphItem);
            GraphItems.Move(index, GraphItems.Count - 1);
        }

        var topLeft = new Point(GraphContainer.Width, GraphContainer.Height);
        var bottomRight = new Point(0, 0);

        foreach (var nodeGraphItem in nodeGraphItems)
        {
            topLeft.X = Math.Min(topLeft.X, nodeGraphItem.PosX - GroupPadding.Left);
            topLeft.Y = Math.Min(topLeft.Y, nodeGraphItem.PosY - GroupPadding.Top);
            bottomRight.X = Math.Max(bottomRight.X, nodeGraphItem.PosX + nodeGraphItem.Element.ActualWidth + GroupPadding.Right);
            bottomRight.Y = Math.Max(bottomRight.Y, nodeGraphItem.PosY + nodeGraphItem.Element.ActualHeight + GroupPadding.Bottom);
        }

        var width = bottomRight.X - topLeft.X;
        var height = bottomRight.Y - topLeft.Y;

        width = Math.Max(width, 0);
        height = Math.Max(height, 0);

        item.UpdatePos(topLeft);
        item.Width = width;
        item.Height = height;
    }

    private void refreshGraphItemPosition(GraphItem item)
    {
        updateGraphItemPosition(item, new Point(item.PosX, item.PosY));
    }

    private bool updateGraphItemPosition(GraphItem item, Point position)
    {
        position.X = Math.Clamp(position.X, 0, GraphContainer.Width - item.Element.ActualWidth);
        position.Y = Math.Clamp(position.Y, 0, GraphContainer.Height - item.Element.ActualHeight);

        position.X += item.SnapOffset.X;
        position.Y += item.SnapOffset.Y;

        snapPointToGrid(ref position);

        position.X -= item.SnapOffset.X;
        position.Y -= item.SnapOffset.Y;

        var positionChanged = Math.Abs(position.X - item.PosX) > double.Epsilon || Math.Abs(position.Y - item.PosY) > double.Epsilon;
        if (!positionChanged) return false;

        item.UpdatePos(position);
        return true;
    }

    private (TranslateTransform Translation, ScaleTransform Scale) getGraphTransform()
    {
        return ((TranslateTransform)InnerContainer.RenderTransform, (ScaleTransform)OuterContainer.RenderTransform);
    }

    private void resetCanvasPosition()
    {
        graphDrag = null;

        var graphTranslation = getGraphTransform().Translation;
        if (graphTranslation.X == 0 && graphTranslation.Y == 0) return;

        var pf = new PathFigure
        {
            StartPoint = new Point(graphTranslation.X, graphTranslation.Y),
            Segments = new PathSegmentCollection
            {
                new LineSegment(new Point(0, 0), isStroked: true)
            }
        };

        var pg = new PathGeometry(new[] { pf });

        var animX = new DoubleAnimationUsingPath
        {
            PathGeometry = pg,
            Duration = TimeSpan.FromSeconds(0.5f),
            Source = PathAnimationSource.X,
            FillBehavior = FillBehavior.Stop,
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5
        };

        var animY = new DoubleAnimationUsingPath
        {
            PathGeometry = pg,
            Duration = TimeSpan.FromSeconds(0.5f),
            Source = PathAnimationSource.Y,
            FillBehavior = FillBehavior.Stop,
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5
        };

        animX.Completed += (_, _) => graphTranslation.X = 0;
        animY.Completed += (_, _) => graphTranslation.Y = 0;

        Storyboard.SetTarget(animX, InnerContainer);
        Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

        Storyboard.SetTarget(animY, InnerContainer);
        Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

        var sb = new Storyboard();
        sb.Children.Add(animX);
        sb.Children.Add(animY);
        sb.Begin();
    }

    #endregion

    #region OuterContainer

    private void OuterContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        if (e.ChangedButton == GRAPH_DRAG_BUTTON)
        {
            Debug.Assert(graphDrag is null);

            e.Handled = true;

            var mousePos = Mouse.GetPosition(OuterContainer);
            var graphTransform = getGraphTransform();
            var offset = mousePos - new Point(graphTransform.Translation.X, graphTransform.Translation.Y);

            graphDrag = new GraphDrag(offset);
            OuterContainer.CaptureMouse();
        }

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON)
        {
            Debug.Assert(selectionCreate is null);

            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            selectionCreate = new SelectionCreate(mousePos);
            SelectionVisual.Visibility = Visibility.Visible;
            OuterContainer.CaptureMouse();
        }
    }

    private void OuterContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == GRAPH_DRAG_BUTTON && graphDrag is not null)
        {
            e.Handled = true;
            graphDrag = null;
            OuterContainer.ReleaseMouseCapture();
        }

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON && graphItemDrag is not null)
        {
            checkForGroupAdditions();
            e.Handled = true;
            graphItemDrag = null;
            OuterContainer.ReleaseMouseCapture();
        }

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON && nodeGroupGraphItemGrab is not null)
        {
            e.Handled = true;
            nodeGroupGraphItemGrab = null;
            OuterContainer.ReleaseMouseCapture();
        }

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON && connectionDrag is not null)
        {
            e.Handled = true;
            connectionDrag = null;
            OuterContainer.ReleaseMouseCapture();
            stopConnectionDrag();
        }

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON && selectionCreate is not null)
        {
            e.Handled = true;
            selectionCreate = null;
            OuterContainer.ReleaseMouseCapture();
            shrinkWrapSelection();
        }

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON && selectionDrag is not null)
        {
            e.Handled = true;
            selectionDrag = null;
            OuterContainer.ReleaseMouseCapture();
        }

        Graph.Serialise();
    }

    private void checkForGroupAdditions()
    {
        Debug.Assert(graphItemDrag is not null);
        if (graphItemDrag.Item is not NodeGraphItem nodeGraphItem) return;

        if (Graph.Groups.Values.Any(nodeGroup => nodeGroup.Nodes.Contains(nodeGraphItem.Node.Id))) return;

        NodeGroupGraphItem? groupToUpdate = null;

        foreach (var nodeGroupGraphItem in GraphItems.OfType<NodeGroupGraphItem>().Reverse())
        {
            var groupContainer = nodeGroupGraphItem.Element;

            var mousePosRelativeToGroupContainer = Mouse.GetPosition(groupContainer);
            var bounds = new Rect(0, 0, groupContainer.ActualWidth, groupContainer.ActualHeight);
            if (!bounds.Contains(mousePosRelativeToGroupContainer)) continue;

            groupToUpdate = nodeGroupGraphItem;
        }

        if (groupToUpdate is not null)
        {
            groupToUpdate.Group.Nodes.Add(nodeGraphItem.Node.Id);
            updateNodeGroupGraphItem(groupToUpdate);
        }
    }

    private void OuterContainer_OnMouseMove(object sender, MouseEventArgs e)
    {
        updateGraphDrag();
        updateGraphItemDrag();
        updateNodeGroupGraphItemDrag();
        updateConnectionDrag();
        updateSelectionCreate();
        updateSelectionDrag();
    }

    private void OuterContainer_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var direction = e.Delta > 0;

        const double zoom_factor = 1.1;
        const double min_scale = 0.05;
        const double max_scale = 3.0;

        var graphTransform = getGraphTransform();

        if (direction)
        {
            graphTransform.Scale.ScaleX = Math.Min(graphTransform.Scale.ScaleX * zoom_factor, max_scale);
            graphTransform.Scale.ScaleY = Math.Min(graphTransform.Scale.ScaleY * zoom_factor, max_scale);
        }
        else
        {
            graphTransform.Scale.ScaleX = Math.Max(graphTransform.Scale.ScaleX / zoom_factor, min_scale);
            graphTransform.Scale.ScaleY = Math.Max(graphTransform.Scale.ScaleY / zoom_factor, min_scale);
        }

        ContentVisible = graphTransform.Scale.ScaleX >= 0.4d;

        updateGraphDrag();
        updateGraphItemDrag();
        updateNodeGroupGraphItemDrag();
        updateConnectionDrag();
        updateSelectionCreate();
        updateSelectionDrag();
    }

    private void updateGraphDrag()
    {
        if (graphDrag is null) return;

        var mousePos = Mouse.GetPosition(OuterContainer);
        var newPos = mousePos - graphDrag.Offset;
        var graphBounds = new Point(GraphContainer.Width - GraphContainer.Width / 2d, GraphContainer.Height - GraphContainer.Height / 2d);

        newPos.X = Math.Clamp(newPos.X, -graphBounds.X, graphBounds.X);
        newPos.Y = Math.Clamp(newPos.Y, -graphBounds.Y, graphBounds.Y);

        var canvasPosition = getGraphTransform();
        canvasPosition.Translation.X = newPos.X;
        canvasPosition.Translation.Y = newPos.Y;
    }

    private void updateGraphItemDrag()
    {
        if (graphItemDrag is null) return;

        var mousePos = Mouse.GetPosition(GraphContainer);
        var newPos = new Point(mousePos.X - graphItemDrag.Offset.X, mousePos.Y - graphItemDrag.Offset.Y);

        var hasUpdated = updateGraphItemPosition(graphItemDrag.Item, newPos);
        if (!hasUpdated) return;

        if (graphItemDrag.Item is NodeGraphItem nodeGraphItem)
        {
            drawNodeConnections(nodeGraphItem.Node);

            var group = Graph.Groups.Values.SingleOrDefault(group => group.Nodes.Contains(nodeGraphItem.Node.Id));

            if (group is not null)
            {
                updateNodeGroupGraphItem(GraphItems.OfType<NodeGroupGraphItem>().Single(nodeGroupItem => nodeGroupItem.Group.Id == group.Id));
            }
        }

        if (graphItemDrag.Item is NodeGroupGraphItem nodeGroupGraphItem)
        {
            updateNodeGroupGraphItem(nodeGroupGraphItem);
        }
    }

    private void updateNodeGroupGraphItemDrag()
    {
        if (nodeGroupGraphItemGrab is null) return;

        var mousePos = Mouse.GetPosition(GraphContainer);
        var currPos = new Point(nodeGroupGraphItemGrab.Item.PosX, nodeGroupGraphItemGrab.Item.PosY) - nodeGroupGraphItemGrab.OffsetFromGrid;
        var newPos = mousePos - nodeGroupGraphItemGrab.Offset - nodeGroupGraphItemGrab.OffsetFromGrid;
        var groupGraphItem = nodeGroupGraphItemGrab.Item;

        newPos.X = Math.Clamp(newPos.X, 0, GraphContainer.Width - groupGraphItem.Element.ActualWidth);
        newPos.Y = Math.Clamp(newPos.Y, 0, GraphContainer.Height - groupGraphItem.Element.ActualHeight);

        snapPointToGrid(ref newPos);

        var delta = new Vector(newPos.X - currPos.X, newPos.Y - currPos.Y);

        var positionChanged = Math.Abs(delta.X) >= SNAP_DISTANCE || Math.Abs(delta.Y) >= SNAP_DISTANCE;
        if (!positionChanged) return;

        foreach (var graphItem in nodeGroupGraphItemGrab.Items)
        {
            graphItem.UpdatePos(new Point(graphItem.PosX + delta.X, graphItem.PosY + delta.Y));
        }

        drawNodeConnections(nodeGroupGraphItemGrab.Connections);
        groupGraphItem.UpdatePos(newPos + nodeGroupGraphItemGrab.OffsetFromGrid);
    }

    private void updateConnectionDrag()
    {
        if (connectionDrag is null) return;

        var element = connectionDrag.OriginElement;
        var offset = new Vector(element.Width / 2d, element.Height / 2d);

        var isReversed = connectionDrag.Origin is ConnectionDragOrigin.FlowInput or ConnectionDragOrigin.ValueInput;

        var startPoint = element.TranslatePoint(new Point(0, 0), GraphContainer) + offset;
        var endPoint = Mouse.GetPosition(GraphContainer);
        var minDelta = Math.Min(Math.Abs(endPoint.Y - startPoint.Y) / 2d, 50d);
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.5d, minDelta);
        var controlPoint1 = Point.Add(startPoint, new Vector(isReversed ? -delta : delta, 0));
        var controlPoint2 = Point.Add(endPoint, new Vector(isReversed ? delta : -delta, 0));

        var pathGeometry = (PathGeometry)ConnectionDragPath.Data;
        var pathFigure = pathGeometry.Figures[0];
        var curve = (BezierSegment)pathFigure.Segments[0];

        pathFigure.StartPoint = startPoint;
        curve.Point1 = controlPoint1;
        curve.Point2 = controlPoint2;
        curve.Point3 = endPoint;
    }

    private void updateSelectionCreate()
    {
        if (selectionCreate is null) return;

        var mousePos = Mouse.GetPosition(GraphContainer);

        var posX = Math.Min(mousePos.X, selectionCreate.Position.X);
        var posY = Math.Min(mousePos.Y, selectionCreate.Position.Y);

        var width = Math.Abs(mousePos.X - selectionCreate.Position.X);
        var height = Math.Abs(mousePos.Y - selectionCreate.Position.Y);

        SelectionVisual.Width = width;
        SelectionVisual.Height = height;

        var selectionTransform = (TranslateTransform)SelectionVisual.RenderTransform;
        selectionTransform.X = posX;
        selectionTransform.Y = posY;
    }

    private void updateSelectionDrag()
    {
        if (selectionDrag is null) return;

        Debug.Assert(selection is not null);

        var mousePos = Mouse.GetPosition(GraphContainer);
        var transform = (TranslateTransform)SelectionVisual.RenderTransform;
        var currPos = new Point(transform.X, transform.Y) - selection.OffsetFromGrid;
        var newPos = mousePos - selectionDrag.Offset - selection.OffsetFromGrid;

        newPos.X = Math.Clamp(newPos.X, 0, GraphContainer.Width - SelectionVisual.ActualWidth);
        newPos.Y = Math.Clamp(newPos.Y, 0, GraphContainer.Height - SelectionVisual.ActualHeight);

        snapPointToGrid(ref newPos);

        var delta = new Point(newPos.X - currPos.X, newPos.Y - currPos.Y);

        var positionChanged = Math.Abs(delta.X) >= SNAP_DISTANCE || Math.Abs(delta.Y) >= SNAP_DISTANCE;
        if (!positionChanged) return;

        transform.X = newPos.X + selection.OffsetFromGrid.X;
        transform.Y = newPos.Y + selection.OffsetFromGrid.Y;

        foreach (var item in selection.Items)
        {
            updateGraphItemPosition(item, new Point(item.PosX + delta.X, item.PosY + delta.Y));
        }

        drawNodeConnections(selection.Connections);

        foreach (var nodeGroupGraphItem in GraphItems.OfType<NodeGroupGraphItem>().ToList())
        {
            updateNodeGroupGraphItem(nodeGroupGraphItem);
        }
    }

    private void shrinkWrapSelection()
    {
        var bounds = new Rect(0, 0, SelectionVisual.ActualWidth, SelectionVisual.ActualHeight);

        var topLeft = new Point(GraphContainer.ActualWidth, GraphContainer.ActualHeight);
        var bottomRight = new Point(0, 0);

        var items = new List<GraphItem>();

        foreach (var graphItem in GraphItems.OfType<NodeGraphItem>())
        {
            var element = graphItem.Element;
            var startPoint = element.TranslatePoint(new Point(0, 0), SelectionVisual);
            var endPoint = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), SelectionVisual);

            var nodeContainerPosition = (TranslateTransform)element.RenderTransform;

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
            {
                topLeft.X = Math.Min(topLeft.X, nodeContainerPosition.X);
                topLeft.Y = Math.Min(topLeft.Y, nodeContainerPosition.Y);
                bottomRight.X = Math.Max(bottomRight.X, nodeContainerPosition.X + element.ActualWidth);
                bottomRight.Y = Math.Max(bottomRight.Y, nodeContainerPosition.Y + element.ActualHeight);
                items.Add(graphItem);
            }
        }

        if (items.Count == 0)
        {
            deselectGraphItems();
            return;
        }

        topLeft.X -= SelectionPadding.Left;
        topLeft.Y -= SelectionPadding.Top;
        bottomRight.X += SelectionPadding.Right;
        bottomRight.Y += SelectionPadding.Bottom;

        SelectionVisual.Width = bottomRight.X - topLeft.X;
        SelectionVisual.Height = bottomRight.Y - topLeft.Y;

        var selectionTransform = (TranslateTransform)SelectionVisual.RenderTransform;
        selectionTransform.X = topLeft.X;
        selectionTransform.Y = topLeft.Y;

        var offsetFromGrid = new Vector(topLeft.X % SNAP_DISTANCE, topLeft.Y % SNAP_DISTANCE);

        var connections = items.OfType<NodeGraphItem>()
                               .SelectMany(nodeGraphItem => Graph.Connections.Values.Where(c => c.InputNodeId == nodeGraphItem.Node.Id || c.OutputNodeId == nodeGraphItem.Node.Id))
                               .Distinct();

        selection = new GraphItemSelection(offsetFromGrid, items.ToArray(), connections.ToArray());
    }

    #endregion

    #region GraphContainer

    private void GraphContainer_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        graphContextMenuPosition = Mouse.GetPosition(GraphContainer);

        if (connectionDrag is not null)
        {
            var node = connectionDrag.NodeGraphItem.Node;
            var slot = connectionDrag.Slot;
            var mousePos = Mouse.GetPosition(GraphContainer);

            if (connectionDrag.Origin == ConnectionDragOrigin.ValueInput)
            {
                var slotInputType = node.GetTypeOfInputSlot(slot);
                if (slotInputType.IsGenericType && slotInputType.GetGenericTypeDefinition() == typeof(Nullable<>)) slotInputType = slotInputType.GenericTypeArguments[0];

                if (NodeConstants.INPUT_TYPES.Contains(slotInputType) || slotInputType.IsAssignableTo(typeof(Enum)) || slotInputType == typeof(Keybind))
                {
                    e.Handled = true;

                    var type = typeof(ValueNode<>).MakeGenericType(slotInputType);
                    var outputNode = Graph.AddNode(type, mousePos);
                    Graph.CreateValueConnection(outputNode.Id, 0, node.Id, slot);
                    Graph.MarkDirty();
                    stopConnectionDrag();
                    return;
                }
            }

            if (connectionDrag.Origin == ConnectionDragOrigin.ValueOutput)
            {
                var slotOutputType = node.GetTypeOfOutputSlot(slot);

                e.Handled = true;

                var type = typeof(DisplayNode<>).MakeGenericType(slotOutputType);
                var inputNode = Graph.AddNode(type, mousePos);
                Graph.CreateValueConnection(node.Id, slot, inputNode.Id, 0);
                Graph.MarkDirty();
                stopConnectionDrag();
                return;
            }

            if (connectionDrag.Origin == ConnectionDragOrigin.FlowInput)
            {
                e.Handled = true;

                var outputNode = Graph.AddNode(typeof(ButtonNode), mousePos);
                Graph.CreateFlowConnection(outputNode.Id, 0, node.Id);
                Graph.MarkDirty();
                stopConnectionDrag();
                return;
            }
        }
    }

    #endregion

    #region NodeContainer

    private void NodeContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        var element = (FrameworkElement)sender;
        var graphItem = (NodeGraphItem)element.Tag;

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON)
        {
            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            var nodePos = new Point(graphItem.PosX, graphItem.PosY);
            var offset = mousePos - nodePos;

            var groupItem = GraphItems.OfType<NodeGroupGraphItem>().SingleOrDefault(groupItem => groupItem.Group.Nodes.Contains(graphItem.Node.Id));

            if (groupItem is not null)
                updateNodeGroupGraphItem(groupItem);

            var index = GraphItems.IndexOf(graphItem);
            GraphItems.Move(index, GraphItems.Count - 1);

            graphItemDrag = new GraphItemDrag(offset, graphItem);
            OuterContainer.CaptureMouse();
        }
    }

    #endregion

    #region GroupContainer

    private void GroupContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        var element = (FrameworkElement)sender;
        var nodeGroupGraphItem = (NodeGroupGraphItem)element.Tag;

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON)
        {
            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            var groupPos = new Point(nodeGroupGraphItem.PosX, nodeGroupGraphItem.PosY);
            var offset = mousePos - groupPos;

            var nodeGraphItems = GraphItems.OfType<NodeGraphItem>()
                                           .Where(nodeGraphItem => nodeGroupGraphItem.Group.Nodes.Contains(nodeGraphItem.Node.Id))
                                           .ToList();

            var connections = nodeGraphItems.SelectMany(nodeGraphItem => Graph.Connections.Values.Where(c => c.InputNodeId == nodeGraphItem.Node.Id || c.OutputNodeId == nodeGraphItem.Node.Id))
                                            .Distinct();

            var offsetFromGrid = new Vector(groupPos.X % SNAP_DISTANCE, groupPos.Y % SNAP_DISTANCE);

            var groupGraphItemIndex = GraphItems.IndexOf(nodeGroupGraphItem);
            GraphItems.Move(groupGraphItemIndex, GraphItems.Count - 1);

            foreach (var nodeGraphItem in nodeGraphItems)
            {
                var index = GraphItems.IndexOf(nodeGraphItem);
                GraphItems.Move(index, GraphItems.Count - 1);
            }

            nodeGroupGraphItemGrab = new NodeGroupGraphItemDrag(offset, offsetFromGrid, nodeGroupGraphItem, nodeGraphItems, connections);
            OuterContainer.CaptureMouse();
        }
    }

    #endregion

    #region Selection Visual

    private void SelectionVisual_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        var element = (FrameworkElement)sender;
        var position = (TranslateTransform)element.RenderTransform;

        if (e.ChangedButton == GRAPH_ITEM_DRAG_BUTTON)
        {
            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            var selectionPos = new Point(position.X, position.Y);
            var offset = mousePos - selectionPos;

            selectionDrag = new SelectionDrag(offset);
            OuterContainer.CaptureMouse();
        }
    }

    #endregion

    #region Context Menus

    private void GraphContextMenu_NodeTypeItemClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeType = (Type)element.Tag;

        if (nodeType.IsGenericTypeDefinition)
        {
            var nodeCreatorWindow = new NodeCreatorWindow(Graph, nodeType);

            nodeCreatorWindow.Closed += (_, _) =>
            {
                if (nodeCreatorWindow.ConstructedType is null) return;

                Graph.AddNode(nodeCreatorWindow.ConstructedType, graphContextMenuPosition);
                Graph.MarkDirty();
            };

            nodeCreatorWindowManager.TrySpawnChild(nodeCreatorWindow);
        }
        else
        {
            Graph.AddNode(nodeType, graphContextMenuPosition);
            Graph.MarkDirty();
        }
    }

    private void GraphContextMenu_PresetItemClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var preset = (NodePreset)element.Tag;

        preset.SpawnTo(Graph, graphContextMenuPosition);
        Graph.MarkDirty();
    }

    private void NodeContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        var groupToUpdate = GraphItems.OfType<NodeGroupGraphItem>().SingleOrDefault(nodeGroupGraphItem => nodeGroupGraphItem.Group.Nodes.Contains(nodeGraphItem.Node.Id));

        if (groupToUpdate is not null)
        {
            groupToUpdate.Group.Nodes.Remove(nodeGraphItem.Node.Id);
            updateNodeGroupGraphItem(groupToUpdate);
        }

        Graph.DeleteNode(nodeGraphItem.Node.Id);
        Graph.MarkDirty();
    }

    private void GroupContextMenu_DissolveClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGroupGraphItem = (NodeGroupGraphItem)element.Tag;

        Graph.DeleteGroup(nodeGroupGraphItem.Group.Id);
        Graph.MarkDirty();
    }

    private void GroupContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGroupGraphItem = (NodeGroupGraphItem)element.Tag;

        foreach (var node in nodeGroupGraphItem.Group.Nodes.ToList())
        {
            Graph.DeleteNode(node);
        }

        Graph.DeleteGroup(nodeGroupGraphItem.Group.Id);
        Graph.MarkDirty();
    }

    private void SelectionContextMenu_CreateGroupClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(selection is not null);

        Graph.AddGroup(selection.Items.OfType<NodeGraphItem>().Select(item => item.Node.Id));
        Graph.MarkDirty();
        deselectGraphItems();
    }

    private void SelectionContextMenu_SaveAsPresetClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(selection is not null);

        var selectedNodes = selection.Items.OfType<NodeGraphItem>().Select(item => item.Node.Id).ToList();
        var position = (TranslateTransform)SelectionVisual.RenderTransform;

        var presetCreatorWindow = new PresetCreatorWindow();

        presetCreatorWindow.Closed += (_, _) =>
        {
            if (string.IsNullOrEmpty(presetCreatorWindow.PresetName)) return;

            Graph.CreatePreset(presetCreatorWindow.PresetName, selectedNodes, (float)position.X, (float)position.Y);
            RefreshContextMenu();
        };

        presetCreatorWindowManager.TrySpawnChild(presetCreatorWindow);
        deselectGraphItems();
    }

    private void SelectionContextMenu_DeleteAllClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(selection is not null);

        foreach (var item in selection.Items)
        {
            if (item is NodeGraphItem nodeGraphItem)
            {
                Graph.DeleteNode(nodeGraphItem.Node.Id);
            }
        }

        Graph.MarkDirty();
        deselectGraphItems();
    }

    #endregion

    #region Connection Points

    private (ConnectionType Type, int Slot) connectionDataFromElement(FrameworkElement element)
    {
        var split = ((string)element.Tag).Split('_');
        return (split[0] == "flow" ? ConnectionType.Flow : ConnectionType.Value, int.Parse(split[2]));
    }

    private void startConnectionDrag(ConnectionDragOrigin source, FrameworkElement sourceElement)
    {
        Debug.Assert(connectionDrag is null);

        var nodeGraphItem = ((ConnectionViewModel)sourceElement.DataContext).NodeGraphItem;
        var (_, slot) = connectionDataFromElement(sourceElement);

        if (source == ConnectionDragOrigin.ValueInput)
        {
            var existingConnection = Graph.Connections.Values.FirstOrDefault(c => c.ConnectionType == ConnectionType.Value && c.InputNodeId == nodeGraphItem.Node.Id && c.InputSlot == slot);

            if (existingConnection is not null)
            {
                var outputNodeGraphItem = GraphItems.OfType<NodeGraphItem>().Single(outputNodeGraphItem => outputNodeGraphItem.Node.Id == existingConnection.OutputNodeId);

                Graph.RemoveConnection(existingConnection);
                Graph.MarkDirty();

                connectionDrag = new ConnectionDrag(ConnectionDragOrigin.ValueOutput, outputNodeGraphItem, existingConnection.OutputSlot,
                    outputNodeGraphItem.ValueOutputs[existingConnection.OutputSlot]);
            }
        }

        if (source == ConnectionDragOrigin.FlowInput)
        {
            var existingConnection = Graph.Connections.Values.FirstOrDefault(c => c.ConnectionType == ConnectionType.Flow && c.InputNodeId == nodeGraphItem.Node.Id && c.InputSlot == slot);

            if (existingConnection is not null)
            {
                var outputNodeGraphItem = GraphItems.OfType<NodeGraphItem>().Single(outputNodeGraphItem => outputNodeGraphItem.Node.Id == existingConnection.OutputNodeId);

                Graph.RemoveConnection(existingConnection);
                Graph.MarkDirty();

                connectionDrag = new ConnectionDrag(ConnectionDragOrigin.FlowOutput, outputNodeGraphItem, existingConnection.OutputSlot,
                    outputNodeGraphItem.FlowOutputs[existingConnection.OutputSlot]);
            }
        }

        connectionDrag ??= new ConnectionDrag(source, nodeGraphItem, slot, sourceElement);
        ConnectionDragPath.Visibility = Visibility.Visible;
        updateConnectionDrag();
    }

    private void stopConnectionDrag()
    {
        connectionDrag = null;
        ConnectionDragPath.Visibility = Visibility.Collapsed;
    }

    private void stopConnectionDragAndCreate(ConnectionDragOrigin destination, FrameworkElement destinationElement)
    {
        Debug.Assert(connectionDrag is not null);

        var nodeGraphItem = ((ConnectionViewModel)destinationElement.DataContext).NodeGraphItem;
        var (_, slot) = connectionDataFromElement(destinationElement);

        if (connectionDrag.Origin == ConnectionDragOrigin.FlowInput && destination == ConnectionDragOrigin.FlowOutput)
        {
            Graph.CreateFlowConnection(nodeGraphItem.Node.Id, slot, connectionDrag.NodeGraphItem.Node.Id);
            Graph.MarkDirty();
        }

        if (connectionDrag.Origin == ConnectionDragOrigin.FlowOutput && destination == ConnectionDragOrigin.FlowInput)
        {
            Graph.CreateFlowConnection(connectionDrag.NodeGraphItem.Node.Id, connectionDrag.Slot, nodeGraphItem.Node.Id);
            Graph.MarkDirty();
        }

        if (connectionDrag.Origin == ConnectionDragOrigin.ValueInput && destination == ConnectionDragOrigin.ValueOutput)
        {
            Graph.CreateValueConnection(nodeGraphItem.Node.Id, slot, connectionDrag.NodeGraphItem.Node.Id, connectionDrag.Slot);
            Graph.MarkDirty();
        }

        if (connectionDrag.Origin == ConnectionDragOrigin.ValueOutput && destination == ConnectionDragOrigin.ValueInput)
        {
            Graph.CreateValueConnection(connectionDrag.NodeGraphItem.Node.Id, connectionDrag.Slot, nodeGraphItem.Node.Id, slot);
            Graph.MarkDirty();
        }

        stopConnectionDrag();
    }

    private void ConnectionInput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != GRAPH_ITEM_DRAG_BUTTON) return;

        Debug.Assert(connectionDrag is null);

        e.Handled = true;
        var element = (FrameworkElement)sender;
        var (type, _) = connectionDataFromElement(element);
        startConnectionDrag(type == ConnectionType.Flow ? ConnectionDragOrigin.FlowInput : ConnectionDragOrigin.ValueInput, element);
    }

    private void ConnectionInput_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != GRAPH_ITEM_DRAG_BUTTON) return;
        if (connectionDrag is null) return;

        e.Handled = true;
        var element = (FrameworkElement)sender;
        var (type, _) = connectionDataFromElement(element);
        stopConnectionDragAndCreate(type == ConnectionType.Flow ? ConnectionDragOrigin.FlowInput : ConnectionDragOrigin.ValueInput, element);
    }

    private void ConnectionOutput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != GRAPH_ITEM_DRAG_BUTTON) return;

        Debug.Assert(connectionDrag is null);

        e.Handled = true;
        var element = (FrameworkElement)sender;
        var (type, _) = connectionDataFromElement(element);

        startConnectionDrag(type == ConnectionType.Flow ? ConnectionDragOrigin.FlowOutput : ConnectionDragOrigin.ValueOutput, element);
    }

    private void ConnectionOutput_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != GRAPH_ITEM_DRAG_BUTTON) return;
        if (connectionDrag is null) return;

        e.Handled = true;
        var element = (FrameworkElement)sender;
        var (type, _) = connectionDataFromElement(element);
        stopConnectionDragAndCreate(type == ConnectionType.Flow ? ConnectionDragOrigin.FlowOutput : ConnectionDragOrigin.ValueOutput, element);
    }

    #endregion

    #region NodeGraph Title

    private bool graphTitleEditing;

    public bool GraphTitleEditing
    {
        get => graphTitleEditing;
        set
        {
            if (value == graphTitleEditing) return;

            graphTitleEditing = value;
            OnPropertyChanged();
        }
    }

    private void NodeGraphTitle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;

        e.Handled = true;
        GraphTitleEditing = true;
        NodeGraphTitleTextBox.Focus();
    }

    private void NodeGraphTitleTextBoxContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void NodeGraphTitleTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        GraphTitleEditing = false;

        if (string.IsNullOrEmpty(NodeGraphTitleTextBox.Text))
        {
            Graph.Name.Value = "New Graph";
            return;
        }

        Graph.Name.Value = NodeGraphTitleTextBox.Text;
        Graph.Serialise();
    }

    private void NodeGraphTitleTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Focus();
        }
    }

    #endregion

    private void GroupTitle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var nodeGroupGraphItem = (NodeGroupGraphItem)element.Tag;

        nodeGroupGraphItem.Editing = true;

        element.Parent.FindVisualChild<TextBox>("GroupTitleTextBox")!.Focus();
    }

    private void GroupTitleTextBoxContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void GroupTitleTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var nodeGroupGraphItem = (NodeGroupGraphItem)element.Tag;

        nodeGroupGraphItem.Editing = false;
        Graph.Serialise();
    }

    private void GroupTitleTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Focus();
        }
    }

    private void ButtonNode_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        Graph.StartFlow(nodeGraphItem.Node);
    }

    private async void EnumValueNode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        if (!hasLoaded) return;

        // force off the UI thread to wait for a new render
        await Task.Delay(1);

        Dispatcher.Invoke(() =>
        {
            populateNodeGraphItemSnapOffset(nodeGraphItem);
            refreshGraphItemPosition(nodeGraphItem);
            drawNodeConnections(nodeGraphItem.Node);
        }, DispatcherPriority.Render);
    }

    private void InputVariableSize_IncreaseOnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        nodeGraphItem.Node.VariableSize.ValueInputSize++;

        Graph.Serialise();

        var valueInputItemsControl = nodeGraphItem.ValueInputs.Last().FindVisualParent<ItemsControl>("ValueInputItemsControl")!;
        valueInputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();

        Dispatcher.Invoke(() => populateNodeGraphItem(nodeGraphItem), DispatcherPriority.Render);
    }

    private void InputVariableSize_DecreaseOnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        if (nodeGraphItem.Node.VariableSize.ValueInputSize == 1) return;

        var inputSlot = nodeGraphItem.Node.Metadata.InputsCount - 1 + (nodeGraphItem.Node.VariableSize.ValueInputSize - 1);
        var connectionToRemove = Graph.Connections.Values.SingleOrDefault(c => c.ConnectionType == ConnectionType.Value && c.InputNodeId == nodeGraphItem.Node.Id && c.InputSlot == inputSlot);

        if (connectionToRemove is not null)
        {
            Graph.RemoveConnection(connectionToRemove);
            Graph.MarkDirty();
        }

        nodeGraphItem.Node.VariableSize.ValueInputSize--;

        Graph.Serialise();

        var valueInputItemsControl = nodeGraphItem.ValueInputs.Last().FindVisualParent<ItemsControl>("ValueInputItemsControl")!;
        valueInputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();

        Dispatcher.Invoke(() => populateNodeGraphItem(nodeGraphItem), DispatcherPriority.Render);
    }

    private void OutputVariableSize_IncreaseOnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        nodeGraphItem.Node.VariableSize.ValueOutputSize++;

        Graph.Serialise();

        var valueOutputItemsControl = nodeGraphItem.ValueOutputs.Last().FindVisualParent<ItemsControl>("ValueOutputItemsControl")!;
        valueOutputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();

        Dispatcher.Invoke(() => populateNodeGraphItem(nodeGraphItem), DispatcherPriority.Render);
    }

    private void OutputVariableSize_DecreaseOnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGraphItem = (NodeGraphItem)element.Tag;

        if (nodeGraphItem.Node.VariableSize.ValueOutputSize == 1) return;

        var outputSLot = nodeGraphItem.Node.Metadata.OutputsCount - 1 + (nodeGraphItem.Node.VariableSize.ValueOutputSize - 1);
        var connectionToRemove = Graph.Connections.Values.SingleOrDefault(c => c.ConnectionType == ConnectionType.Value && c.OutputNodeId == nodeGraphItem.Node.Id && c.OutputSlot == outputSLot);

        if (connectionToRemove is not null)
        {
            Graph.RemoveConnection(connectionToRemove);
            Graph.MarkDirty();
        }

        nodeGraphItem.Node.VariableSize.ValueOutputSize--;

        Graph.Serialise();

        var valueOutputItemsControl = nodeGraphItem.ValueOutputs.Last().FindVisualParent<ItemsControl>("ValueOutputItemsControl")!;
        valueOutputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();

        Dispatcher.Invoke(() => populateNodeGraphItem(nodeGraphItem), DispatcherPriority.Render);
    }

    private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var richTextBox = (RichTextBox)sender;

        richTextBox.Document.LineStackingStrategy = LineStackingStrategy.MaxHeight;

        if (e.Key == Key.Tab)
        {
            e.Handled = true;
            richTextBox.CaretPosition.InsertTextInRun("\t");
        }
    }

    private void TextBoxValueOutputOnlyNodeTemplate_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var nodeGraphItem = (NodeGraphItem)textBox.Tag;

        var type = nodeGraphItem.Node.Metadata.Outputs[0].Type;

        try
        {
            var iNumberType = typeof(INumber<>).MakeGenericType(type);
            if (!iNumberType.IsAssignableFrom(type)) return;

            var expression = new Expression(textBox.Text);
            expression.disableImpliedMultiplicationMode();
            var result = expression.calculate();

            var convertedResult = Convert.ChangeType(result, type);
            e.Handled = true;
            textBox.Text = convertedResult.ToString()!;
        }
        catch
        {
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum ConnectionDragOrigin
{
    FlowOutput,
    FlowInput,
    ValueOutput,
    ValueInput
}

public record SelectionCreate(Point Position);

public record GraphItemSelection(Vector OffsetFromGrid, GraphItem[] Items, NodeConnection[] Connections);

public record GraphDrag(Vector Offset);

public record GraphItemDrag(Vector Offset, GraphItem Item) : GraphDrag(Offset);

public record NodeGroupGraphItemDrag(Vector Offset, Vector OffsetFromGrid, NodeGroupGraphItem Item, IEnumerable<GraphItem> Items, IEnumerable<NodeConnection> Connections) : GraphDrag(Offset);

public record ConnectionDrag(ConnectionDragOrigin Origin, NodeGraphItem NodeGraphItem, int Slot, FrameworkElement OriginElement);

public record SelectionDrag(Vector Offset) : GraphDrag(Offset);

public record GraphItem : INotifyPropertyChanged
{
    public FrameworkElement Element { get; set; } = null!;
    public Vector SnapOffset { get; set; }

    private double posX;

    public double PosX
    {
        get => posX;
        private set
        {
            if (value.Equals(posX)) return;

            posX = value;
            OnPropertyChanged();
        }
    }

    private double posY;

    public double PosY
    {
        get => posY;
        private set
        {
            if (value.Equals(posY)) return;

            posY = value;
            OnPropertyChanged();
        }
    }

    public virtual void UpdatePos(Point position)
    {
        PosX = position.X;
        PosY = position.Y;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public record NodeGraphItem : GraphItem
{
    public Node Node { get; }
    public FrameworkElement[] FlowInputs { get; set; } = Array.Empty<FrameworkElement>();
    public FrameworkElement[] FlowOutputs { get; set; } = Array.Empty<FrameworkElement>();
    public FrameworkElement[] ValueInputs { get; set; } = Array.Empty<FrameworkElement>();
    public FrameworkElement[] ValueOutputs { get; set; } = Array.Empty<FrameworkElement>();

    public NodeGraphItem(Node node)
    {
        Node = node;
    }

    public override void UpdatePos(Point position)
    {
        base.UpdatePos(position);
        Node.NodePosition = position;
    }

    public virtual bool Equals(NodeGraphItem? other) => Node.Id == other?.Node.Id;

    public override int GetHashCode() => Node.Id.GetHashCode();
}

public record NodeGroupGraphItem : GraphItem
{
    public NodeGroup Group { get; }

    private bool editing;

    public bool Editing
    {
        get => editing;
        set
        {
            if (value == editing) return;

            editing = value;
            OnPropertyChanged();
        }
    }

    private double width;

    public double Width
    {
        get => width;
        set
        {
            if (value.Equals(width)) return;

            width = value;
            OnPropertyChanged();
        }
    }

    private double height;

    public double Height
    {
        get => height;
        set
        {
            if (value.Equals(height)) return;

            height = value;
            OnPropertyChanged();
        }
    }

    public NodeGroupGraphItem(NodeGroup group)
    {
        Group = group;
    }
}

public record ConnectionItem
{
    public NodeConnection Connection { get; }
    public Path Path { get; }

    public ConnectionItem(NodeConnection connection, Path path)
    {
        Connection = connection;
        Path = path;
    }

    public virtual bool Equals(ConnectionItem? other) => Connection == other?.Connection;

    public override int GetHashCode() => Connection.GetHashCode();
}