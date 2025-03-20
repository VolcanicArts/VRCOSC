// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodeCanvas
{
    public const double SNAP_DISTANCE = 20d;
    public const double SIGNIFICANT_SNAP_DISTANCE = SNAP_DISTANCE * 10d;

    public NodeCanvas(NodeField nodeField)
    {
        NodeField = nodeField;
        InitializeComponent();
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
        DataContext = this;
    }

    public NodeField NodeField { get; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();

        drawRootCanvasLines();
        setZIndexesOfNodes();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            canvasDrag = null;

            var canvasPosition = getCanvasPosition();
            canvasPosition.X = 0;
            canvasPosition.Y = 0;
        }
    }

    private void drawRootCanvasLines()
    {
        RootCanvas.Children.Clear();

        for (var i = 0; i <= CanvasContainer.ActualWidth / SNAP_DISTANCE; i++)
        {
            var significant = i % (SIGNIFICANT_SNAP_DISTANCE / SNAP_DISTANCE) == 0;
            var xPos = i * SNAP_DISTANCE;
            var lineHeight = CanvasContainer.ActualHeight;
            Logger.Log($"Drawing root canvas line at {xPos}", LoggingTarget.Information);

            var line = new Line
            {
                X1 = xPos,
                Y1 = 0,
                X2 = xPos,
                Y2 = lineHeight,
                Stroke = significant ? (Brush)FindResource("CForeground2") : (Brush)FindResource("CBackground1"),
                StrokeThickness = 1,
                Opacity = significant ? 0.5f : 1f
            };

            RootCanvas.Children.Add(line);
        }

        for (var i = 0; i <= CanvasContainer.ActualHeight / SNAP_DISTANCE; i++)
        {
            var significant = i % (SIGNIFICANT_SNAP_DISTANCE / SNAP_DISTANCE) == 0;
            var yPos = i * SNAP_DISTANCE;
            var lineWidth = CanvasContainer.ActualWidth;

            var line = new Line
            {
                X1 = 0,
                Y1 = yPos,
                X2 = lineWidth,
                Y2 = yPos,
                Stroke = significant ? (Brush)FindResource("CForeground2") : (Brush)FindResource("CBackground1"),
                StrokeThickness = 1,
                Opacity = significant ? 0.5f : 1f
            };

            RootCanvas.Children.Add(line);
        }
    }

    private void setZIndexesOfNodes()
    {
        var itemsControl = NodesItemsControl;

        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is not FrameworkElement container) continue;

            var pair = (ObservableKeyValuePair<Guid, Node>)container.DataContext;
            Panel.SetZIndex(container, pair.Value.ZIndex);
        }
    }

    private CanvasDragInstance? canvasDrag;
    private NodeDragInstance? nodeDrag;

    private TranslateTransform getCanvasPosition()
    {
        return (TranslateTransform)CanvasContainer.RenderTransform;
    }

    private void RootContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(RootContainer);
        var transform = getCanvasPosition();

        var offsetX = mousePosRelativeToNodesItemsControl.X - transform.X;
        var offsetY = mousePosRelativeToNodesItemsControl.Y - transform.Y;

        Logger.Log($"{nameof(RootContainer_OnMouseDown)}: Starting node drag. Offset X {offsetX}. Offset Y {offsetY}", LoggingTarget.Information);
        canvasDrag = new CanvasDragInstance(offsetX, offsetY);
    }

    private void RootContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (canvasDrag is not null)
        {
            e.Handled = true;
            Logger.Log($"{nameof(RootContainer_OnMouseUp)}: Ending canvas drag", LoggingTarget.Information);
            canvasDrag = null;
        }

        if (nodeDrag is not null)
        {
            e.Handled = true;
            Logger.Log($"{nameof(RootContainer_OnMouseUp)}: Ending node drag", LoggingTarget.Information);
            nodeDrag = null;
        }
    }

    private void RootContainer_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (canvasDrag is null) return;

        e.Handled = true;

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(RootContainer);

        var newCanvasX = (mousePosRelativeToNodesItemsControl.X - canvasDrag.OffsetX);
        var newCanvasY = (mousePosRelativeToNodesItemsControl.Y - canvasDrag.OffsetY);

        var canvasBoundsX = CanvasContainer.Width - CanvasContainer.Width / 2d;
        var canvasBoundsY = CanvasContainer.Height - CanvasContainer.Height / 2d;

        newCanvasX = Math.Clamp(newCanvasX, -canvasBoundsX, canvasBoundsX);
        newCanvasY = Math.Clamp(newCanvasY, -canvasBoundsY, canvasBoundsY);

        //Logger.Log($"{nameof(NodesItemsControl_OnMouseMove)}: Setting canvas drag position to {newCanvasX},{newCanvasY}", LoggingTarget.Information);

        var canvasPosition = getCanvasPosition();
        canvasPosition.X = newCanvasX;
        canvasPosition.Y = newCanvasY;
    }

    private void NodeContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        var element = (Border)sender;
        var node = (Node)element.Tag;

        var dataContext = element.DataContext;

        if (NodesItemsControl.ItemContainerGenerator.ContainerFromItem(dataContext) is FrameworkElement container)
        {
            node.ZIndex = NodeField.ZIndex++;

            Panel.SetZIndex(container, node.ZIndex);
            Logger.Log($"{nameof(NodeContainer_OnMouseDown)}: Set Z index to {node.ZIndex}", LoggingTarget.Information);
        }

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);
        var nodePos = node.Position;

        var offsetX = mousePosRelativeToNodesItemsControl.X - nodePos.X;
        var offsetY = mousePosRelativeToNodesItemsControl.Y - nodePos.Y;

        Logger.Log($"{nameof(NodeContainer_OnMouseDown)}: Starting node drag. Offset X {offsetX}. Offset Y {offsetY}", LoggingTarget.Information);
        nodeDrag = new NodeDragInstance(node, offsetX, offsetY);
    }

    private void NodesItemsControl_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (nodeDrag is null) return;

        e.Handled = true;

        Logger.Log($"{nameof(NodesItemsControl_OnMouseUp)}: Ending node drag", LoggingTarget.Information);
        nodeDrag = null;
    }

    private void NodesItemsControl_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (nodeDrag is null) return;

        e.Handled = true;

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);

        var newNodeX = mousePosRelativeToNodesItemsControl.X - nodeDrag.OffsetX;
        var newNodeY = mousePosRelativeToNodesItemsControl.Y - nodeDrag.OffsetY;

        newNodeX = Math.Round(newNodeX / SNAP_DISTANCE) * SNAP_DISTANCE;
        newNodeY = Math.Round(newNodeY / SNAP_DISTANCE) * SNAP_DISTANCE;

        newNodeX = Math.Clamp(newNodeX, 0, CanvasContainer.Width);
        newNodeY = Math.Clamp(newNodeY, 0, CanvasContainer.Height);

        //Logger.Log($"{nameof(NodesItemsControl_OnMouseMove)}: Setting node drag position to {newNodeX},{newNodeY}", LoggingTarget.Information);

        nodeDrag.Node.Position.X = newNodeX;
        nodeDrag.Node.Position.Y = newNodeY;
    }
}

public record NodeDragInstance(Node Node, double OffsetX, double OffsetY);

public record CanvasDragInstance(double OffsetX, double OffsetY);