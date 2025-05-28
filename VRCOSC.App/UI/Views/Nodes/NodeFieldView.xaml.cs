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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows.Nodes;
using VRCOSC.App.Utils;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;
using Vector = System.Windows.Vector;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodeFieldView : INotifyPropertyChanged
{
    public const MouseButton CANVAS_DRAG_BUTTON = MouseButton.Middle;
    public const MouseButton ELEMENT_DRAG_BUTTON = MouseButton.Left;
    public const int SNAP_DISTANCE = 20;
    public const int SIGNIFICANT_SNAP_DISTANCE = SNAP_DISTANCE * 20;
    public const bool SNAP_ENABLED = true;
    public Padding GroupPadding { get; } = new(30, 55, 30, 30);

    private WindowManager nodeCreatorWindowManager = null!;

    public ContextMenuRoot FieldContextMenu { get; set; }

    private bool loaded;

    public NodeField NodeField { get; }

    public NodeFieldView(NodeField nodeField)
    {
        NodeField = nodeField;
        InitializeComponent();
        KeyDown += OnKeyDown;
        Loaded += OnLoaded;
        DataContext = this;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (loaded) return;

        nodeCreatorWindowManager = new WindowManager(this);

        FieldContextMenu = new();
        FieldContextMenu.Items.Add(ContextMenuBuilder.BuildCreateNodesMenu());

        drawRootCanvasLines();
        setZIndexesOfNodes();

        NodeField.Nodes.OnCollectionChanged(onNodesChanged, true);
        NodeField.Connections.OnCollectionChanged(onConnectionsChanged, true);
        NodeField.Groups.OnCollectionChanged(onGroupsChanged, true);

        loaded = true;
    }

    public void FocusMainContainer()
    {
        Focus();
    }

    public ObservableCollection<object> NodesItemsControlItemsSource { get; } = [];

    private async void onGroupsChanged(IEnumerable<NodeGroup> newGroups, IEnumerable<NodeGroup> oldGroups)
    {
        foreach (var newGroup in newGroups)
        {
            var nodeGroupViewModel = new NodeGroupViewModel
            {
                NodeGroup = newGroup
            };

            newGroup.Nodes.OnCollectionChanged(async (newNodes, oldNodes) =>
            {
                foreach (var newNodeId in newNodes)
                {
                    nodeGroupViewModel.Nodes.Add(NodeField.Nodes[newNodeId]);
                    NodesItemsControlItemsSource.RemoveIf(o => o is Node node && newGroup.Nodes.Contains(node.Id));
                }

                foreach (var oldNodeId in oldNodes)
                {
                    nodeGroupViewModel.Nodes.RemoveIf(node => oldNodeId == node.Id);
                    NodesItemsControlItemsSource.Add(NodeField.Nodes[oldNodeId]);
                }

                await Dispatcher.Yield(DispatcherPriority.Loaded);
                updateGroups(newGroups);
            }, true);

            NodesItemsControlItemsSource.Add(nodeGroupViewModel);
        }

        foreach (var oldGroup in oldGroups)
        {
            NodesItemsControlItemsSource.RemoveIf(obj => obj is NodeGroupViewModel nodeGroupViewModel && nodeGroupViewModel.NodeGroup == oldGroup);
        }

        await Dispatcher.Yield(DispatcherPriority.Loaded);
        updateGroups(newGroups);
    }

    private async void onNodesChanged(IEnumerable<ObservableKeyValuePair<Guid, Node>> newNodes, IEnumerable<ObservableKeyValuePair<Guid, Node>> oldNodes)
    {
        NodesItemsControlItemsSource.AddRange(newNodes.Select(pair => pair.Value));
        NodesItemsControlItemsSource.RemoveIf(o => o is Node node && oldNodes.Select(pair => pair.Value).Contains(node));

        await Dispatcher.Yield(DispatcherPriority.Loaded);
        newNodes.ForEach(pair => updateNodePosition(pair.Value));
        updateGroups(NodeField.Groups);
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
                var node = NodeField.Nodes[nodeId];
                var nodeContainer = getNodeContainerFromId(nodeId);

                topLeft.X = Math.Min(topLeft.X, node.Position.X - GroupPadding.Left);
                topLeft.Y = Math.Min(topLeft.Y, node.Position.Y - GroupPadding.Top);
                bottomRight.X = Math.Max(bottomRight.X, node.Position.X + nodeContainer.ActualWidth + GroupPadding.Right);
                bottomRight.Y = Math.Max(bottomRight.Y, node.Position.Y + nodeContainer.ActualHeight + GroupPadding.Bottom);
            }

            var groupContainer = getGroupContainerFromId(group.Id);

            var width = bottomRight.X - topLeft.X;
            var height = bottomRight.Y - topLeft.Y;

            width = Math.Max(width, 0);
            height = Math.Max(height, 0);

            groupContainer.Width = width;
            groupContainer.Height = height;
            groupContainer.RenderTransform = new TranslateTransform(topLeft.X, topLeft.Y);
        }
    }

    #endregion

    private void drawRootCanvasLines()
    {
        CanvasContainerCanvas.Children.Clear();

        for (var i = 0; i <= CanvasContainer.Width / SNAP_DISTANCE; i++)
        {
            var significant = i % (SIGNIFICANT_SNAP_DISTANCE / SNAP_DISTANCE) == 0;
            var xPos = i * SNAP_DISTANCE;
            var lineHeight = CanvasContainer.Height;

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

        for (var i = 0; i <= CanvasContainer.Height / SNAP_DISTANCE; i++)
        {
            var significant = i % (SIGNIFICANT_SNAP_DISTANCE / SNAP_DISTANCE) == 0;
            var yPos = i * SNAP_DISTANCE;
            var lineWidth = CanvasContainer.Width;

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

            var pair = (Node)container.DataContext;
            Panel.SetZIndex(container, pair.ZIndex.Value);
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
        var connections = NodeField.Connections.Where(connection => connection.OutputNodeId == node.Id || connection.InputNodeId == node.Id);

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

    private FrameworkElement? getOutputSlotElementForConnection(NodeConnection connection)
    {
        var outputNodeElement = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "NodeContainer", Tag: Node node } && node.Id == connection.OutputNodeId);
        if (outputNodeElement is null) return null;

        var outputSlotName = connection.ConnectionType == ConnectionType.Flow ? $"flow_output_{connection.OutputSlot}" : $"value_output_{connection.OutputSlot}";
        var outputSlotElement = outputNodeElement.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == outputSlotName);
        return outputSlotElement;
    }

    private FrameworkElement? getInputSlotElementForConnection(NodeConnection connection)
    {
        var inputNodeElement = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "NodeContainer", Tag: Node node } && node.Id == connection.InputNodeId);
        if (inputNodeElement is null) return null;

        var inputSlotName = connection.ConnectionType == ConnectionType.Flow ? $"flow_input_{connection.InputSlot}" : $"value_input_{connection.InputSlot}";
        var inputSlotElement = inputNodeElement.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == inputSlotName);
        return inputSlotElement;
    }

    private void addConnection(NodeConnection connection)
    {
        var outputSlotElement = getOutputSlotElementForConnection(connection);
        var inputSlotElement = getInputSlotElementForConnection(connection);

        if (outputSlotElement is null || inputSlotElement is null) return;

        var pathTag = connection.ConnectionType == ConnectionType.Flow
            ? $"flow_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}"
            : $"value_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}_{connection.InputSlot}";

        // TODO: Update paths when a node gets added to or removed from a group

        // check if output node is in group
        // check if input node is in group
        // TODO: decide where to render

        drawConnectionPath(pathTag, outputSlotElement, inputSlotElement, connection);
    }

    private void drawConnectionPath(string tag, FrameworkElement outputSlotElement, FrameworkElement inputSlotElement, NodeConnection connection)
    {
        var outputElementXOffset = outputSlotElement.Width / 2d;
        var outputElementYOffset = outputSlotElement.Height / 2d;
        var inputElementXOffset = inputSlotElement.Width / 2d;
        var inputElementYOffset = inputSlotElement.Height / 2d;

        var outputElementPosRelativeToCanvas = outputSlotElement.TranslatePoint(new Point(0, 0), CanvasContainer) + new Vector(outputElementXOffset, outputElementYOffset);
        var inputElementPosRelativeToCanvas = inputSlotElement.TranslatePoint(new Point(0, 0), CanvasContainer) + new Vector(inputElementXOffset, inputElementYOffset);

        var startPoint = outputElementPosRelativeToCanvas;
        var endPoint = inputElementPosRelativeToCanvas;
        var minDelta = Math.Min(Math.Abs(endPoint.Y - startPoint.Y) / 2d, 75d);
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.5d, minDelta);
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
            Stroke = connection.ConnectionType == ConnectionType.Value ? connection.OutputType?.GetTypeBrush() : Brushes.DeepSkyBlue,
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
                Stroke = connection.ConnectionType == ConnectionType.Value ? connection.OutputType?.GetTypeBrush() : Brushes.DeepSkyBlue,
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
        DependencyObject scope = FocusManager.GetFocusScope(this);
        FocusManager.SetFocusedElement(scope, this);

        Logger.Log($"{nameof(ParentContainer_OnMouseUp)}", LoggingTarget.Information);

        if (canvasDrag is not null && e.ChangedButton == CANVAS_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(ParentContainer_OnMouseUp)}: Ending canvas drag", LoggingTarget.Information);
            canvasDrag = null;
            NodeField.Serialise();
            return;
        }

        if (nodeDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(ParentContainer_OnMouseUp)}: Ending node drag", LoggingTarget.Information);
            nodeDrag = null;
            NodeField.Serialise();
            return;
        }

        if (ConnectionDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(ParentContainer_OnMouseUp)}: Ending connection drag", LoggingTarget.Information);
            ConnectionDrag = null;
            NodeField.Serialise();
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
    }

    private Point? contextMenuMousePosition;

    private void ParentContainer_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (ConnectionDrag is not null && ConnectionDrag.Origin == ConnectionDragOrigin.ValueInput)
        {
            var node = ConnectionDrag.Node;
            var slot = ConnectionDrag.Slot;

            var slotInputType = node.GetTypeOfInputSlot(slot);
            if (slotInputType.IsGenericType && slotInputType.GetGenericTypeDefinition() == typeof(Nullable<>)) slotInputType = slotInputType.GenericTypeArguments[0];

            if (NodeConstants.INPUT_TYPES.Contains(slotInputType) || slotInputType.IsAssignableTo(typeof(Enum)) || slotInputType == typeof(Keybind))
            {
                e.Handled = true;

                var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);

                var type = typeof(ValueNode<>).MakeGenericType(slotInputType);
                var outputNode = NodeField.AddNode(type);

                NodeField.CreateValueConnection(outputNode.Id, 0, node.Id, slot);
                outputNode.Position.X = mousePosRelativeToCanvas.X;
                outputNode.Position.Y = mousePosRelativeToCanvas.Y;

                ConnectionDrag = null;
            }
        }

        if (ConnectionDrag is not null && ConnectionDrag.Origin == ConnectionDragOrigin.FlowInput)
        {
            e.Handled = true;

            var node = ConnectionDrag.Node;
            var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);

            var outputNode = NodeField.AddNode(typeof(ButtonNode));

            NodeField.CreateFlowConnection(outputNode.Id, 0, node.Id);
            outputNode.Position.X = mousePosRelativeToCanvas.X;
            outputNode.Position.Y = mousePosRelativeToCanvas.Y;

            ConnectionDrag = null;
        }

        if (e.Handled) return;

        contextMenuMousePosition = Mouse.GetPosition(CanvasContainer);
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

        NodeField.Serialise();
    }

    private async void checkForGroupAdditions()
    {
        Debug.Assert(nodeDrag is not null);

        if (NodeField.Groups.Any(nodeGroup => nodeGroup.Nodes.Contains(nodeDrag.Node.Id))) return;

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
            var node = NodeField.Nodes[nodeId];
            var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);
            var nodePos = node.Position;

            var offsetX = Math.Round(mousePosRelativeToNodesItemsControl.X - nodePos.X);
            var offsetY = Math.Round(mousePosRelativeToNodesItemsControl.Y - nodePos.Y);

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
            node.ZIndex.Value = NodeField.ZIndex++;

            Panel.SetZIndex(container, node.ZIndex.Value);
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

        var newNodeX = Math.Round(mousePosRelativeToNodesItemsControl.X - nodeDrag.OffsetX);
        var newNodeY = Math.Round(mousePosRelativeToNodesItemsControl.Y - nodeDrag.OffsetY);

        if (SNAP_ENABLED)
        {
            newNodeX = Math.Round(newNodeX / SNAP_DISTANCE) * SNAP_DISTANCE;
            newNodeY = Math.Round(newNodeY / SNAP_DISTANCE) * SNAP_DISTANCE;
        }

        newNodeX = Math.Clamp(newNodeX, 0, CanvasContainer.Width);
        newNodeY = Math.Clamp(newNodeY, 0, CanvasContainer.Height);

        // don't check for epsilon purposefully
        var positionChanged = newNodeX != nodeDrag.Node.Position.X || newNodeY != nodeDrag.Node.Position.Y;

        //Logger.Log($"{nameof(NodesItemsControl_OnMouseMove)}: Setting node drag position to {newNodeX},{newNodeY}", LoggingTarget.Information);

        if (positionChanged)
        {
            nodeDrag.Node.Position.X = (int)newNodeX;
            nodeDrag.Node.Position.Y = (int)newNodeY;

            updateConnectionsForNode(nodeDrag.Node);
            updateGroups(NodeField.Groups.Where(group => group.Nodes.Contains(nodeDrag.Node.Id)));
        }
    }

    private void updateNodePosition(Node node)
    {
        var newNodeX = node.Position.X;
        var newNodeY = node.Position.Y;

        if (SNAP_ENABLED)
        {
            newNodeX = Math.Round(newNodeX / SNAP_DISTANCE) * SNAP_DISTANCE;
            newNodeY = Math.Round(newNodeY / SNAP_DISTANCE) * SNAP_DISTANCE;
        }

        newNodeX = Math.Clamp(newNodeX, 0, CanvasContainer.Width);
        newNodeY = Math.Clamp(newNodeY, 0, CanvasContainer.Height);

        var positionChanged = Math.Abs(newNodeX - node.Position.X) > double.Epsilon || Math.Abs(newNodeY - node.Position.Y) > double.Epsilon;

        node.Position.X = newNodeX;
        node.Position.Y = newNodeY;

        if (positionChanged)
        {
            updateConnectionsForNode(node);
            updateGroups(NodeField.Groups.Where(group => group.Nodes.Contains(node.Id)));
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
            NodeField.Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.InputNodeId == node.Id && connection.InputSlot == slot);

        if (existingConnection is not null)
        {
            NodeField.Connections.Remove(existingConnection);
            node = NodeField.Nodes[existingConnection.OutputNodeId];
            slot = existingConnection.OutputSlot;
            element = getOutputSlotElementForConnection(existingConnection);
            Logger.Log($"{nameof(FlowInput_OnMouseDown)}: Starting output flow drag from node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
            ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.FlowOutput, node, slot, element);
        }
        else
        {
            Logger.Log($"{nameof(FlowInput_OnMouseDown)}: Starting input flow drag from node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
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

        Logger.Log($"{nameof(FlowInput_OnMouseUp)}: Ending input flow drag on node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
        NodeField.CreateFlowConnection(ConnectionDrag.Node.Id, ConnectionDrag.Slot, node.Id);

        ConnectionDrag = null;
    }

    private void FlowOutput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(FlowOutput_OnMouseDown)}: Starting output flow drag from node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
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

        Logger.Log($"{nameof(FlowOutput_OnMouseUp)}: Ending output flow drag on node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
        NodeField.CreateFlowConnection(node.Id, slot, ConnectionDrag.Node.Id);

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
            NodeField.Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Value && connection.InputNodeId == node.Id && connection.InputSlot == slot);

        if (existingConnection is not null)
        {
            NodeField.Connections.Remove(existingConnection);
            node = NodeField.Nodes[existingConnection.OutputNodeId];
            slot = existingConnection.OutputSlot;
            element = getOutputSlotElementForConnection(existingConnection);
            Logger.Log($"{nameof(ValueInput_OnMouseDown)}: Starting output value drag from node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
            ConnectionDrag = new ConnectionDragInstance(ConnectionDragOrigin.ValueOutput, node, slot, element);
        }
        else
        {
            Logger.Log($"{nameof(ValueInput_OnMouseDown)}: Starting input value drag from node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
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

        Logger.Log($"{nameof(ValueInput_OnMouseUp)}: Ending input value drag on node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
        NodeField.CreateValueConnection(ConnectionDrag.Node.Id, ConnectionDrag.Slot, node.Id, slot);
        ConnectionDrag = null;
    }

    private void ValueOutput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(ValueOutput_OnMouseDown)}: Starting output value drag from node {node.Metadata.Title} in slot {slot}", LoggingTarget.Information);
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
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        Logger.Log($"{nameof(ValueOutput_OnMouseUp)}: Ending output value drag on node {node.GetType().Name} in slot {slot}", LoggingTarget.Information);
        NodeField.CreateValueConnection(node.Id, slot, ConnectionDrag.Node.Id, ConnectionDrag.Slot);
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

        var isReversed = ConnectionDrag.Origin is ConnectionDragOrigin.FlowInput or ConnectionDragOrigin.ValueInput;

        var startPoint = elementPosRelativeToCanvas;
        var endPoint = mousePosRelativeToCanvas;
        var minDelta = Math.Min(Math.Abs(endPoint.Y - startPoint.Y) / 2d, 75d);
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.5d, minDelta);
        var controlPoint1 = Point.Add(startPoint, new Vector(isReversed ? -delta : delta, 0));
        var controlPoint2 = Point.Add(endPoint, new Vector(isReversed ? delta : -delta, 0));

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

    private void FieldContextMenu_ItemClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeType = (Type)element.Tag;

        Debug.Assert(contextMenuMousePosition is not null);

        if (nodeType.IsGenericTypeDefinition)
        {
            var nodeCreatorWindow = new NodeCreatorWindow(NodeField, nodeType);
            nodeCreatorWindowManager.TrySpawnChild(nodeCreatorWindow);

            nodeCreatorWindow.Closed += (_, _) =>
            {
                if (nodeCreatorWindow.ConstructedType is null) return;

                var node = NodeField.AddNode(nodeCreatorWindow.ConstructedType);
                var mousePosRelativeToCanvas = contextMenuMousePosition.Value;

                node.Position.X = mousePosRelativeToCanvas.X;
                node.Position.Y = mousePosRelativeToCanvas.Y;

                updateNodePosition(node);
            };
        }
        else
        {
            var node = NodeField.AddNode(nodeType);
            var mousePosRelativeToCanvas = contextMenuMousePosition.Value;

            node.Position.X = mousePosRelativeToCanvas.X;
            node.Position.Y = mousePosRelativeToCanvas.Y;

            updateNodePosition(node);
        }

        ConnectionDrag = null;
    }

    private void NodeContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        NodeField.DeleteNode(node);
    }

    private void NodeContextMenu_CreateGroupClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        var group = NodeField.AddGroup();
        group.Nodes.Add(node.Id);
    }

    private void ValueOutputAddButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        node.VariableSize.ValueOutputSize++;

        var valueOutputItemsControl = element.FindVisualParent<FrameworkElement>("ValueOutputContainer")!.FindVisualChild<ItemsControl>("ValueOutputItemsControl")!;
        valueOutputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void ValueOutputRemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        if (node.VariableSize.ValueOutputSize == 1) return;

        node.VariableSize.ValueOutputSize--;

        var valueOutputItemsControl = element.FindVisualParent<FrameworkElement>("ValueOutputContainer")!.FindVisualChild<ItemsControl>("ValueOutputItemsControl")!;
        valueOutputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void ValueInputAddButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        node.VariableSize.ValueInputSize++;

        var valueInputItemsControl = element.FindVisualParent<FrameworkElement>("ValueInputContainer")!.FindVisualChild<ItemsControl>("ValueInputItemsControl")!;
        valueInputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void ValueInputRemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        if (node.VariableSize.ValueInputSize == 1) return;

        node.VariableSize.ValueInputSize--;

        var valueInputItemsControl = element.FindVisualParent<FrameworkElement>("ValueInputContainer")!.FindVisualChild<ItemsControl>("ValueInputItemsControl")!;
        valueInputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void GroupContextMenu_DissolveClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGroup = ((NodeGroupViewModel)element.Tag).NodeGroup;

        nodeGroup.Nodes.RemoveIf(_ => true);
    }

    private void GroupContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGroup = ((NodeGroupViewModel)element.Tag).NodeGroup;

        var nodesCopy = nodeGroup.Nodes.ToList();
        nodeGroup.Nodes.RemoveIf(_ => true);

        foreach (var node in nodesCopy)
        {
            NodeField.Nodes.Remove(node);
        }
    }

    private void ButtonInputNode_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        NodeField.StartFlow(node);
    }

    private void MainContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var contextMenu = (ContextMenu)sender;
        contextMenu.ItemsSource = FieldContextMenu.Items;
    }

    private void GroupTitle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var nodeGroupViewModel = (NodeGroupViewModel)element.Tag;

        nodeGroupViewModel.Editing = true;

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
        var nodeGroupViewModel = (NodeGroupViewModel)element.Tag;

        nodeGroupViewModel.Editing = false;
    }

    private void GroupTitleTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Focus();
        }
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

public class NodeGroupViewModel : INotifyPropertyChanged
{
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

    public ObservableCollection<Node> Nodes { get; set; } = [];
    public NodeGroup NodeGroup { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}