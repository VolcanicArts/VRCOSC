// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes.ViewModels;
using VRCOSC.App.UI.Windows.Nodes;
using VRCOSC.App.Utils;
using Xceed.Wpf.AvalonDock.Controls;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Vector = System.Windows.Vector;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodeGraphView
{
    public const double GRAPH_SIZE = 50_000;
    public const double SNAP_DISTANCE = 25d / 2d;
    public const MouseButton GRAPH_INTERACT_BUTTON = MouseButton.Left;
    public const MouseButton GRAPH_DRAG_BUTTON = MouseButton.Middle;
    public const MouseButton GRAPH_SECONDARY_BUTTON = MouseButton.Right;
    public Padding GroupPadding { get; } = new(30, 55, 30, 25);
    public Padding SelectionPadding { get; } = new((int)(SNAP_DISTANCE * 1.5f), (int)(SNAP_DISTANCE * 1.5f), (int)(SNAP_DISTANCE * 1.5f), (int)(SNAP_DISTANCE * 1.5f));

    public double GraphSize => GRAPH_SIZE;

    public NodeGraph Graph { get; }

    public ObservableCollection<GraphElementViewModel> GraphElements { get; } = [];
    public IEnumerable<IGraphVariable> GraphVariablesSource => Graph.GraphVariables.Values;

    private bool hasLoaded;

    private WindowManager nodeCreatorWindowManager = null!;
    private WindowManager variableCreatorWindowManager = null!;
    private WindowManager presetCreatorWindowManager = null!;

    public Observable<bool> ShowDetails { get; } = new(true);

    private SelectionCreate? selectionCreate;
    private SelectionDrag? selectionDrag;
    private ElementsSelection? elementsSelection;

    public NodeGraphView(NodeGraph graph)
    {
        InitializeComponent();
        Graph = graph;
        Graph.OnMarkedDirty += onGraphMarkedDirty;
        Loaded += OnLoaded;
        DataContext = this;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        nodeCreatorWindowManager = new WindowManager(this);
        variableCreatorWindowManager = new WindowManager(this);
        presetCreatorWindowManager = new WindowManager(this);
        refreshContextMenu();
        centerGraph();
        Task.Run(Graph.MarkDirty);
    }

    #region Util

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T snapToGrid<T>(T value) where T : IFloatingPointIeee754<T> => T.Round(value / (T.CreateChecked(25) / T.CreateChecked(2))) * (T.CreateChecked(25) / T.CreateChecked(2)) - T.CreateChecked(0.5);

    #endregion

    #region Records

    private record ElementOffset(GridGraphElementViewModel ViewModel, Point Offset);

    private record ConnectionDrag(FrameworkElement Control, INodeElement Element, int SlotIndex);

    private record SelectionCreate(Point Point);

    private record SelectionDrag(Vector Offset);

    private record ElementsSelection(GridGraphElementViewModel[] Items);

    private record GroupDrag(Vector Offset, Vector OffsetFromGrid, GroupViewModel GroupVm, IEnumerable<GridGraphElementViewModel> Items, IEnumerable<IConnection> Connections);

    #endregion

    #region Marked Dirty

    private async Task onGraphMarkedDirty(GraphChanges changes)
    {
        try
        {
            if (changes.RemovedNodes.Count != 0)
                GraphElements.RemoveIf(item => item is NodeViewModel vm && changes.RemovedNodes.Contains(vm.Node));

            if (changes.RemovedConnections.Count != 0)
                GraphElements.RemoveIf(item => item is ConnectionViewModel vm && changes.RemovedConnections.Contains(vm.Connection));

            if (changes.RemovedGroups.Count != 0)
                GraphElements.RemoveIf(item => item is GroupViewModel vm && changes.RemovedGroups.Contains(vm.Group));

            var addedNodes = new List<NodeViewModel>();
            var addedConnections = new List<ConnectionViewModel>();
            var addedGroups = new List<GroupViewModel>();

            var offset = GraphElements.Count;

            await Dispatcher.InvokeAsync(() =>
            {
                addedNodes.AddRange(changes.AddedNodes.Select(node => new NodeViewModel((INode)Graph.Elements[node.Id])).ToList());
                addedConnections.AddRange(changes.AddedConnections.Select(connection => new ConnectionViewModel(connection)).ToList());
                addedGroups.AddRange(changes.AddedGroups.Select(group => new GroupViewModel(group)).ToList());
                GraphElements.AddRange(addedNodes);
                GraphElements.AddRange(addedConnections);
                GraphElements.AddRange(addedGroups);
            });

            await Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < addedNodes.Count; i++)
                {
                    var nodeViewModel = addedNodes[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var nodeContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    nodeViewModel.Control = nodeContainer;
                    populateNodeViewModel(nodeViewModel);
                    updateGridGraphElementPosition(nodeViewModel, nodeViewModel.Position);
                }

                offset += addedNodes.Count;

                for (var i = 0; i < addedConnections.Count; i++)
                {
                    var connectionViewModel = addedConnections[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var connectionContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    connectionViewModel.Control = connectionContainer;
                    updateConnectionViewModelPoints(connectionViewModel);
                    connectionViewModel.IsVisible = true;
                }

                offset += addedConnections.Count;

                for (var i = 0; i < addedGroups.Count; i++)
                {
                    var nodeGroupGraphItem = addedGroups[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var groupContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    nodeGroupGraphItem.Control = groupContainer;
                    updateNodeGroupGraphItem(nodeGroupGraphItem);
                }
            }, DispatcherPriority.Render);

            Graph.Serialise();

            if (!hasLoaded)
            {
                Dispatcher.Invoke(() =>
                {
                    hasLoaded = true;
                    LoadingOverlay.FadeOut(250);
                });
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error occured when handling dirty graph");
        }
    }

    #endregion

    #region View Model Populators

    private static List<FrameworkElement>[] getElementControls(List<FrameworkElement> searchList, INodeSharedMetadata metadata, ConnectionPoint connectionPoint, int slotCount)
    {
        var listControlName = connectionPoint switch
        {
            ConnectionPoint.FlowOutput => "FlowOutputListTemplateInstance",
            ConnectionPoint.FlowInput => "FlowInputListTemplateInstance",
            ConnectionPoint.ValueOutput => "ValueOutputListTemplateInstance",
            ConnectionPoint.ValueInput => "ValueInputListTemplateInstance",
            _ => throw new ArgumentOutOfRangeException(nameof(connectionPoint), connectionPoint, null)
        };

        var listViewModelType = connectionPoint switch
        {
            ConnectionPoint.FlowOutput => typeof(NodeFlowOutputListViewModel),
            ConnectionPoint.FlowInput => typeof(NodeFlowInputListViewModel),
            ConnectionPoint.ValueOutput => typeof(NodeValueOutputListViewModel),
            ConnectionPoint.ValueInput => typeof(NodeValueInputListViewModel),
            _ => throw new ArgumentOutOfRangeException(nameof(connectionPoint), connectionPoint, null)
        };

        var viewModelType = connectionPoint switch
        {
            ConnectionPoint.FlowOutput => typeof(NodeFlowOutputViewModel),
            ConnectionPoint.FlowInput => typeof(NodeFlowInputViewModel),
            ConnectionPoint.ValueOutput => typeof(NodeValueOutputViewModel),
            ConnectionPoint.ValueInput => typeof(NodeValueInputViewModel),
            _ => throw new ArgumentOutOfRangeException(nameof(connectionPoint), connectionPoint, null)
        };

        const string control_name = "ConnectionPointContainer";
        var controls = new List<FrameworkElement>[slotCount];

        for (var slot = 0; slot < slotCount; slot++)
        {
            var elementSharedMetadata = metadata.Elements[connectionPoint][slot];
            controls[slot] = [];

            if (!elementSharedMetadata.IsList)
            {
                var slotElement = searchList.Single(c => c.Name == control_name && c.Tag?.GetType() == viewModelType && ((ConnectionPointViewModel)c.Tag).Element.Metadata.Shared.Slot == slot);
                controls[slot].Add(slotElement);
            }
            else
            {
                var slotElements = searchList.Single(c => c.Name == listControlName && c.Tag?.GetType() == listViewModelType && ((ConnectionPointListViewModel)c.Tag).Element.Metadata.Shared.Slot == slot)
                                             .FindVisualChildren<FrameworkElement>().Where(c => c.Name == control_name && c.Tag?.GetType() == viewModelType);
                controls[slot].AddRange(slotElements);
            }
        }

        return controls;
    }

    private void populateNodeViewModel(NodeViewModel vm)
    {
        var visualChildren = vm.Control.FindVisualChildren<FrameworkElement>().ToList();
        var sharedMetadata = vm.Node.Metadata.Shared;

        if (sharedMetadata.IsFlowOutput)
            vm.FlowOutputControls = getElementControls(visualChildren, sharedMetadata, ConnectionPoint.FlowOutput, sharedMetadata.FlowOutputCount);

        if (sharedMetadata.IsFlowInput)
            vm.FlowInputControls = getElementControls(visualChildren, sharedMetadata, ConnectionPoint.FlowInput, sharedMetadata.FlowInputCount);

        if (sharedMetadata.IsValueOutput)
            vm.ValueOutputControls = getElementControls(visualChildren, sharedMetadata, ConnectionPoint.ValueOutput, sharedMetadata.ValueOutputCount);

        if (sharedMetadata.IsValueInput)
            vm.ValueInputControls = getElementControls(visualChildren, sharedMetadata, ConnectionPoint.ValueInput, sharedMetadata.ValueInputCount);

        populateNodeViewModelSnapOffset(vm);
    }

    private void updateConnectionViewModelPoints(ConnectionViewModel vm)
    {
        var outputNodeVm = GraphElements.OfType<NodeViewModel>().Single(n => n.Node.Id == vm.Connection.OutputId);
        var inputNodeVm = GraphElements.OfType<NodeViewModel>().Single(n => n.Node.Id == vm.Connection.InputId);

        updateConnectionOfNode(vm, outputNodeVm);
        updateConnectionOfNode(vm, inputNodeVm);
        vm.CreatePath();
    }

    private void populateNodeViewModelSnapOffset(NodeViewModel vm)
    {
        var control = getSnappingControl(vm);
        vm.SnappingControl = control;

        var controlPos = control.TranslatePoint(new Point(control.ActualWidth / 2d, control.ActualHeight / 2d), GraphContainer);
        var xOffset = controlPos.X - vm.Position.X;
        var yOffset = controlPos.Y - vm.Position.Y;
        vm.SnapOffset = new Point(xOffset, yOffset);
    }

    /// <summary>
    /// Chooses a control to calculate the snap offset from
    /// </summary>
    private static FrameworkElement getSnappingControl(NodeViewModel vm)
    {
        var metadata = vm.Node.Metadata.Shared;

        if (metadata.IsFlowOutput) return vm.FlowOutputControls[0][0];
        if (metadata.IsFlowInput) return vm.FlowInputControls[0][0];
        if (metadata.IsValueOutput) return vm.ValueOutputControls[0][0];
        if (metadata.IsValueInput) return vm.ValueInputControls[0][0];

        throw new Exception($"Unable to get snapping control for node {vm.Node}");
    }

    #endregion

    #region GraphControl

    private Point lastGraphPointerPos;

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        lastGraphPointerPos = e.GetPosition(GraphContainer);
        if (e.Handled) return;

        GraphContainer.Focus();
        handleMouseUpdates(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        updateGridGraphElementDrag(e);
        updateGraphDrag(e);
        updateConnectionDrag();
        updateGroupDrag();
        updateSelectionCreate();
        updateSelectionDrag();
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        handleMouseUpdates(e);
    }

    private void handleMouseUpdates(MouseButtonEventArgs e)
    {
        var mousePos = e.GetPosition(GraphContainer);
        var snappedMousePos = new Vector2((float)snapToGrid(mousePos.X), (float)snapToGrid(mousePos.Y));

        if (e is { ChangedButton: GRAPH_DRAG_BUTTON, ButtonState: MouseButtonState.Pressed } && graphDragMousePos is null)
        {
            graphDragMousePos = e.GetPosition(this);
            GraphContainer.CaptureMouse();
        }

        if (e is { ChangedButton: GRAPH_DRAG_BUTTON, ButtonState: MouseButtonState.Released } && graphDragMousePos is not null)
        {
            graphDragMousePos = null;
            GraphContainer.ReleaseMouseCapture();
        }

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON } && connectionDrag is not null)
        {
            endConnectionDrag();
        }

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Pressed } && selectionCreate is null)
        {
            selectionCreate = new SelectionCreate(e.GetPosition(GraphContainer));
            SelectionVisual.Visibility = Visibility.Visible;
            GraphContainer.CaptureMouse();
        }

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Released } && selectionCreate is not null)
        {
            selectionCreate = null;
            GraphContainer.ReleaseMouseCapture();
            shrinkWrapSelection();
        }

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Released } && groupDrag is not null)
        {
            groupDrag = null;
            GraphContainer.ReleaseMouseCapture();
        }

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Released } && selectionDrag is not null)
        {
            selectionDrag = null;
            GraphContainer.ReleaseMouseCapture();
        }

        if (e is { ChangedButton: GRAPH_SECONDARY_BUTTON, ButtonState: MouseButtonState.Released } && connectionDrag is not null)
        {
            var success = createNodeFromDrag(snappedMousePos);

            if (success)
            {
                e.Handled = true;
                endConnectionDrag();
                Graph.MarkDirty();
            }
        }
    }

    private void refreshContextMenu()
    {
        var contextMenu = GraphContainer.ContextMenu!;
        contextMenu.Items.Clear();
        contextMenu.Items.Add(GraphContextMenuBuilder.Items.Value);
    }

    #endregion

    #region GraphTransform

    private MatrixTransform graphTransform => (MatrixTransform)GraphContainer.RenderTransform!;

    private Point? graphDragMousePos;

    private void centerGraph()
    {
        var viewportWidth = GraphCanvas.ActualWidth;
        var viewportHeight = GraphCanvas.ActualHeight;

        var graphWidth = GraphContainer.ActualWidth;
        var graphHeight = GraphContainer.ActualHeight;

        var m = Matrix.Identity;
        m.Translate((viewportWidth - graphWidth) * 0.5, (viewportHeight - graphHeight) * 0.5);
        graphTransform.Matrix = m;
    }

    private void updateGraphDrag(MouseEventArgs e)
    {
        if (graphDragMousePos is null) return;

        var current = e.GetPosition(this);
        var delta = current - graphDragMousePos.Value;
        graphDragMousePos = current;

        var translation = Matrix.Identity;
        translation.Translate(delta.X, delta.Y);

        graphTransform.Matrix *= translation;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        var m = graphTransform.Matrix;

        var zoomFactor = Math.Pow(1.1, e.Delta / 120f);
        var oldScale = m.M11;
        var newScale = Math.Clamp(oldScale * zoomFactor, 0.05, 3.0);
        var k = newScale / oldScale;
        if (Math.Abs(k - 1.0) < 1e-6) return;

        var pivotLocal = m;
        pivotLocal.Invert();
        var pivotLocalPoint = pivotLocal.Transform(e.GetPosition(this));

        var z = Matrix.Identity;
        z.ScaleAt(k, k, pivotLocalPoint.X, pivotLocalPoint.Y);

        graphTransform.Matrix = z * graphTransform.Matrix;

        ShowDetails.Value = newScale >= 0.4d;
    }

    #endregion

    #region GridGraphElement

    private ElementOffset? draggingGridGraphElement;
    private GroupDrag? groupDrag;

    private void GridGraphElementContainer_OnMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        if (e is not { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Pressed }) return;

        var control = (FrameworkElement)sender!;
        var graphElementViewModel = (GridGraphElementViewModel)control.Tag!;
        control.CaptureMouse();
        e.Handled = true;

        var offset = e.GetPosition(GraphContainer) - graphElementViewModel.Position;
        draggingGridGraphElement = new ElementOffset(graphElementViewModel, new Point(offset.X, offset.Y));
    }

    private void GridGraphElementContainer_OnMouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Released } && connectionDrag is null)
        {
            var control = (FrameworkElement)sender!;
            control.ReleaseMouseCapture();
            e.Handled = true;
            draggingGridGraphElement = null;
            Graph.Serialise();
        }
    }

    private void updateGridGraphElementDrag(MouseEventArgs e)
    {
        if (draggingGridGraphElement is null) return;

        var newPos = e.GetPosition(GraphContainer) - draggingGridGraphElement.Offset;
        updateGridGraphElementPosition(draggingGridGraphElement.ViewModel, new Point(newPos.X, newPos.Y));
    }

    private void updateGridGraphElementPosition(GridGraphElementViewModel vm, Point position)
    {
        var x = position.X;
        var y = position.Y;

        x = double.Clamp(x, 0, GraphContainer.ActualWidth - vm.Control.ActualWidth);
        y = double.Clamp(y, 0, GraphContainer.ActualHeight - vm.Control.ActualHeight);

        x = snapToGrid(x + vm.SnapOffset.X) - vm.SnapOffset.X;
        y = snapToGrid(y + vm.SnapOffset.Y) - vm.SnapOffset.Y;

        vm.SetPosition(new Point(x, y));

        if (vm is NodeViewModel nodeVm)
            updateNodeViewModelConnections(nodeVm);
    }

    #endregion

    private NodeViewModel getNodeViewModel(INode node) => GraphElements.OfType<NodeViewModel>().Single(vm => vm.Node == node);

    private void updateNodeGroupGraphItem(GroupViewModel groupVm)
    {
        var nodeGraphItems = GraphElements.OfType<NodeViewModel>().Where(nodeGraphItem => groupVm.Group.Nodes.Contains(nodeGraphItem.Node.Id)).ToList();

        foreach (var nodeGraphItem in nodeGraphItems)
        {
            var index = GraphElements.IndexOf(nodeGraphItem);
            GraphElements.Move(index, GraphElements.Count - 1);
        }

        var topLeft = new Point(GraphContainer.ActualWidth, GraphContainer.ActualHeight);
        var bottomRight = new Point(0, 0);

        foreach (var nodeGraphItem in nodeGraphItems)
        {
            topLeft.X = Math.Min(topLeft.X, nodeGraphItem.Position.X - GroupPadding.Left);
            topLeft.Y = Math.Min(topLeft.Y, nodeGraphItem.Position.Y - GroupPadding.Top);
            bottomRight.X = Math.Max(bottomRight.X, nodeGraphItem.Position.X + nodeGraphItem.Control.ActualWidth + GroupPadding.Right);
            bottomRight.Y = Math.Max(bottomRight.Y, nodeGraphItem.Position.Y + nodeGraphItem.Control.ActualHeight + GroupPadding.Bottom);
        }

        var width = bottomRight.X - topLeft.X;
        var height = bottomRight.Y - topLeft.Y;

        width = Math.Max(width, 0);
        height = Math.Max(height, 0);

        groupVm.SetPosition(topLeft);
        groupVm.Width = width;
        groupVm.Height = height;
    }

    private void updateGroupDrag()
    {
        if (groupDrag is null) return;

        var mousePos = Mouse.GetPosition(GraphContainer);
        var currPos = new Point(groupDrag.GroupVm.Position.X, groupDrag.GroupVm.Position.Y) - groupDrag.OffsetFromGrid;
        var newPos = mousePos - groupDrag.Offset - groupDrag.OffsetFromGrid;
        var groupVm = groupDrag.GroupVm;

        newPos.X = snapToGrid(double.Clamp(newPos.X, 0, GraphContainer.ActualWidth - groupVm.Control.ActualWidth));
        newPos.Y = snapToGrid(double.Clamp(newPos.Y, 0, GraphContainer.ActualHeight - groupVm.Control.ActualHeight));

        var delta = new Vector(newPos.X - currPos.X, newPos.Y - currPos.Y);

        var positionChanged = double.Abs(delta.X) >= SNAP_DISTANCE || double.Abs(delta.Y) >= SNAP_DISTANCE;
        if (!positionChanged) return;

        foreach (var graphItem in groupDrag.Items)
        {
            graphItem.Position = new Point(graphItem.Position.X + delta.X, graphItem.Position.Y + delta.Y);

            if (graphItem is NodeViewModel nodeVm)
                updateNodeViewModelConnections(nodeVm);
        }

        groupVm.SetPosition(newPos + groupDrag.OffsetFromGrid);
    }

    #region Connections

    private Dictionary<Guid, ConnectionViewModel> getConnectionViewModels(IEnumerable<IConnection> connections)
    {
        var dict = new Dictionary<Guid, ConnectionViewModel>();

        foreach (var connection in connections)
        {
            var vm = GraphElements.OfType<ConnectionViewModel>().Single(vm => vm.Connection.Id == connection.Id);
            dict.Add(connection.Id, vm);
        }

        return dict;
    }

    private void updateNodeViewModelConnections(NodeViewModel nodeVm)
    {
        var nodeId = nodeVm.Node.Id;
        var connections = Graph.GetConnectionsForNode(nodeId).ToList();
        var connectionViewModels = getConnectionViewModels(connections);

        foreach (var connection in connections)
        {
            var connectionVm = connectionViewModels[connection.Id];
            updateConnectionOfNode(connectionVm, nodeVm);
            connectionVm.CreatePath();
        }
    }

    private void updateConnectionOfNode(ConnectionViewModel connectionVm, NodeViewModel nodeVm)
    {
        var connection = connectionVm.Connection;
        var nodeId = nodeVm.Node.Id;

        if (connection.OutputId == nodeId)
        {
            if (connection is IFlowConnection)
            {
                var control = nodeVm.FlowOutputControls[connection.OutputSlot][connection.OutputSlotIndex];
                var offset = new Point(control.ActualWidth / 2d, control.ActualHeight / 2d);
                connectionVm.StartPoint = control.TranslatePoint(offset, GraphContainer);
            }

            if (connection is IValueConnection)
            {
                var control = nodeVm.ValueOutputControls[connection.OutputSlot][connection.OutputSlotIndex];
                var offset = new Point(control.ActualWidth / 2d, control.ActualHeight / 2d);
                connectionVm.StartPoint = control.TranslatePoint(offset, GraphContainer);
            }
        }

        if (connection.InputId == nodeId)
        {
            if (connection is IFlowConnection)
            {
                var control = nodeVm.FlowInputControls[connection.InputSlot][connection.InputSlotIndex];
                var offset = new Point(control.ActualWidth / 2d, control.ActualHeight / 2d);
                connectionVm.EndPoint = control.TranslatePoint(offset, GraphContainer);
            }

            if (connection is IValueConnection)
            {
                var control = nodeVm.ValueInputControls[connection.InputSlot][connection.InputSlotIndex];
                var offset = new Point(control.ActualWidth / 2d, control.ActualHeight / 2d);
                connectionVm.EndPoint = control.TranslatePoint(offset, GraphContainer);
            }
        }
    }

    #endregion

    #region ConnectionDrag

    private ConnectionDrag? connectionDrag;

    private bool createNodeFromDrag(Vector2 position)
    {
        Debug.Assert(connectionDrag is not null);

        var element = connectionDrag.Element;
        var slotIndex = connectionDrag.SlotIndex;

        var isFlowInput = element.GetType().IsAssignableTo(typeof(IFlowInputBase));
        var isFlowOutput = element.GetType().IsAssignableTo(typeof(IFlowOutputBase));
        var isValueInput = element.GetType().IsAssignableTo(typeof(IValueInputBase));
        var isValueOutput = element.GetType().IsAssignableTo(typeof(IValueOutputBase));

        if (isFlowInput)
        {
            var nodeResult = Graph.AddNode(typeof(ButtonNode));
            if (!nodeResult.IsSuccess) return false;

            var node = nodeResult.Value;
            node.Metadata.Position = position;

            var connectionResult = Graph.CreateConnection(node.Metadata.ElementInstancesFor(ConnectionPoint.FlowOutput)[0], 0, element, slotIndex);
            if (!connectionResult.IsSuccess) return false;

            return true;
        }

        if (isValueInput && NodeConstants.INPUT_TYPES.Any(type => element.Metadata.Shared.ValueType.IsAssignableTo(type)))
        {
            var nodeType = typeof(ValueNode<>).MakeGenericType(element.Metadata.Shared.ValueType);
            var nodeResult = Graph.AddNode(nodeType);
            if (!nodeResult.IsSuccess) return false;

            var node = nodeResult.Value;
            node.Metadata.Position = position;

            if (!element.Metadata.Shared.IsList)
                nodeType.GetProperty(nameof(ValueNode<>.Value))!.SetValue(node, ((IValueInput)element).GetField());

            var connectionResult = Graph.CreateConnection(node.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput)[0], 0, element, slotIndex);
            if (!connectionResult.IsSuccess) return false;

            return true;
        }

        if (isValueOutput)
        {
            var nodeResult = Graph.AddNode(typeof(DisplayNode<>).MakeGenericType(element.Metadata.Shared.ValueType));
            if (!nodeResult.IsSuccess) return false;

            var node = nodeResult.Value;
            node.Metadata.Position = position;

            var connectionResult = Graph.CreateConnection(element, slotIndex, node.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[0], 0);
            if (!connectionResult.IsSuccess) return false;

            return true;
        }

        return false;
    }

    private void startConnectionDrag(ConnectionPointViewModel vm, FrameworkElement source)
    {
        Debug.Assert(connectionDrag is null);

        var element = vm.Element;
        var node = element.Owner;

        if (element is IValueInputBase)
        {
            var existingConnection = Graph.Connections.FirstOrDefault(c => c is IValueConnection && c.InputId == node.Id && c.InputSlot == element.Metadata.Shared.Slot && c.InputSlotIndex == vm.Index);

            if (existingConnection is not null)
            {
                var outputNodeVm = GraphElements.OfType<NodeViewModel>().Single(nodeVm => nodeVm.Node.Id == existingConnection.OutputId);
                var outputNode = outputNodeVm.Node;
                var outputElement = outputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput)[existingConnection.OutputSlot];

                Graph.RemoveConnection(existingConnection);
                Graph.MarkDirty();

                connectionDrag = new ConnectionDrag(outputNodeVm.ValueOutputControls[existingConnection.OutputSlot][existingConnection.OutputSlotIndex], outputElement, existingConnection.OutputSlotIndex);
            }
        }

        if (element is IFlowInputBase)
        {
            var existingConnection = Graph.Connections.FirstOrDefault(c => c is IFlowConnection && c.InputId == node.Id && c.InputSlot == element.Metadata.Shared.Slot && c.InputSlotIndex == vm.Index);

            if (existingConnection is not null)
            {
                var outputNodeVm = GraphElements.OfType<NodeViewModel>().Single(nodeVm => nodeVm.Node.Id == existingConnection.OutputId);
                var outputNode = outputNodeVm.Node;
                var outputElement = outputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowOutput)[existingConnection.OutputSlot];

                Graph.RemoveConnection(existingConnection);
                Graph.MarkDirty();

                connectionDrag = new ConnectionDrag(outputNodeVm.FlowOutputControls[existingConnection.OutputSlot][existingConnection.OutputSlotIndex], outputElement, existingConnection.OutputSlotIndex);
            }
        }

        connectionDrag ??= new ConnectionDrag(source, vm.Element, vm.Index);
        ConnectionDragPath.Visibility = Visibility.Visible;
        updateConnectionDrag();
    }

    private void endConnectionDrag()
    {
        ConnectionDragPath.Visibility = Visibility.Collapsed;
        connectionDrag = null;
    }

    private void Connection_OnMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (connectionDrag is not null) return;
        if (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed) return;

        var control = (FrameworkElement)sender!;
        var vm = (ConnectionPointViewModel)control.Tag!;

        startConnectionDrag(vm, control);
        Mouse.Capture(null);
        e.Handled = true;
    }

    private void Connection_OnMouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (connectionDrag is null) return;
        if (e.ChangedButton != GRAPH_INTERACT_BUTTON || e.ButtonState != MouseButtonState.Released) return;

        var control = (FrameworkElement)sender!;
        var connectionViewModel = (ConnectionPointViewModel)control.Tag!;
        e.Handled = true;

        var originElement = connectionDrag.Element;
        var originSlotIndex = connectionDrag.SlotIndex;

        INodeElement outputElement;
        int outputSlotIndex;

        INodeElement inputElement;
        int inputSlotIndex;

        if (originElement is IValueOutputBase or IFlowOutputBase)
        {
            outputElement = originElement;
            outputSlotIndex = originSlotIndex;
            inputElement = connectionViewModel.Element;
            inputSlotIndex = connectionViewModel.Index;
        }
        else
        {
            outputElement = connectionViewModel.Element;
            outputSlotIndex = connectionViewModel.Index;
            inputElement = originElement;
            inputSlotIndex = originSlotIndex;
        }

        var result = Graph.CreateConnection(outputElement, outputSlotIndex, inputElement, inputSlotIndex);

        if (!result.IsSuccess)
            Logger.Error(result.Exception, nameof(Connection_OnMouseUp));
        else
            Graph.MarkDirty();

        endConnectionDrag();
    }

    private void updateConnectionDrag()
    {
        if (connectionDrag is null) return;

        var element = connectionDrag.Control;
        var offset = new Point(connectionDrag.Control.ActualWidth / 2d, connectionDrag.Control.ActualHeight / 2d);
        var isReversed = connectionDrag.Element is IFlowInputBase or IValueInputBase;

        var startPoint = element.TranslatePoint(offset, GraphContainer);
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

    #endregion

    private void GraphContextMenu_NodeEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender!;
        var entry = (ContextMenuNodeEntry)control.Tag!;

        var nodeTypeMetadata = NodeTypeManager.Data[entry.TypeLookup];

        if (nodeTypeMetadata.LinkedTypes.Any(t => t.IsGenericType) && entry.FilteredGeneric is null)
        {
            var window = new NodeCreatorWindow(nodeTypeMetadata);
            nodeCreatorWindowManager.TrySpawnChild(window);

            window.Closed += (_, _) =>
            {
                if (window.ConstructedType is null) return;

                var result = Graph.AddNode(window.ConstructedType, position: new Vector2(snapToGrid((float)lastGraphPointerPos.X), snapToGrid((float)lastGraphPointerPos.Y)));

                if (!result.IsSuccess)
                    Logger.Error(result.Exception, nameof(GraphContextMenu_NodeEntry_OnClick));

                Graph.MarkDirty();
            };

            window.Show();
        }
        else
        {
            if (entry.FilteredGeneric is not null)
            {
                var type = nodeTypeMetadata.Shared.Type.MakeGenericType(entry.FilteredGeneric);
                var result = Graph.AddNode(type, position: new Vector2(snapToGrid((float)lastGraphPointerPos.X), snapToGrid((float)lastGraphPointerPos.Y)));

                if (!result.IsSuccess)
                    Logger.Error(result.Exception, nameof(GraphContextMenu_NodeEntry_OnClick));
                else
                {
                    Graph.MarkDirty();
                }
            }
            else
            {
                var result = Graph.AddNode(nodeTypeMetadata.Shared.Type, position: new Vector2(snapToGrid((float)lastGraphPointerPos.X), snapToGrid((float)lastGraphPointerPos.Y)));

                if (!result.IsSuccess)
                    Logger.Error(result.Exception, nameof(GraphContextMenu_NodeEntry_OnClick));
                else
                {
                    Graph.MarkDirty();
                }
            }
        }
    }

    private void ListElementAdd_OnClick(object sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender!;
        var vm = (ConnectionPointListViewModel)control.Tag!;
        var element = vm.Element;
        var currentSize = element.Metadata.Size;

        e.Handled = true;

        element.Metadata.Size = ++currentSize;
        vm.AddItem();

        var nodeVm = getNodeViewModel(element.Owner);

        Dispatcher.Invoke(() =>
        {
            populateNodeViewModel(nodeVm);
            updateNodeViewModelConnections(nodeVm);
        }, DispatcherPriority.Render);

        _ = Graph.TriggerTree(vm.Element.Owner);
        _ = Graph.MarkDirty();
    }

    private void ListElementRemove_OnClick(object sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender!;
        var vm = (ConnectionPointListViewModel)control.Tag!;
        var element = vm.Element;
        var currentSize = element.Metadata.Size;

        e.Handled = true;

        if (currentSize - 1 == 0) return;

        var connection = Graph.Connections.SingleOrDefault(c => c.OutputId == element.Owner.Id && c.OutputSlot == element.Metadata.Shared.Slot && c.OutputSlotIndex == currentSize - 1
                                                                || c.InputId == element.Owner.Id && c.InputSlot == element.Metadata.Shared.Slot && c.InputSlotIndex == currentSize - 1);

        if (connection is not null)
            Graph.RemoveConnection(connection);

        element.Metadata.Size = --currentSize;
        vm.RemoveItem();

        var nodeVm = getNodeViewModel(element.Owner);

        Dispatcher.Invoke(() =>
        {
            populateNodeViewModel(nodeVm);
            updateNodeViewModelConnections(nodeVm);
        }, DispatcherPriority.Render);

        _ = Graph.TriggerTree(vm.Element.Owner);
        _ = Graph.MarkDirty();
    }

    private void CallNode_OnClick(object sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender!;
        var vm = (NodeViewModel)control.Tag!;

        e.Handled = true;
        _ = Graph.TriggerTree(vm.Node);
    }

    private void NodeContextMenu_DeleteClick(object? sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender!;
        var vm = (NodeViewModel)control.Tag!;

        e.Handled = true;
        Graph.RemoveNode(vm.Node.Id);
        Graph.MarkDirty();
    }

    private void updateSelectionCreate()
    {
        if (selectionCreate is null) return;

        var mousePos = Mouse.GetPosition(GraphContainer);

        var posX = double.Min(mousePos.X, selectionCreate.Point.X);
        var posY = double.Min(mousePos.Y, selectionCreate.Point.Y);

        var width = double.Abs(mousePos.X - selectionCreate.Point.X);
        var height = double.Abs(mousePos.Y - selectionCreate.Point.Y);

        SelectionVisual.Width = width;
        SelectionVisual.Height = height;

        var selectionTransform = (TranslateTransform)SelectionVisual.RenderTransform;
        selectionTransform.X = posX;
        selectionTransform.Y = posY;
    }

    private void updateSelectionDrag()
    {
        if (selectionDrag is null) return;

        Debug.Assert(elementsSelection is not null);

        var mousePos = Mouse.GetPosition(GraphContainer);
        var transform = (TranslateTransform)SelectionVisual.RenderTransform;
        var currPos = new Point(transform.X, transform.Y);
        var newPos = mousePos - selectionDrag.Offset;

        newPos.X = snapToGrid(double.Clamp(newPos.X, 0, GraphContainer.ActualWidth - SelectionVisual.ActualWidth));
        newPos.Y = snapToGrid(double.Clamp(newPos.Y, 0, GraphContainer.ActualHeight - SelectionVisual.ActualHeight));

        var delta = new Point(newPos.X - currPos.X, newPos.Y - currPos.Y);

        var positionChanged = double.Abs(delta.X) >= SNAP_DISTANCE || double.Abs(delta.Y) >= SNAP_DISTANCE;
        if (!positionChanged) return;

        transform.X = newPos.X;
        transform.Y = newPos.Y;

        foreach (var item in elementsSelection.Items)
        {
            updateGridGraphElementPosition(item, new Point(item.Position.X + delta.X, item.Position.Y + delta.Y));
            if (item is NodeViewModel nodeVm) updateNodeViewModelConnections(nodeVm);
        }

        foreach (var groupVm in GraphElements.OfType<GroupViewModel>().ToList())
        {
            updateNodeGroupGraphItem(groupVm);
        }
    }

    private void deselectGraphItems()
    {
        elementsSelection = null;
        SelectionVisual.Visibility = Visibility.Collapsed;
    }

    private void shrinkWrapSelection()
    {
        var bounds = new Rect(0, 0, SelectionVisual.ActualWidth, SelectionVisual.ActualHeight);

        var topLeft = new Point(GraphContainer.ActualWidth, GraphContainer.ActualHeight);
        var bottomRight = new Point(0, 0);

        var elements = new List<GridGraphElementViewModel>();

        foreach (var graphItem in GraphElements.OfType<GridGraphElementViewModel>())
        {
            var element = graphItem.Control;
            var startPoint = element.TranslatePoint(new Point(0, 0), SelectionVisual);
            var endPoint = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), SelectionVisual);

            var nodeContainerPosition = (TranslateTransform)element.RenderTransform;

            if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
            {
                topLeft.X = Math.Min(topLeft.X, nodeContainerPosition.X);
                topLeft.Y = Math.Min(topLeft.Y, nodeContainerPosition.Y);
                bottomRight.X = Math.Max(bottomRight.X, nodeContainerPosition.X + element.ActualWidth);
                bottomRight.Y = Math.Max(bottomRight.Y, nodeContainerPosition.Y + element.ActualHeight);
                elements.Add(graphItem);
            }
        }

        if (elements.Count == 0)
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

        elementsSelection = new ElementsSelection(elements.ToArray());
        SelectionVisual.Visibility = Visibility.Visible;
    }

    private void GroupContainer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            selectionCreate = new SelectionCreate(mousePos);
            SelectionVisual.Visibility = Visibility.Visible;
            GraphContainer.CaptureMouse();
            return;
        }

        var element = (FrameworkElement)sender;
        var groupVm = (GroupViewModel)element.Tag;

        if (e.ChangedButton == GRAPH_INTERACT_BUTTON)
        {
            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            var groupPos = groupVm.Position;
            var offset = mousePos - groupPos;

            var nodeVms = GraphElements.OfType<NodeViewModel>()
                                       .Where(nodeGraphItem => groupVm.Group.Nodes.Contains(nodeGraphItem.Node.Id))
                                       .ToList();

            var connections = nodeVms.SelectMany(nodeVm => Graph.Connections.Where(c => c.InputId == nodeVm.Node.Id || c.OutputId == nodeVm.Node.Id))
                                     .Distinct();

            var offsetFromGrid = new Vector(groupPos.X % SNAP_DISTANCE, groupPos.Y % SNAP_DISTANCE);

            var groupGraphItemIndex = GraphElements.IndexOf(groupVm);
            GraphElements.Move(groupGraphItemIndex, GraphElements.Count - 1);

            foreach (var nodeVm in nodeVms)
            {
                var index = GraphElements.IndexOf(nodeVm);
                GraphElements.Move(index, GraphElements.Count - 1);
            }

            groupDrag = new GroupDrag(offset, offsetFromGrid, groupVm, nodeVms, connections);
            GraphContainer.CaptureMouse();

            deselectGraphItems();
        }
    }

    private void SelectionVisual_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var position = (TranslateTransform)element.RenderTransform;

        if (e.ChangedButton == GRAPH_INTERACT_BUTTON && e.ButtonState == MouseButtonState.Pressed)
        {
            e.Handled = true;

            var mousePos = Mouse.GetPosition(GraphContainer);
            var selectionPos = new Point(position.X, position.Y);
            var offset = mousePos - selectionPos;

            selectionDrag = new SelectionDrag(offset);
            GraphContainer.CaptureMouse();
        }
    }

    private void VariableAdd_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new VariableCreatorWindow(Graph);

        window.Closed += (_, _) =>
        {
            if (window.VariableType is null) return;

            Graph.CreateVariable(window.VariableType, window.VariableName, window.VariablePersistent);
            Graph.MarkDirty();
        };

        variableCreatorWindowManager.TrySpawnChild(window);
    }

    private void addVariableNode(Type type, IGraphVariable graphVariable)
    {
        var window = Window.GetWindow(this)!;
        var offset = TranslatePoint(new Point(window.ActualWidth / 2d, window.ActualHeight / 2d), GraphContainer);
        var nodeResult = Graph.AddNode(type, position: new Vector2((float)offset.X, (float)offset.Y));
        Debug.Assert(nodeResult.IsSuccess);
        var variableReference = (IHasVariableReference)nodeResult.Value;
        variableReference.VariableId = graphVariable.GetId();
        Graph.MarkDirty();
    }

    private void CreateVariableSource_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var item = (MenuItem)sender;
        var graphVariable = (IGraphVariable)item.Tag;
        addVariableNode(typeof(VariableSourceNode<>).MakeGenericType(graphVariable.GetValueType()), graphVariable);
    }

    private void CreateVariableReference_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var item = (MenuItem)sender;
        var graphVariable = (IGraphVariable)item.Tag;
        addVariableNode(typeof(VariableReferenceNode<>).MakeGenericType(graphVariable.GetValueType()), graphVariable);
    }

    private void CreateVariableWrite_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var item = (MenuItem)sender;
        var graphVariable = (IGraphVariable)item.Tag;
        addVariableNode(typeof(DirectWriteVariableNode<>).MakeGenericType(graphVariable.GetValueType()), graphVariable);
    }

    private void CreateVariableDrive_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var item = (MenuItem)sender;
        var graphVariable = (IGraphVariable)item.Tag;
        addVariableNode(typeof(DriveVariableNode<>).MakeGenericType(graphVariable.GetValueType()), graphVariable);
    }

    private void DeleteVariable_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var item = (MenuItem)sender;
        var graphVariable = (IGraphVariable)item.Tag;

        var result = MessageBox.Show(Window.GetWindow(this),
            "Are you sure you want to delete this variable?\n\nThis will remove all nodes that reference this variable",
            "Variable Delete Warning",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        Graph.DeleteVariable(graphVariable);
        Graph.MarkDirty();
    }

    private void GroupContextMenu_DissolveClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var groupVm = (GroupViewModel)element.Tag;

        Graph.DeleteGroup(groupVm.Group.Id);
        Graph.MarkDirty();
    }

    private void GroupContextMenu_DeleteClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var groupVm = (GroupViewModel)element.Tag;

        foreach (var node in groupVm.Group.Nodes.ToList())
        {
            Graph.RemoveNode(node);
        }

        Graph.DeleteGroup(groupVm.Group.Id);
        Graph.MarkDirty();
    }

    private void SelectionContextMenu_CreateGroupClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(elementsSelection is not null);

        Graph.AddGroup(elementsSelection.Items.OfType<NodeViewModel>().Select(item => item.Node.Id));
        Graph.MarkDirty();
        deselectGraphItems();
    }

    private void SelectionContextMenu_SaveAsPresetClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(elementsSelection is not null);

        var selectedNodes = elementsSelection.Items.OfType<NodeViewModel>().Select(item => item.Node.Id).ToList();
        var position = (TranslateTransform)SelectionVisual.RenderTransform;

        var presetCreatorWindow = new PresetCreatorWindow();

        presetCreatorWindow.Closed += (_, _) =>
        {
            if (string.IsNullOrEmpty(presetCreatorWindow.PresetName)) return;

            Graph.CreatePreset(presetCreatorWindow.PresetName, selectedNodes, (float)position.X, (float)position.Y);
        };

        presetCreatorWindowManager.TrySpawnChild(presetCreatorWindow);
        deselectGraphItems();
    }

    private void SelectionContextMenu_DeleteAllClick(object sender, RoutedEventArgs e)
    {
        deleteSelection().Forget();
    }

    private async Task deleteSelection()
    {
        Debug.Assert(elementsSelection is not null);

        var groupsToUpdate = new List<GroupViewModel>();

        foreach (var item in elementsSelection.Items)
        {
            if (item is NodeViewModel nodeVm)
            {
                var groupItem = GraphElements.OfType<GroupViewModel>().SingleOrDefault(groupItem => groupItem.Group.Nodes.Contains(nodeVm.Node.Id));
                if (groupItem is not null && !groupsToUpdate.Contains(groupItem)) groupsToUpdate.Add(groupItem);

                Graph.RemoveNode(nodeVm.Node.Id);
            }
        }

        await Graph.MarkDirtyAsync();

        foreach (var nodeGroupGraphItem in groupsToUpdate)
        {
            updateNodeGroupGraphItem(nodeGroupGraphItem);
        }

        deselectGraphItems();
    }

    private void OnGraphTitleEditCompleted(object sender, RoutedEventArgs e)
    {
        Graph.Serialise();
    }

    private void OnGroupTitleEditCompleted(object sender, RoutedEventArgs e)
    {
        Graph.Serialise();
    }
}