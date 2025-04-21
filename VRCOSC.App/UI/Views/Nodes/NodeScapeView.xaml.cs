// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;
using Vector = System.Windows.Vector;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodeScapeView : INotifyPropertyChanged
{
    public const MouseButton CANVAS_DRAG_BUTTON = MouseButton.Middle;
    public const MouseButton ELEMENT_DRAG_BUTTON = MouseButton.Left;
    public const MouseButton CONTEXT_MENU_BUTTON = MouseButton.Right;
    public const double SNAP_DISTANCE = 20d;
    public const double SIGNIFICANT_SNAP_DISTANCE = SNAP_DISTANCE * 20d;
    public const bool SNAP_ENABLED = true;
    public const double GROUP_PADDING = SNAP_DISTANCE * 2d;

    public NodeScapeView(NodeScape nodeScape)
    {
        NodeScape = nodeScape;
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        KeyDown += OnKeyDown;
        DataContext = this;
    }

    public NodeScape NodeScape { get; }
    public ObservableCollection<object> NodesItemsControlItemsSource { get; } = [];

    private IDisposable nodesCollectionBind = null!;
    private IDisposable nodeScapeConnectionsBind = null!;
    private IDisposable nodeGroupsCollectionBind = null!;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ConnectionCanvas.Children.Clear();

        Focus();

        drawRootCanvasLines();
        setZIndexesOfNodes();

        nodesCollectionBind = NodeScape.Nodes.OnCollectionChanged(onNodeCollectionChanged, true);
        nodeScapeConnectionsBind = NodeScape.Connections.OnCollectionChanged(onConnectionsChanged, true);
        nodeGroupsCollectionBind = NodeScape.Groups.OnCollectionChanged(onGroupsChanged, true);
    }

    /***
    private void generateAddMode()
    {
        var nodeAttributes = NodeScape.Metadata.Select(pair => (pair.Key.GetCustomAttribute<NodeAttribute>()!.GroupPath, pair.Value));

        foreach (var (path, metadata) in nodeAttributes)
        {
            var paths = path.Split('/');

            foreach (var pathPart in paths)
            {
                var part = ContextMenu.AddModeItemsSource.FirstOrDefault(item => item.Title == pathPart);
                if (part is not null) continue;

                ContextMenu.AddModeItemsSource.Add(new ContextMenuAddGroup(pathPart, new List<object>()));
            }

            var title = metadata.Title;

            ContextMenuAddGroup? group = ContextMenu.AddModeItemsSource.FirstOrDefault(item => item.Title == paths[0]);

            foreach (var pathPart in paths.Skip(1))
            {
                group = (ContextMenuAddGroup)group.Children.FirstOrDefault(child => child is ContextMenuAddGroup groupInner && groupInner.Title == pathPart);
            }

            group.Children.Add(new ContextMenuAddNode(title, metadata));
        }
    }
    ***/

    private async void onGroupsChanged(IEnumerable<NodeGroup> newGroups, IEnumerable<NodeGroup> oldGroups)
    {
        foreach (var newGroup in newGroups)
        {
            var nodeGroupViewModel = new NodeGroupViewModel
            {
                NodeGroup = newGroup
            };

            newGroup.Nodes.OnCollectionChanged((newNodes, oldNodes) =>
            {
                nodeGroupViewModel.Nodes.AddRange(newNodes.Select(nodeId => NodeScape.Nodes[nodeId]));
                nodeGroupViewModel.Nodes.RemoveIf(node => oldNodes.Contains(node.Id));
                NodesItemsControlItemsSource.RemoveIf(o => o is Node node && newGroup.Nodes.Contains(node.Id));
            }, true);

            NodesItemsControlItemsSource.Add(nodeGroupViewModel);
        }

        await Dispatcher.Yield(DispatcherPriority.Loaded);
        updateGroups(newGroups);
    }

    private void onNodeCollectionChanged(IEnumerable<ObservableKeyValuePair<Guid, Node>> newNodes, IEnumerable<ObservableKeyValuePair<Guid, Node>> oldNodes)
    {
        NodesItemsControlItemsSource.AddRange(newNodes.Select(pair => pair.Value));
        NodesItemsControlItemsSource.RemoveIf(o => o is Node node && oldNodes.Select(pair => pair.Value).Contains(node));

        // TODO: Remove from group as well
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        nodesCollectionBind.Dispose();
        nodeScapeConnectionsBind.Dispose();
        nodeGroupsCollectionBind.Dispose();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            canvasDrag = null;

            var canvasPosition = getRootPosition();
            canvasPosition.X = 0;
            canvasPosition.Y = 0;
        }
    }

    #region Groups

    private FrameworkElement getGroupContainerFromId(Guid groupId)
    {
        var groupContainer = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "GroupContainer", Tag: NodeGroup group } && group.Id == groupId);
        Debug.Assert(groupContainer is not null);
        return groupContainer;
    }

    private void updateGroups(IEnumerable<NodeGroup> groups)
    {
        foreach (var group in groups)
        {
            var topLeft = new Point(CanvasContainer.Width, CanvasContainer.Height);
            var bottomRight = new Point(0, 0);

            foreach (var nodeId in group.Nodes)
            {
                var node = NodeScape.Nodes[nodeId];
                var nodeContainer = getNodeContainerFromId(nodeId);

                topLeft.X = Math.Min(topLeft.X, node.Position.X - GROUP_PADDING);
                topLeft.Y = Math.Min(topLeft.Y, node.Position.Y - GROUP_PADDING);
                bottomRight.X = Math.Max(bottomRight.X, node.Position.X + nodeContainer.ActualWidth + GROUP_PADDING);
                bottomRight.Y = Math.Max(bottomRight.Y, node.Position.Y + nodeContainer.ActualHeight + GROUP_PADDING);
            }

            var groupContainer = getGroupContainerFromId(group.Id);

            var width = bottomRight.X - topLeft.X;
            var height = bottomRight.Y - topLeft.Y;
            groupContainer.Width = width;
            groupContainer.Height = height;
            groupContainer.RenderTransform = new TranslateTransform(topLeft.X, topLeft.Y);
        }
    }

    #endregion

    private void drawRootCanvasLines()
    {
        CanvasContainerCanvas.Children.Clear();

        for (var i = 0; i <= CanvasContainer.ActualWidth / SNAP_DISTANCE; i++)
        {
            var significant = i % (SIGNIFICANT_SNAP_DISTANCE / SNAP_DISTANCE) == 0;
            var xPos = i * SNAP_DISTANCE;
            var lineHeight = CanvasContainer.ActualHeight;

            if (!SNAP_ENABLED && !significant) continue;

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

            CanvasContainerCanvas.Children.Add(line);
        }

        for (var i = 0; i <= CanvasContainer.ActualHeight / SNAP_DISTANCE; i++)
        {
            var significant = i % (SIGNIFICANT_SNAP_DISTANCE / SNAP_DISTANCE) == 0;
            var yPos = i * SNAP_DISTANCE;
            var lineWidth = CanvasContainer.ActualWidth;

            if (!SNAP_ENABLED && !significant) continue;

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

            CanvasContainerCanvas.Children.Add(line);
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
    private List<NodeDragInstance>? groupDrag;
    private ConnectionDragInstance? connectionDrag;

    public ConnectionDragInstance? ConnectionDrag
    {
        get => connectionDrag;
        set
        {
            connectionDrag = value;
            OnPropertyChanged();
        }
    }

    private async void onConnectionsChanged(IEnumerable<NodeConnection> newConnections, IEnumerable<NodeConnection> oldConnections)
    {
        await Dispatcher.Yield(DispatcherPriority.Render);

        foreach (var newConnection in newConnections)
        {
            addConnection(newConnection);
        }

        foreach (var oldConnection in oldConnections)
        {
            removeConnection(oldConnection);
        }
    }

    private void removeConnection(NodeConnection connection)
    {
        var pathTag = connection.ConnectionType == ConnectionType.Flow
            ? $"flow_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}"
            : $"value_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}_{connection.InputSlot}";

        var pathToRemove = ConnectionCanvas.FindVisualChildWhere<Path>(path => (string)path.Tag == pathTag);
        ConnectionCanvas.Children.Remove(pathToRemove);

        foreach (var canvas in NodesItemsControl.FindVisualChildrenWhere<Canvas>(element => element.Name == "GroupConnectionCanvas"))
        {
            var pathToRemoveInner = canvas.FindVisualChildWhere<Path>(path => (string)path.Tag == pathTag);
            canvas.Children.Remove(pathToRemoveInner);
        }
    }

    private void updateConnectionsForNode(Node node)
    {
        var connections = NodeScape.Connections.Where(connection => connection.OutputNodeId == node.Id || connection.InputNodeId == node.Id);

        foreach (var connection in connections)
        {
            removeConnection(connection);
            addConnection(connection);
        }
    }

    private FrameworkElement getNodeContainerFromId(Guid nodeId)
    {
        var nodeContainer = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "NodeContainer", Tag: Node node } && node.Id == nodeId);
        Debug.Assert(nodeContainer is not null);
        return nodeContainer;
    }

    private FrameworkElement getOutputSlotElementForConnection(NodeConnection connection)
    {
        var outputNodeElement = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "NodeContainer", Tag: Node node } && node.Id == connection.OutputNodeId);
        Debug.Assert(outputNodeElement is not null);
        var outputSlotName = connection.ConnectionType == ConnectionType.Flow ? $"flow_output_{connection.OutputSlot}" : $"value_output_{connection.OutputSlot}";
        var outputSlotElement = outputNodeElement.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == outputSlotName);
        Debug.Assert(outputSlotElement is not null);
        return outputSlotElement;
    }

    private FrameworkElement getInputSlotElementForConnection(NodeConnection connection)
    {
        var inputNodeElement = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "NodeContainer", Tag: Node node } && node.Id == connection.InputNodeId);
        Debug.Assert(inputNodeElement is not null);
        var inputSlotName = connection.ConnectionType == ConnectionType.Flow ? $"flow_input_{connection.InputSlot}" : $"value_input_{connection.InputSlot}";
        var inputSlotElement = inputNodeElement.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == inputSlotName);
        Debug.Assert(inputSlotElement is not null);
        return inputSlotElement;
    }

    private void addConnection(NodeConnection connection)
    {
        var outputSlotElement = getOutputSlotElementForConnection(connection);
        var inputSlotElement = getInputSlotElementForConnection(connection);

        var pathTag = connection.ConnectionType == ConnectionType.Flow
            ? $"flow_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}"
            : $"value_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}_{connection.InputSlot}";

        // TODO: Update paths when a node gets added to or removed from a group

        // check if output node is in group
        // check if input node is in group
        // TODO: decide where to render

        drawConnectionPath(pathTag, outputSlotElement, inputSlotElement, connection.ConnectionType);
    }

    private void drawConnectionPath(string tag, FrameworkElement outputSlotElement, FrameworkElement inputSlotElement, ConnectionType connectionType)
    {
        var outputElementXOffset = outputSlotElement.Width / 2d;
        var outputElementYOffset = outputSlotElement.Height / 2d;
        var inputElementXOffset = inputSlotElement.Width / 2d;
        var inputElementYOffset = inputSlotElement.Height / 2d;

        var outputElementPosRelativeToCanvas = outputSlotElement.TranslatePoint(new Point(0, 0), CanvasContainer) + new Vector(outputElementXOffset, outputElementYOffset);
        var inputElementPosRelativeToCanvas = inputSlotElement.TranslatePoint(new Point(0, 0), CanvasContainer) + new Vector(inputElementXOffset, inputElementYOffset);

        var startPoint = outputElementPosRelativeToCanvas;
        var endPoint = inputElementPosRelativeToCanvas;
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.75d, 75d);
        var controlPoint1 = Point.Add(startPoint, new Vector(delta, 0));
        var controlPoint2 = Point.Add(endPoint, new Vector(-delta, 0));

        //Logger.Log(startPoint + " - " + controlPoint1 + " - " + controlPoint2 + " - " + endPoint, LoggingTarget.Information);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        var bezierSegment = new BezierSegment();

        pathGeometry.Figures.Add(pathFigure);
        pathFigure.Segments.Add(bezierSegment);

        pathFigure.StartPoint = startPoint;
        bezierSegment.Point1 = controlPoint1;
        bezierSegment.Point2 = controlPoint2;
        bezierSegment.Point3 = endPoint;

        var path = new Path
        {
            Tag = tag,
            Data = pathGeometry,
            Stroke = connectionType == ConnectionType.Value ? Brushes.Red : Brushes.Aqua,
            StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };

        ConnectionCanvas.Children.Add(path);

        foreach (var canvas in NodesItemsControl.FindVisualChildrenWhere<Canvas>(element => element.Name == "GroupConnectionCanvas"))
        {
            var pathInner = new Path
            {
                Tag = tag,
                Data = pathGeometry,
                Stroke = connectionType == ConnectionType.Value ? Brushes.Red : Brushes.Aqua,
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            canvas.Children.Add(pathInner);
        }
    }

    #region Canvas Dragging

    private TranslateTransform getRootPosition()
    {
        return (TranslateTransform)RootContainer.RenderTransform;
    }

    private void RootContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != CANVAS_DRAG_BUTTON) return;

        e.Handled = true;

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(ParentContainer);
        var transform = getRootPosition();

        var offsetX = mousePosRelativeToNodesItemsControl.X - transform.X;
        var offsetY = mousePosRelativeToNodesItemsControl.Y - transform.Y;

        Logger.Log($"{nameof(RootContainer_OnMouseDown)}: Starting node drag. Offset X {offsetX}. Offset Y {offsetY}", LoggingTarget.Information);
        canvasDrag = new CanvasDragInstance(offsetX, offsetY);
    }

    private void ParentContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        Logger.Log($"{nameof(ParentContainer_OnMouseUp)}", LoggingTarget.Information);

        if (canvasDrag is not null && e.ChangedButton == CANVAS_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(ParentContainer_OnMouseUp)}: Ending canvas drag", LoggingTarget.Information);
            canvasDrag = null;
            return;
        }

        if (nodeDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(ParentContainer_OnMouseUp)}: Ending node drag", LoggingTarget.Information);
            nodeDrag = null;
            return;
        }

        if (ConnectionDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(ParentContainer_OnMouseUp)}: Ending connection drag", LoggingTarget.Information);
            ConnectionDrag = null;
        }
    }

    private void ParentContainer_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (canvasDrag is not null)
        {
            var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(ParentContainer);

            var newCanvasX = mousePosRelativeToNodesItemsControl.X - canvasDrag.OffsetX;
            var newCanvasY = mousePosRelativeToNodesItemsControl.Y - canvasDrag.OffsetY;

            var canvasBoundsX = CanvasContainer.Width - CanvasContainer.Width / 2d;
            var canvasBoundsY = CanvasContainer.Height - CanvasContainer.Height / 2d;

            newCanvasX = Math.Clamp(newCanvasX, -canvasBoundsX, canvasBoundsX);
            newCanvasY = Math.Clamp(newCanvasY, -canvasBoundsY, canvasBoundsY);

            //Logger.Log($"{nameof(NodesItemsControl_OnMouseMove)}: Setting canvas drag position to {newCanvasX},{newCanvasY}", LoggingTarget.Information);

            var canvasPosition = getRootPosition();
            canvasPosition.X = newCanvasX;
            canvasPosition.Y = newCanvasY;
        }

        if (ConnectionDrag is not null)
        {
            e.Handled = true;
            updateConnectionDragPath();
        }
    }

    #endregion

    private void CanvasContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        return;

        if (e.ChangedButton != CONTEXT_MENU_BUTTON) return;

        var mousePosRelativeToScreen = Mouse.GetPosition(this);
        Logger.Log(mousePosRelativeToScreen.X + "," + mousePosRelativeToScreen.Y, LoggingTarget.Information);

        //ContextMenuMode = Nodes.ContextMenuMode.Add;
        var translation = (TranslateTransform)ContextMenu.RenderTransform;
        translation.X = mousePosRelativeToScreen.X;
        translation.Y = mousePosRelativeToScreen.Y;
    }

    private void CanvasContainer_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (nodeDrag is not null)
        {
            updateNodePosition(nodeDrag);
        }

        if (groupDrag is not null)
        {
            foreach (var groupNodeDrag in groupDrag)
            {
                updateNodePosition(groupNodeDrag);
            }
        }
    }

    private void CanvasContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (nodeDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;

            checkForGroupAdditions();

            nodeDrag = null;
            Logger.Log($"{nameof(CanvasContainer_OnMouseUp)}: Ending node drag", LoggingTarget.Information);
        }

        if (groupDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(CanvasContainer_OnMouseUp)}: Ending group drag", LoggingTarget.Information);
            groupDrag = null;
        }
    }

    private async void checkForGroupAdditions()
    {
        Debug.Assert(nodeDrag is not null);

        if (NodeScape.Groups.Any(nodeGroup => nodeGroup.Nodes.Contains(nodeDrag.Node.Id))) return;

        NodeGroup? groupToUpdate = null;

        foreach (var groupContainer in NodesItemsControl.FindVisualChildrenWhere<Border>(element => element.Name == "GroupContainer").OrderByDescending(Panel.GetZIndex))
        {
            var mousePosRelativeToGroupContainer = Mouse.GetPosition(groupContainer);
            var bounds = new Rect(0, 0, groupContainer.ActualWidth, groupContainer.ActualHeight);
            if (!bounds.Contains(mousePosRelativeToGroupContainer)) continue;

            groupToUpdate = (NodeGroup)groupContainer.Tag;
        }

        if (groupToUpdate is null) return;

        groupToUpdate.Nodes.Add(nodeDrag.Node.Id);
        await Dispatcher.Yield(DispatcherPriority.Loaded);
        updateGroups([groupToUpdate]);
    }

    #region Group Dragging

    private void GroupContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Log($"{nameof(GroupContainer_OnMouseDown)}", LoggingTarget.Information);

        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;
        canvasDrag = null;

        var element = (Border)sender;
        var group = (NodeGroup)element.Tag;

        Logger.Log($"{nameof(GroupContainer_OnMouseDown)}: Starting group drag", LoggingTarget.Information);

        groupDrag = new List<NodeDragInstance>();

        foreach (var nodeId in group.Nodes)
        {
            var node = NodeScape.Nodes[nodeId];
            var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);
            var nodePos = node.Position;

            var offsetX = mousePosRelativeToNodesItemsControl.X - nodePos.X;
            var offsetY = mousePosRelativeToNodesItemsControl.Y - nodePos.Y;

            if (SNAP_ENABLED)
            {
                offsetX = Math.Round(offsetX / SNAP_DISTANCE) * SNAP_DISTANCE;
                offsetY = Math.Round(offsetY / SNAP_DISTANCE) * SNAP_DISTANCE;
            }

            Logger.Log($"{nameof(NodeContainer_OnMouseDown)}: Starting node drag. Offset X {offsetX}. Offset Y {offsetY}", LoggingTarget.Information);
            groupDrag.Add(new NodeDragInstance(node, offsetX, offsetY));
        }
    }

    #endregion

    #region Node Dragging

    private void NodeContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;
        canvasDrag = null;

        var element = (Border)sender;
        var node = (Node)element.Tag;

        if (NodesItemsControl.ItemContainerGenerator.ContainerFromItem(node) is FrameworkElement container)
        {
            node.ZIndex = NodeScape.ZIndex++;

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

    private void updateNodePosition(NodeDragInstance nodeDrag)
    {
        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);

        var newNodeX = mousePosRelativeToNodesItemsControl.X - nodeDrag.OffsetX;
        var newNodeY = mousePosRelativeToNodesItemsControl.Y - nodeDrag.OffsetY;

        if (SNAP_ENABLED)
        {
            newNodeX = Math.Round(newNodeX / SNAP_DISTANCE) * SNAP_DISTANCE;
            newNodeY = Math.Round(newNodeY / SNAP_DISTANCE) * SNAP_DISTANCE;
        }

        newNodeX = Math.Clamp(newNodeX, 0, CanvasContainer.Width);
        newNodeY = Math.Clamp(newNodeY, 0, CanvasContainer.Height);

        var positionChanged = Math.Abs(newNodeX - nodeDrag.Node.Position.X) > double.Epsilon || Math.Abs(newNodeY - nodeDrag.Node.Position.Y) > double.Epsilon;

        //Logger.Log($"{nameof(NodesItemsControl_OnMouseMove)}: Setting node drag position to {newNodeX},{newNodeY}", LoggingTarget.Information);

        nodeDrag.Node.Position.X = newNodeX;
        nodeDrag.Node.Position.Y = newNodeY;

        if (positionChanged)
        {
            updateConnectionsForNode(nodeDrag.Node);
            updateGroups(NodeScape.Groups.Where(group => group.Nodes.Contains(nodeDrag.Node.Id)));
        }
    }

    #endregion

    #region Connection Points

    // TODO: Combine all these into methods that use the tag

    private void FlowInput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        // TODO: if there's already a connection going into this slot, remove the connection

        // Check if a connection already comes into this input
        var existingConnection =
            NodeScape.Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.InputNodeId == node.Id && connection.InputSlot == slot);

        if (existingConnection is not null)
        {
            NodeScape.Connections.Remove(existingConnection);
            node = NodeScape.Nodes[existingConnection.OutputNodeId];
            slot = existingConnection.OutputSlot;
            element = getOutputSlotElementForConnection(existingConnection);
            Logger.Log($"{nameof(FlowInput_OnMouseDown)}: Starting output flow drag from node {node.Title} in slot {slot}", LoggingTarget.Information);
            ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.FlowOutput, node, slot, element);
        }
        else
        {
            Logger.Log($"{nameof(FlowInput_OnMouseDown)}: Starting input flow drag from node {node.Title} in slot {slot}", LoggingTarget.Information);
            ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.FlowInput, node, slot, element);
        }

        updateConnectionDragPath();
    }

    private void FlowInput_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ConnectionDrag is null) return;

        e.Handled = true;

        if (ConnectionDrag.Origin != ConnectionDragOrigin.FlowOutput)
        {
            ConnectionDrag = null;
            return;
        }

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(FlowInput_OnMouseUp)}: Ending input flow drag on node {node.Title} in slot {slot}", LoggingTarget.Information);
        NodeScape.CreateFlowConnection(ConnectionDrag.Node.Id, ConnectionDrag.Slot, node.Id);

        ConnectionDrag = null;
    }

    private void FlowOutput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(FlowOutput_OnMouseDown)}: Starting output flow drag from node {node.Title} in slot {slot}", LoggingTarget.Information);
        ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.FlowOutput, node, slot, element);

        updateConnectionDragPath();
    }

    private void FlowOutput_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ConnectionDrag is null) return;

        e.Handled = true;

        if (ConnectionDrag.Origin != ConnectionDragOrigin.FlowInput)
        {
            ConnectionDrag = null;
            return;
        }

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(FlowOutput_OnMouseUp)}: Ending output flow drag on node {node.Title} in slot {slot}", LoggingTarget.Information);
        NodeScape.CreateFlowConnection(node.Id, slot, ConnectionDrag.Node.Id);

        ConnectionDrag = null;
    }

    private void ValueInput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        // Check if a connection already comes into this input
        var existingConnection =
            NodeScape.Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Value && connection.InputNodeId == node.Id && connection.InputSlot == slot);

        if (existingConnection is not null)
        {
            NodeScape.Connections.Remove(existingConnection);
            node = NodeScape.Nodes[existingConnection.OutputNodeId];
            slot = existingConnection.OutputSlot;
            element = getOutputSlotElementForConnection(existingConnection);
            Logger.Log($"{nameof(ValueInput_OnMouseDown)}: Starting output value drag from node {node.Title} in slot {slot}", LoggingTarget.Information);
            ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.ValueOutput, node, slot, element);
        }
        else
        {
            Logger.Log($"{nameof(ValueInput_OnMouseDown)}: Starting input value drag from node {node.Title} in slot {slot}", LoggingTarget.Information);
            ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.ValueInput, node, slot, element);
        }

        updateConnectionDragPath();
    }

    private void ValueInput_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ConnectionDrag is null) return;

        e.Handled = true;

        if (ConnectionDrag.Origin != ConnectionDragOrigin.ValueOutput)
        {
            ConnectionDrag = null;
            return;
        }

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(ValueInput_OnMouseUp)}: Ending input value drag on node {node.Title} in slot {slot}", LoggingTarget.Information);
        NodeScape.CreateValueConnection(ConnectionDrag.Node.Id, ConnectionDrag.Slot, node.Id, slot);
        ConnectionDrag = null;
    }

    private void ValueOutput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(ValueOutput_OnMouseDown)}: Starting output value drag from node {node.Title} in slot {slot}", LoggingTarget.Information);
        ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.ValueOutput, node, slot, element);

        updateConnectionDragPath();
    }

    private void ValueOutput_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ConnectionDrag is null) return;

        e.Handled = true;

        if (ConnectionDrag.Origin != ConnectionDragOrigin.ValueInput)
        {
            ConnectionDrag = null;
            return;
        }

        var element = (FrameworkElement)sender;
        var node = (Node)element.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(ValueOutput_OnMouseUp)}: Ending output value drag on node {node.GetType().Name} in slot {slot}", LoggingTarget.Information);
        NodeScape.CreateValueConnection(node.Id, slot, ConnectionDrag.Node.Id, ConnectionDrag.Slot);
        ConnectionDrag = null;
    }

    #endregion

    private int getSlotFromConnectionElement(FrameworkElement element)
    {
        return int.Parse(((string)element.Tag).Split('_').Last());
    }

    private void updateConnectionDragPath()
    {
        if (ConnectionDrag is null) throw new Exception("Nope");

        var element = ConnectionDrag.Element;
        var centerOffsetX = element.Width / 2d;
        var centerOffsetY = element.Height / 2d;
        var elementPosRelativeToCanvas = element.TranslatePoint(new Point(0, 0), CanvasContainer) + new Vector(centerOffsetX, centerOffsetY);
        var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);

        var startPoint = elementPosRelativeToCanvas;
        var endPoint = mousePosRelativeToCanvas;
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.75d, 75d);
        var controlPoint1 = Point.Add(startPoint, new Vector(delta, 0));
        var controlPoint2 = Point.Add(endPoint, new Vector(-delta, 0));

        //Logger.Log(startPoint + " - " + controlPoint1 + " - " + controlPoint2 + " - " + endPoint, LoggingTarget.Information);

        var path = (Path)DragCanvas.Children[0];
        var pathGeometry = (PathGeometry)path.Data;
        var pathFigure = pathGeometry.Figures[0];
        var curve = (BezierSegment)pathFigure.Segments[0];

        pathFigure.StartPoint = startPoint;
        curve.Point1 = controlPoint1;
        curve.Point2 = controlPoint2;
        curve.Point3 = endPoint;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public record NodeDragInstance(Node Node, double OffsetX, double OffsetY);

public record CanvasDragInstance(double OffsetX, double OffsetY);

public record ConnectionDragInstance(ConnectionDragOrigin Origin, Node Node, int Slot, FrameworkElement Element);

public enum ConnectionDragOrigin
{
    FlowOutput,
    FlowInput,
    ValueOutput,
    ValueInput
}

public class NodeGroupViewModel
{
    public ObservableCollection<Node> Nodes { get; set; } = [];
    public NodeGroup NodeGroup { get; set; }
}