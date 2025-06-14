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
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Types.Base;
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

#pragma warning disable CS0162 // Unreachable code detected

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodeGraphView : INotifyPropertyChanged
{
    public const MouseButton CANVAS_DRAG_BUTTON = MouseButton.Middle;
    public const MouseButton ELEMENT_DRAG_BUTTON = MouseButton.Left;
    public const int SNAP_DISTANCE = 25;
    public const int SIGNIFICANT_SNAP_DISTANCE = SNAP_DISTANCE * 20;
    public const bool SNAP_ENABLED = true;
    public Padding GroupPadding { get; } = new(30, 55, 30, 30);
    public Padding SelectionPadding { get; } = new(20, 20, 20, 20);

    private WindowManager nodeCreatorWindowManager = null!;
    private WindowManager presetCreatorWindowManager = null!;

    private bool isFirstLoad = true;
    private Point? fieldContextMenuMousePosition;

    public ContextMenuRoot FieldContextMenu { get; } = new();
    public NodeGraph NodeGraph { get; }
    public ObservableCollection<object> NodesItemsControlItemsSource { get; } = [];

    public NodeGraphView(NodeGraph nodeGraph)
    {
        NodeGraph = nodeGraph;
        FieldContextMenu.Items.Add(ContextMenuBuilder.BuildCreateNodesMenu());
        FieldContextMenu.Items.Add(ContextMenuBuilder.BuildSpawnPresetMenu());

        InitializeComponent();
        KeyDown += OnKeyDown;
        Loaded += OnLoaded;
        DataContext = this;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!isFirstLoad) return;

        nodeCreatorWindowManager = new WindowManager(this);
        presetCreatorWindowManager = new WindowManager(this);

        GridCanvasContent.Content = new GridBackgroundVisualHost(CanvasContainer.Width, CanvasContainer.Height, SNAP_DISTANCE, SIGNIFICANT_SNAP_DISTANCE);
        updateNodeContainerZIndexes();

        NodeGraph.Nodes.OnCollectionChanged(onNodesChanged, true);
        NodeGraph.Connections.OnCollectionChanged(onConnectionsChanged, true);
        NodeGraph.Groups.OnCollectionChanged(onGroupsChanged, true);

        isFirstLoad = false;
    }

    public void FocusGrid()
    {
        Focus();
    }

    #region Helpers

    private async void updateNodePosition(Node node, double newX = double.NaN, double newY = double.NaN, bool yieldForRender = true, bool updateGroupVisuals = true)
    {
        if (yieldForRender)
            await Dispatcher.Yield(DispatcherPriority.Render);

        if (double.IsNaN(newX)) newX = node.Position.X;
        if (double.IsNaN(newY)) newY = node.Position.Y;

        var nodeContainer = getNodeContainerFromId(node.Id);
        var nodeOffset = getNodeSnapOffset(node, nodeContainer);

        newX += nodeOffset.X;
        newY += nodeOffset.Y;

        snapAndClampPosition(ref newX, ref newY, nodeContainer.ActualWidth, nodeContainer.ActualHeight);

        newX -= nodeOffset.X;
        newY -= nodeOffset.Y;

        var positionChanged = Math.Abs(newX - node.Position.X) > double.Epsilon || Math.Abs(newY - node.Position.Y) > double.Epsilon;
        if (!positionChanged) return;

        node.Position.X = newX;
        node.Position.Y = newY;

        redrawAllConnectionsForNode(node.Id, false);

        if (updateGroupVisuals)
            updateAllGroupVisuals(NodeGraph.Groups.Where(group => group.Nodes.Contains(node.Id)));
    }

    private (double X, double Y) getNodeSnapOffset(Node node, FrameworkElement nodeContainer)
    {
        var snappingSlotElement = getSnappingSlotElementForConnection(node, nodeContainer);
        var elementPos = snappingSlotElement.TranslatePoint(new Point(0, 0), CanvasContainer) + new Vector(snappingSlotElement.ActualWidth / 2d, snappingSlotElement.ActualHeight / 2d);

        return (elementPos.X - node.Position.X, elementPos.Y - node.Position.Y);
    }

    private FrameworkElement getSnappingSlotElementForConnection(Node node, FrameworkElement nodeContainer)
    {
        var outputSlotName = string.Empty;

        if (node.Metadata.IsFlowOutput)
            outputSlotName = "flow_output_0";
        else if (node.Metadata.IsFlowInput)
            outputSlotName = "flow_input_0";
        else if (node.Metadata.IsValueOutput)
            outputSlotName = "value_output_0";
        else if (node.Metadata.IsValueInput)
            outputSlotName = "value_input_0";

        if (string.IsNullOrEmpty(outputSlotName)) throw new InvalidOperationException();

        var outputSlotElement = nodeContainer.FindVisualChildWhere<FrameworkElement>(element => element.Tag is string slotTag && slotTag == outputSlotName);
        Debug.Assert(outputSlotElement is not null);

        return outputSlotElement;
    }

    private void snapAndClampPosition(ref double x, ref double y, double width = double.NaN, double height = double.NaN)
    {
        if (SNAP_ENABLED)
        {
            x = Math.Round(x / SNAP_DISTANCE) * SNAP_DISTANCE;
            y = Math.Round(y / SNAP_DISTANCE) * SNAP_DISTANCE;
        }

        if (!double.IsNaN(width) && !double.IsNaN(height))
        {
            x = Math.Clamp(x, 0, CanvasContainer.Width - width);
            y = Math.Clamp(y, 0, CanvasContainer.Height - height);
        }
    }

    #endregion

    #region Setup

    private void updateNodeContainerZIndexes()
    {
        var itemsControl = NodesItemsControl;

        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is not FrameworkElement container) continue;

            var pair = (Node)container.DataContext;
            Panel.SetZIndex(container, pair.ZIndex.Value);
        }
    }

    #endregion

    #region Updates

    private void onNodesChanged(IEnumerable<ObservableKeyValuePair<Guid, Node>> newNodes, IEnumerable<ObservableKeyValuePair<Guid, Node>> oldNodes)
    {
        var newNodesActual = newNodes.Select(pair => pair.Value);

        NodesItemsControlItemsSource.AddRange(newNodesActual);
        NodesItemsControlItemsSource.RemoveIf(o => o is Node node && oldNodes.Select(pair => pair.Value).Contains(node));

        foreach (var pair in newNodesActual) updateNodePosition(pair);
    }

    private void onConnectionsChanged(IEnumerable<NodeConnection> newConnections, IEnumerable<NodeConnection> oldConnections)
    {
        foreach (var newConnection in newConnections)
        {
            createConnectionVisual(newConnection);
        }

        foreach (var oldConnection in oldConnections)
        {
            removeConnectionVisual(oldConnection);
        }
    }

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
                foreach (var newNodeId in newNodes)
                {
                    nodeGroupViewModel.Nodes.Add(NodeGraph.Nodes[newNodeId]);
                    NodesItemsControlItemsSource.RemoveIf(o => o is Node node && newGroup.Nodes.Contains(node.Id));
                }

                foreach (var oldNodeId in oldNodes)
                {
                    nodeGroupViewModel.Nodes.RemoveIf(node => oldNodeId == node.Id);
                    NodesItemsControlItemsSource.Add(NodeGraph.Nodes[oldNodeId]);
                }
            }, true);

            NodesItemsControlItemsSource.Add(nodeGroupViewModel);
        }

        foreach (var oldGroup in oldGroups)
        {
            NodesItemsControlItemsSource.RemoveIf(obj => obj is NodeGroupViewModel nodeGroupViewModel && nodeGroupViewModel.NodeGroup == oldGroup);
        }

        await Dispatcher.Yield(DispatcherPriority.Loaded);
        updateAllGroupVisuals(newGroups);
    }

    #endregion

    #region Root Key Controls

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            resetCanvasPosition();
        }
    }

    private void resetCanvasPosition()
    {
        canvasDrag = null;

        var canvasPosition = getRootPosition();
        if (canvasPosition.X == 0 && canvasPosition.Y == 0) return;

        var pf = new PathFigure
        {
            StartPoint = new Point(canvasPosition.X, canvasPosition.Y),
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

        animX.Completed += (_, _) => canvasPosition.X = 0;
        animY.Completed += (_, _) => canvasPosition.Y = 0;

        Storyboard.SetTarget(animX, RootContainer);
        Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

        Storyboard.SetTarget(animY, RootContainer);
        Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

        var sb = new Storyboard();
        sb.Children.Add(animX);
        sb.Children.Add(animY);
        sb.Begin();
    }

    #endregion

    #region Groups

    private FrameworkElement getGroupContainerFromId(Guid groupId)
    {
        var groupContainer = NodesItemsControl.FindVisualChildWhere<FrameworkElement>(element => element is { Name: "GroupContainer", Tag: NodeGroup group } && group.Id == groupId);
        Debug.Assert(groupContainer is not null);
        return groupContainer;
    }

    private void updateAllGroupVisuals(IEnumerable<NodeGroup> groups)
    {
        foreach (var group in groups)
        {
            updateGroupVisual(group);
        }
    }

    private async void updateGroupVisual(NodeGroup group)
    {
        await Dispatcher.Yield(DispatcherPriority.Loaded);

        var topLeft = new Point(CanvasContainer.Width, CanvasContainer.Height);
        var bottomRight = new Point(0, 0);

        foreach (var nodeId in group.Nodes)
        {
            var node = NodeGraph.Nodes[nodeId];
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

    #endregion

    #region Connections

    private string getPathTagForConnection(NodeConnection connection)
    {
        return connection.ConnectionType == ConnectionType.Flow
            ? $"flow_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}"
            : $"value_{connection.OutputNodeId}_{connection.OutputSlot}_{connection.InputNodeId}_{connection.InputSlot}";
    }

    private async void createConnectionVisual(NodeConnection connection, bool yieldForRender = true)
    {
        if (yieldForRender)
            await Dispatcher.Yield(DispatcherPriority.Render);

        var outputSlotElement = getOutputSlotElementForConnection(connection);
        var inputSlotElement = getInputSlotElementForConnection(connection);
        if (outputSlotElement is null || inputSlotElement is null) return;

        // check if output node is in group
        // check if input node is in group
        // TODO: decide where to render

        var pathTag = getPathTagForConnection(connection);
        drawConnectionPath(pathTag, outputSlotElement, inputSlotElement, connection);
    }

    private async void removeConnectionVisual(NodeConnection connection, bool yieldForRender = true)
    {
        if (yieldForRender)
            await Dispatcher.Yield(DispatcherPriority.Render);

        var pathTag = getPathTagForConnection(connection);

        ConnectionCanvas.RemoveChildrenWhere<Path>(path => (string)path.Tag == pathTag);

        foreach (var canvas in NodesItemsControl.FindVisualChildrenWhere<Canvas>(element => element.Name == "GroupConnectionCanvas"))
        {
            canvas.RemoveChildWhere<Path>(path => (string)path.Tag == pathTag);
        }
    }

    private void redrawAllConnectionsForNode(Guid nodeId, bool yieldForRender = true)
    {
        var connections = NodeGraph.Connections.Where(connection => connection.OutputNodeId == nodeId || connection.InputNodeId == nodeId);

        foreach (var connection in connections)
        {
            removeConnectionVisual(connection, yieldForRender);
            createConnectionVisual(connection, yieldForRender);
        }
    }

    #endregion

    #region Drag

    private SelectionCreateInstance? selectionCreate;
    private SelectionDragInstance? selectionDrag;
    private CanvasDragInstance? canvasDrag;
    private NodeDragInstance? nodeDrag;
    private GroupDragInstance? groupDrag;
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
        var minDelta = Math.Min(Math.Abs(endPoint.Y - startPoint.Y) / 2d, 50d);
        var delta = Math.Max(Math.Abs(endPoint.X - startPoint.X) * 0.5d, minDelta);
        var controlPoint1 = Point.Add(startPoint, new Vector(delta, 0));
        var controlPoint2 = Point.Add(endPoint, new Vector(-delta, 0));

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        var bezierSegment = new BezierSegment();

        pathGeometry.Figures.Add(pathFigure);
        pathFigure.Segments.Add(bezierSegment);

        pathFigure.StartPoint = startPoint;
        bezierSegment.Point1 = controlPoint1;
        bezierSegment.Point2 = controlPoint2;
        bezierSegment.Point3 = endPoint;

        bezierSegment.Freeze();
        pathFigure.Freeze();
        pathGeometry.Freeze();

        Brush brush;

        if (connection.ConnectionType == ConnectionType.Flow)
        {
            brush = Brushes.DeepSkyBlue;
        }
        else
        {
            brush = new LinearGradientBrush(connection.OutputType?.GetTypeBrush().Color ?? Brushes.Black.Color, connection.InputType?.GetTypeBrush().Color ?? Brushes.Black.Color, new Point(0, 0),
                new Point(1, 1))
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = startPoint,
                EndPoint = endPoint
            };
        }

        var path = new Path
        {
            Tag = tag,
            Data = pathGeometry,
            Stroke = brush,
            StrokeThickness = 3.5,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };

        ConnectionCanvas.Children.Add(path);
    }

    private TranslateTransform getRootPosition()
    {
        return (TranslateTransform)RootContainer.RenderTransform;
    }

    private void RootContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == CANVAS_DRAG_BUTTON)
        {
            e.Handled = true;

            var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(ParentContainer);
            var transform = getRootPosition();

            var offsetX = mousePosRelativeToNodesItemsControl.X - transform.X;
            var offsetY = mousePosRelativeToNodesItemsControl.Y - transform.Y;

            canvasDrag = new CanvasDragInstance(offsetX, offsetY);
        }

        if (e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;

            var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);
            var posX = mousePosRelativeToCanvas.X;
            var posY = mousePosRelativeToCanvas.Y;
            selectionCreate = new SelectionCreateInstance(posX, posY);
        }
    }

    private void ParentContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var scope = FocusManager.GetFocusScope(this);
        FocusManager.SetFocusedElement(scope, this);

        if (canvasDrag is not null && e.ChangedButton == CANVAS_DRAG_BUTTON)
        {
            e.Handled = true;
            canvasDrag = null;
            NodeGraph.Serialise();
            return;
        }

        if (nodeDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            nodeDrag = null;
            NodeGraph.Serialise();
            return;
        }

        if (ConnectionDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            ConnectionDrag = null;
            NodeGraph.Serialise();
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
                var outputNode = NodeGraph.AddNode(type);

                NodeGraph.CreateValueConnection(outputNode.Id, 0, node.Id, slot);

                outputNode.Position.X = mousePosRelativeToCanvas.X;
                outputNode.Position.Y = mousePosRelativeToCanvas.Y;

                ConnectionDrag = null;
            }
        }

        if (ConnectionDrag is not null && ConnectionDrag.Origin == ConnectionDragOrigin.ValueOutput)
        {
            var node = ConnectionDrag.Node;
            var slot = ConnectionDrag.Slot;

            var slotOutputType = node.GetTypeOfOutputSlot(slot);

            e.Handled = true;

            var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);

            var type = typeof(DisplayNode<>).MakeGenericType(slotOutputType);
            var inputNode = NodeGraph.AddNode(type);

            NodeGraph.CreateValueConnection(node.Id, slot, inputNode.Id, 0);

            inputNode.Position.X = mousePosRelativeToCanvas.X;
            inputNode.Position.Y = mousePosRelativeToCanvas.Y;

            ConnectionDrag = null;
        }

        if (ConnectionDrag is not null && ConnectionDrag.Origin == ConnectionDragOrigin.FlowInput)
        {
            e.Handled = true;

            var node = ConnectionDrag.Node;
            var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);

            var outputNode = NodeGraph.AddNode(typeof(ButtonNode));

            NodeGraph.CreateFlowConnection(outputNode.Id, 0, node.Id);
            outputNode.Position.X = mousePosRelativeToCanvas.X;
            outputNode.Position.Y = mousePosRelativeToCanvas.Y;

            ConnectionDrag = null;
        }

        if (e.Handled) return;

        fieldContextMenuMousePosition = Mouse.GetPosition(CanvasContainer);
    }

    private void CanvasContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton is ELEMENT_DRAG_BUTTON)
        {
            SelectionContainer.Visibility = Visibility.Collapsed;
        }
    }

    private void CanvasContainer_OnMouseMove(object sender, MouseEventArgs e)
    {
        updateGroupDrag();
        updateNodeDrag();
        updateSelectionDrag();
        updateSelectionCreate();
    }

    private void GroupContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var groupContainer = (FrameworkElement)sender;
        var group = (NodeGroup)groupContainer.Tag;

        startGroupDrag(group, groupContainer);
    }

    private void NodeContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        SelectionContainer.Visibility = Visibility.Collapsed;

        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;
        canvasDrag = null;

        var element = (Border)sender;
        var node = (Node)element.Tag;

        startNodeDrag(node);
    }

    private void SelectionContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        selectionDrag = null;
    }

    private void SelectionContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        startSelectionDrag();
    }

    private void startGroupDrag(NodeGroup group, FrameworkElement groupContainer)
    {
        var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);
        var groupPos = (TranslateTransform)groupContainer.RenderTransform;

        var offsetX = mousePosRelativeToCanvas.X - groupPos.X;
        var offsetY = mousePosRelativeToCanvas.Y - groupPos.Y;

        groupDrag = new GroupDragInstance(group.Id, offsetX, offsetY);
    }

    private void startNodeDrag(Node node)
    {
        if (NodesItemsControl.ItemContainerGenerator.ContainerFromItem(node) is FrameworkElement container)
        {
            node.ZIndex.Value = NodeGraph.ZIndex++;
            Panel.SetZIndex(container, node.ZIndex.Value);
        }

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);
        var nodePos = node.Position;

        var offsetX = mousePosRelativeToNodesItemsControl.X - nodePos.X;
        var offsetY = mousePosRelativeToNodesItemsControl.Y - nodePos.Y;

        nodeDrag = new NodeDragInstance(node.Id, offsetX, offsetY);
    }

    private void startSelectionDrag()
    {
        var bounds = new Rect(0, 0, SelectionContainer.ActualWidth, SelectionContainer.ActualHeight);

        var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);
        var selectionPos = (TranslateTransform)SelectionContainer.RenderTransform;

        var offsetX = mousePosRelativeToCanvas.X - selectionPos.X;
        var offsetY = mousePosRelativeToCanvas.Y - selectionPos.Y;

        var selectedNodes = new List<Guid>();

        foreach (var nodeContainer in NodesItemsControl.FindVisualChildrenWhere<FrameworkElement>(child => child.Name == "NodeContainer"))
        {
            var startPoint = nodeContainer.TranslatePoint(new Point(0, 0), SelectionContainer);
            var endPoint = nodeContainer.TranslatePoint(new Point(nodeContainer.ActualWidth, nodeContainer.ActualHeight), SelectionContainer);

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
                selectedNodes.Add(((Node)nodeContainer.Tag).Id);
        }

        selectionDrag = new SelectionDragInstance(selectedNodes.ToArray(), offsetX, offsetY);
    }

    private void updateGroupDrag()
    {
        if (groupDrag is null) return;

        var groupContainer = getGroupContainerFromId(groupDrag.Group);
        var groupTransform = (TranslateTransform)groupContainer.RenderTransform;

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);

        var newX = Math.Round(mousePosRelativeToNodesItemsControl.X - groupDrag.OffsetX);
        var newY = Math.Round(mousePosRelativeToNodesItemsControl.Y - groupDrag.OffsetY);

        snapAndClampPosition(ref newX, ref newY, groupContainer.ActualWidth, groupContainer.ActualHeight);

        var positionChanged = Math.Abs(newX - groupTransform.X) > double.Epsilon || Math.Abs(newY - groupTransform.Y) > double.Epsilon;
        if (!positionChanged) return;

        var diffX = newX - groupTransform.X;
        var diffY = newY - groupTransform.Y;

        // I have literally no idea
        diffX /= 2;

        var nodeGroup = NodeGraph.GetGroupById(groupDrag.Group);

        foreach (var nodeId in nodeGroup.Nodes)
        {
            var node = NodeGraph.Nodes[nodeId];
            updateNodePosition(node, node.Position.X + diffX, node.Position.Y + diffY, false, false);
        }

        updateGroupVisual(nodeGroup);
    }

    private void updateNodeDrag()
    {
        if (nodeDrag is null) return;

        var mousePosRelativeToNodesItemsControl = Mouse.GetPosition(CanvasContainer);

        var newX = Math.Round(mousePosRelativeToNodesItemsControl.X - nodeDrag.OffsetX);
        var newY = Math.Round(mousePosRelativeToNodesItemsControl.Y - nodeDrag.OffsetY);

        var node = NodeGraph.Nodes[nodeDrag.Node];
        updateNodePosition(node, newX, newY);
    }

    private async void updateSelectionDrag()
    {
        if (selectionDrag is null) return;

        var selectionTransform = (TranslateTransform)SelectionContainer.RenderTransform;
        var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);

        var newX = Math.Round(mousePosRelativeToCanvas.X - selectionDrag.OffsetX);
        var newY = Math.Round(mousePosRelativeToCanvas.Y - selectionDrag.OffsetY);

        snapAndClampPosition(ref newX, ref newY, SelectionContainer.ActualWidth, SelectionContainer.ActualHeight);

        var positionChanged = Math.Abs(newX - selectionTransform.X) > double.Epsilon || Math.Abs(newY - selectionTransform.Y) > double.Epsilon;
        if (!positionChanged) return;

        var diffX = newX - selectionTransform.X;
        var diffY = newY - selectionTransform.Y;

        selectionTransform.X = newX;
        selectionTransform.Y = newY;

        //await Dispatcher.Yield(DispatcherPriority.Render);

        foreach (var nodeId in selectionDrag.Nodes)
        {
            var node = NodeGraph.Nodes[nodeId];
            updateNodePosition(node, node.Position.X + diffX, node.Position.Y + diffY, false);
        }
    }

    private void updateSelectionCreate()
    {
        if (selectionCreate is null) return;

        SelectionContainer.Visibility = Visibility.Visible;

        var mousePosRelativeToCanvas = Mouse.GetPosition(CanvasContainer);
        var posX = Math.Min(mousePosRelativeToCanvas.X, selectionCreate.PosX);
        var posY = Math.Min(mousePosRelativeToCanvas.Y, selectionCreate.PosY);

        var width = Math.Abs(mousePosRelativeToCanvas.X - selectionCreate.PosX);
        var height = Math.Abs(mousePosRelativeToCanvas.Y - selectionCreate.PosY);

        SelectionContainer.Width = width;
        SelectionContainer.Height = height;
        SelectionContainer.RenderTransform = new TranslateTransform(posX, posY);
    }

    private void CanvasContainer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        checkForGroupAdditions();

        if (nodeDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            nodeDrag = null;
            Logger.Log($"{nameof(CanvasContainer_OnMouseUp)}: Ending node drag", LoggingTarget.Information);
        }

        if (groupDrag is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(CanvasContainer_OnMouseUp)}: Ending group drag", LoggingTarget.Information);
            groupDrag = null;
        }

        if (selectionCreate is not null && e.ChangedButton == ELEMENT_DRAG_BUTTON)
        {
            e.Handled = true;
            Logger.Log($"{nameof(CanvasContainer_OnMouseUp)}: Ending selection create", LoggingTarget.Information);
            selectionCreate = null;
            shrinkWrapSelection();
        }

        NodeGraph.Serialise();
        FocusGrid();
    }

    private void checkForGroupAdditions()
    {
        if (nodeDrag is null) return;
        if (NodeGraph.Groups.Any(nodeGroup => nodeGroup.Nodes.Contains(nodeDrag.Node))) return;

        NodeGroup? groupToUpdate = null;

        foreach (var groupContainer in NodesItemsControl.FindVisualChildrenWhere<Border>(element => element.Name == "GroupContainer").OrderByDescending(Panel.GetZIndex))
        {
            var mousePosRelativeToGroupContainer = Mouse.GetPosition(groupContainer);
            var bounds = new Rect(0, 0, groupContainer.ActualWidth, groupContainer.ActualHeight);
            if (!bounds.Contains(mousePosRelativeToGroupContainer)) continue;

            groupToUpdate = (NodeGroup)groupContainer.Tag;
        }

        if (groupToUpdate is null) return;

        groupToUpdate.Nodes.Add(nodeDrag.Node);
        updateGroupVisual(groupToUpdate);
    }

    private void shrinkWrapSelection()
    {
        var bounds = new Rect(0, 0, SelectionContainer.ActualWidth, SelectionContainer.ActualHeight);

        var topLeft = new Point(CanvasContainer.ActualWidth, CanvasContainer.ActualHeight);
        var bottomRight = new Point(0, 0);

        var foundNodes = false;

        foreach (var nodeContainer in NodesItemsControl.FindVisualChildrenWhere<FrameworkElement>(child => child.Name == "NodeContainer"))
        {
            var startPoint = nodeContainer.TranslatePoint(new Point(0, 0), SelectionContainer);
            var endPoint = nodeContainer.TranslatePoint(new Point(nodeContainer.ActualWidth, nodeContainer.ActualHeight), SelectionContainer);

            var nodeContainerPosition = (TranslateTransform)nodeContainer.RenderTransform;

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
            {
                foundNodes = true;
                topLeft.X = Math.Min(topLeft.X, nodeContainerPosition.X - SelectionPadding.Left);
                topLeft.Y = Math.Min(topLeft.Y, nodeContainerPosition.Y - SelectionPadding.Top);
                bottomRight.X = Math.Max(bottomRight.X, nodeContainerPosition.X + nodeContainer.ActualWidth + SelectionPadding.Right);
                bottomRight.Y = Math.Max(bottomRight.Y, nodeContainerPosition.Y + nodeContainer.ActualHeight + SelectionPadding.Bottom);
            }
        }

        SelectionContainer.Visibility = foundNodes ? Visibility.Visible : Visibility.Collapsed;
        if (!foundNodes) return;

        SelectionContainer.RenderTransform = new TranslateTransform(topLeft.X, topLeft.Y);
        SelectionContainer.Width = bottomRight.X - topLeft.X;
        SelectionContainer.Height = bottomRight.Y - topLeft.Y;
    }

    #endregion

    #region Connection Points

    private void updateConnectionDragPath()
    {
        if (ConnectionDrag is null) throw new InvalidOperationException("Cannot update connection drag path when there's no connection drag");

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

        var path = (Path)DragCanvas.Children[0];
        var pathGeometry = (PathGeometry)path.Data;
        var pathFigure = pathGeometry.Figures[0];
        var curve = (BezierSegment)pathFigure.Segments[0];

        pathFigure.StartPoint = startPoint;
        curve.Point1 = controlPoint1;
        curve.Point2 = controlPoint2;
        curve.Point3 = endPoint;
    }

    private int getSlotFromConnectionElement(FrameworkElement element)
    {
        return int.Parse(((string)element.Tag).Split('_').Last());
    }

    private void FlowInput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        var existingConnection =
            NodeGraph.Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.InputNodeId == node.Id && connection.InputSlot == slot);

        if (existingConnection is not null)
        {
            NodeGraph.Connections.Remove(existingConnection);
            node = NodeGraph.Nodes[existingConnection.OutputNodeId];
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
        NodeGraph.CreateFlowConnection(ConnectionDrag.Node.Id, ConnectionDrag.Slot, node.Id);

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
        NodeGraph.CreateFlowConnection(node.Id, slot, ConnectionDrag.Node.Id);

        ConnectionDrag = null;
    }

    private void ValueInput_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != ELEMENT_DRAG_BUTTON) return;

        e.Handled = true;

        var element = (FrameworkElement)sender;
        var node = (Node)element.FindVisualParent<ItemsControl>()!.DataContext;
        var slot = getSlotFromConnectionElement(element);

        var existingConnection =
            NodeGraph.Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Value && connection.InputNodeId == node.Id && connection.InputSlot == slot);

        if (existingConnection is not null)
        {
            NodeGraph.Connections.Remove(existingConnection);
            node = NodeGraph.Nodes[existingConnection.OutputNodeId];
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
        NodeGraph.CreateValueConnection(ConnectionDrag.Node.Id, ConnectionDrag.Slot, node.Id, slot);
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
        NodeGraph.CreateValueConnection(node.Id, slot, ConnectionDrag.Node.Id, ConnectionDrag.Slot);
        ConnectionDrag = null;
    }

    #endregion

    #region Graph Context Menu

    private void GraphContextMenu_NodeTypeItemClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeType = (Type)element.Tag;

        Debug.Assert(fieldContextMenuMousePosition is not null);

        var graphContextMenuPos = fieldContextMenuMousePosition.Value;
        var posX = graphContextMenuPos.X;
        var posY = graphContextMenuPos.Y;
        snapAndClampPosition(ref posX, ref posY);

        if (nodeType.IsGenericTypeDefinition)
        {
            var nodeCreatorWindow = new NodeCreatorWindow(NodeGraph, nodeType);

            nodeCreatorWindow.Closed += (_, _) =>
            {
                if (nodeCreatorWindow.ConstructedType is null) return;

                var node = NodeGraph.AddNode(nodeCreatorWindow.ConstructedType);
                node.Position.X = posX;
                node.Position.Y = posY;
                updateNodePosition(node);
            };

            nodeCreatorWindowManager.TrySpawnChild(nodeCreatorWindow);
        }
        else
        {
            var node = NodeGraph.AddNode(nodeType);
            node.Position.X = posX;
            node.Position.Y = posY;
            updateNodePosition(node);
        }

        ConnectionDrag = null;
    }

    private void FieldContextMenu_PresetItemClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var preset = (NodePreset)element.Tag;

        Debug.Assert(fieldContextMenuMousePosition is not null);

        var fieldContextMenuPos = fieldContextMenuMousePosition.Value;
        var posX = fieldContextMenuPos.X;
        var posY = fieldContextMenuPos.Y;

        snapAndClampPosition(ref posX, ref posY);
        preset.SpawnTo(NodeGraph, posX, posY);
    }

    #endregion

    #region Node Context Menu

    private void NodeContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        NodeGraph.DeleteNode(node.Id);
    }

    #endregion

    #region Group Context Menu

    private void GroupContextMenu_DissolveClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGroup = ((NodeGroupViewModel)element.Tag).NodeGroup;

        nodeGroup.Nodes.RemoveIf(_ => true);
        NodeGraph.Groups.Remove(nodeGroup);
    }

    private void GroupContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var nodeGroup = ((NodeGroupViewModel)element.Tag).NodeGroup;

        var nodes = nodeGroup.Nodes.ToList();

        foreach (var node in nodes)
        {
            NodeGraph.DeleteNode(node);
        }
    }

    #endregion

    #region Selection Context Menu

    private void SelectionContextMenu_CreateGroupClick(object sender, RoutedEventArgs e)
    {
        var bounds = new Rect(0, 0, SelectionContainer.ActualWidth, SelectionContainer.ActualHeight);

        var selectedNodes = new List<Guid>();

        foreach (var nodeContainer in NodesItemsControl.FindVisualChildrenWhere<FrameworkElement>(child => child.Name == "NodeContainer"))
        {
            var startPoint = nodeContainer.TranslatePoint(new Point(0, 0), SelectionContainer);
            var endPoint = nodeContainer.TranslatePoint(new Point(nodeContainer.ActualWidth, nodeContainer.ActualHeight), SelectionContainer);

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
                selectedNodes.Add(((Node)nodeContainer.Tag).Id);
        }

        selectedNodes.RemoveIf(nodeId => NodeGraph.Groups.Any(nodeGroup => nodeGroup.Nodes.Contains(nodeId)));
        SelectionContainer.Visibility = Visibility.Collapsed;

        if (selectedNodes.Count == 0) return;

        var group = NodeGraph.AddGroup();
        group.Title.Value = "Selection";
        group.Nodes.AddRange(selectedNodes);
    }

    private void SelectionContextMenu_SaveAsPresetClick(object sender, RoutedEventArgs e)
    {
        var bounds = new Rect(0, 0, SelectionContainer.ActualWidth, SelectionContainer.ActualHeight);

        var selectedNodes = new List<Guid>();

        foreach (var nodeContainer in NodesItemsControl.FindVisualChildrenWhere<FrameworkElement>(child => child.Name == "NodeContainer"))
        {
            var startPoint = nodeContainer.TranslatePoint(new Point(0, 0), SelectionContainer);
            var endPoint = nodeContainer.TranslatePoint(new Point(nodeContainer.ActualWidth, nodeContainer.ActualHeight), SelectionContainer);

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
                selectedNodes.Add(((Node)nodeContainer.Tag).Id);
        }

        SelectionContainer.Visibility = Visibility.Collapsed;
        var position = (TranslateTransform)SelectionContainer.RenderTransform;

        var presetCreatorWindow = new PresetCreatorWindow();

        presetCreatorWindow.Closed += (_, _) =>
        {
            if (string.IsNullOrEmpty(presetCreatorWindow.PresetName)) return;

            NodeGraph.CreatePreset(presetCreatorWindow.PresetName, selectedNodes, (float)position.X, (float)position.Y);
            FieldContextMenu.Items.RemoveAt(1);
            FieldContextMenu.Items.Add(ContextMenuBuilder.BuildSpawnPresetMenu());
        };

        presetCreatorWindowManager.TrySpawnChild(presetCreatorWindow);
    }

    private void SelectionContextMenu_DeleteAllClick(object sender, RoutedEventArgs e)
    {
        var bounds = new Rect(0, 0, SelectionContainer.ActualWidth, SelectionContainer.ActualHeight);

        var selectedNodes = new List<Guid>();

        foreach (var nodeContainer in NodesItemsControl.FindVisualChildrenWhere<FrameworkElement>(child => child.Name == "NodeContainer"))
        {
            var startPoint = nodeContainer.TranslatePoint(new Point(0, 0), SelectionContainer);
            var endPoint = nodeContainer.TranslatePoint(new Point(nodeContainer.ActualWidth, nodeContainer.ActualHeight), SelectionContainer);

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
                selectedNodes.Add(((Node)nodeContainer.Tag).Id);
        }

        SelectionContainer.Visibility = Visibility.Collapsed;

        foreach (var selectedNode in selectedNodes)
        {
            NodeGraph.DeleteNode(selectedNode);
        }
    }

    #endregion

    #region Variable Size Buttons

    private void ValueOutputAddButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        node.VariableSize.ValueOutputSize++;

        NodeGraph.Serialise();

        var valueOutputItemsControl = element.FindVisualParent<FrameworkElement>("ValueOutputContainer")!.FindVisualChild<ItemsControl>("ValueOutputItemsControl")!;
        valueOutputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void ValueOutputRemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        if (node.VariableSize.ValueOutputSize == 1) return;

        var outputSlot = node.Metadata.OutputsCount + node.VariableSize.ValueOutputSize - 1;
        var connectionToRemove = NodeGraph.Connections.SingleOrDefault(c => c.ConnectionType == ConnectionType.Value && c.OutputNodeId == node.Id && c.OutputSlot == outputSlot);

        if (connectionToRemove is not null)
            NodeGraph.Connections.Remove(connectionToRemove);

        node.VariableSize.ValueOutputSize--;

        NodeGraph.Serialise();

        var valueOutputItemsControl = element.FindVisualParent<FrameworkElement>("ValueOutputContainer")!.FindVisualChild<ItemsControl>("ValueOutputItemsControl")!;
        valueOutputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void ValueInputAddButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        node.VariableSize.ValueInputSize++;

        NodeGraph.Serialise();

        var valueInputItemsControl = element.FindVisualParent<FrameworkElement>("ValueInputContainer")!.FindVisualChild<ItemsControl>("ValueInputItemsControl")!;
        valueInputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    private void ValueInputRemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        if (node.VariableSize.ValueInputSize == 1) return;

        var inputSlot = node.Metadata.InputsCount + node.VariableSize.ValueInputSize - 1;
        var connectionToRemove = NodeGraph.Connections.SingleOrDefault(c => c.ConnectionType == ConnectionType.Value && c.InputNodeId == node.Id && c.InputSlot == inputSlot);

        if (connectionToRemove is not null)
            NodeGraph.Connections.Remove(connectionToRemove);

        node.VariableSize.ValueInputSize--;

        NodeGraph.Serialise();

        var valueInputItemsControl = element.FindVisualParent<FrameworkElement>("ValueInputContainer")!.FindVisualChild<ItemsControl>("ValueInputItemsControl")!;
        valueInputItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty)!.UpdateTarget();
    }

    #endregion

    #region Group Title

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
            NodeGraph.Name.Value = "New Graph";
            return;
        }

        NodeGraph.Name.Value = NodeGraphTitleTextBox.Text;
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

    #region Button Node

    private void ButtonInputNode_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var node = (Node)element.Tag;

        NodeGraph.StartFlow(node);
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public record BaseDragInstance(double OffsetX, double OffsetY);

public record GroupDragInstance(Guid Group, double OffsetX, double OffsetY) : BaseDragInstance(OffsetX, OffsetY);

public record NodeDragInstance(Guid Node, double OffsetX, double OffsetY) : BaseDragInstance(OffsetX, OffsetY);

public record CanvasDragInstance(double OffsetX, double OffsetY) : BaseDragInstance(OffsetX, OffsetY);

public record SelectionCreateInstance(double PosX, double PosY);

public record SelectionDragInstance(Guid[] Nodes, double OffsetX, double OffsetY) : BaseDragInstance(OffsetX, OffsetY);

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