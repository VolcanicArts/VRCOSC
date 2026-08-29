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
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ColorPicker;
using CommunityToolkit.Mvvm.Input;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes.ViewModels;
using VRCOSC.App.UI.Windows.Nodes;
using VRCOSC.App.Utils;
using Xceed.Wpf.AvalonDock.Controls;
using Color = VRCOSC.App.Utils.Color;
using Expression = org.mariuszgromada.math.mxparser.Expression;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
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
    public ObservableCollection<IGraphVariable> GraphVariablesSource { get; } = [];

    private bool hasLoaded;

    private WindowManager nodeCreatorWindowManager = null!;
    private WindowManager variableCreatorWindowManager = null!;
    private WindowManager presetCreatorWindowManager = null!;
    private WindowManager advancedNodeCreatorWindowManager = null!;

    public Observable<bool> ShowDetails { get; } = new(true);

    private SelectionCreate? selectionCreate;
    private SelectionDrag? selectionDrag;
    private ElementsSelection? elementsSelection;

    public ICommand OpenAdvancedNodeCreatorWindowCommand { get; }

    public NodeGraphView(NodeGraph graph)
    {
        InitializeComponent();
        Graph = graph;
        Graph.OnMarkedDirty += onGraphMarkedDirty;
        Loaded += OnLoaded;
        OpenAdvancedNodeCreatorWindowCommand = new RelayCommand(openAdvancedNodeCreatorWindow);
        DataContext = this;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (hasLoaded) return;

        nodeCreatorWindowManager = new WindowManager(this);
        variableCreatorWindowManager = new WindowManager(this);
        presetCreatorWindowManager = new WindowManager(this);
        advancedNodeCreatorWindowManager = new WindowManager(this);
        refreshContextMenu();
        centerGraph();
        Task.Run(Graph.MarkDirty);
    }

    private void openAdvancedNodeCreatorWindow()
    {
        var window = new AdvancedNodeCreatorWindow();
        advancedNodeCreatorWindowManager.TrySpawnChild(window);

        window.Closed += (_, _) =>
        {
            if (window.ConstructedType is null) return;

            var centerPoint = getCenterPoint();
            var result = Graph.AddNode(window.ConstructedType, position: new Vector2(snapToGrid((float)centerPoint.X), snapToGrid((float)centerPoint.Y)));

            if (!result.IsSuccess)
                Logger.Error(result.Exception, nameof(GraphContextMenu_NodeEntry_OnClick));

            Graph.MarkDirty();
        };
    }

    #region Util

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T snapToGrid<T>(T value) where T : IFloatingPointIeee754<T>
    {
        var snapDistance = T.CreateChecked(SNAP_DISTANCE);
        var offset = T.CreateChecked(0.5d);
        return T.Round((value + offset) / snapDistance) * snapDistance - offset;
    }

    #endregion

    #region Records

    private record ElementOffset(GridGraphElementViewModel ViewModel, Point Offset);

    private record ConnectionDrag(FrameworkElement Control, INodeElement Element, int SlotIndex);

    private record SelectionCreate(Point Point);

    private record SelectionDrag(Vector Offset, Vector OffsetFromGrid);

    private record ElementsSelection(GridGraphElementViewModel[] Items);

    private record GroupDrag(Vector Offset, Vector OffsetFromGrid, GroupViewModel GroupVm, IEnumerable<GridGraphElementViewModel> Items);

    #endregion

    #region Marked Dirty

    private async Task onGraphMarkedDirty(GraphChanges changes)
    {
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (changes.RemovedNodes.Count != 0)
                GraphElements.RemoveIf(item => item is NodeViewModel vm && changes.RemovedNodes.Contains(vm.Node));

            if (changes.RemovedConnections.Count != 0)
                GraphElements.RemoveIf(item => item is ConnectionViewModel vm && changes.RemovedConnections.Contains(vm.Connection));

            if (changes.RemovedGroups.Count != 0)
                GraphElements.RemoveIf(item => item is GroupViewModel vm && changes.RemovedGroups.Contains(vm.Group));

            if (changes.RemovedComments.Count != 0)
                GraphElements.RemoveIf(item => item is CommentViewModel vm && changes.RemovedComments.Contains(vm.Comment));

            Logger.Log($"Finished removing elements in {stopwatch.Elapsed}", LoggingTarget.Information);
            stopwatch.Restart();

            var addedNodes = new List<NodeViewModel>();
            var addedConnections = new List<ConnectionViewModel>();
            var addedGroups = new List<GroupViewModel>();
            var addedComments = new List<CommentViewModel>();

            var offset = GraphElements.Count;

            await Dispatcher.InvokeAsync(() =>
            {
                GraphVariablesSource.Clear();
                GraphVariablesSource.AddRange(Graph.GraphVariables.Values.OrderBy(v => v.GetName()).ThenBy(v => v.GetValueType().GetFriendlyName()));

                addedNodes.AddRange(changes.AddedNodes.Select(node =>
                {
                    if (node is IDisplayNode) return new DisplayNodeBaseViewModel(node);

                    return new NodeViewModel(node);
                }).ToList());
                addedConnections.AddRange(changes.AddedConnections.Select(connection => new ConnectionViewModel(connection)).ToList());
                addedComments.AddRange(changes.AddedComments.Select(comment => new CommentViewModel(comment)).ToList());
                addedGroups.AddRange(changes.AddedGroups.Select(group => new GroupViewModel(group)).ToList());

                GraphElements.AddRange(addedNodes);
                GraphElements.AddRange(addedConnections);
                GraphElements.AddRange(addedComments);
                GraphElements.AddRange(addedGroups);

                Logger.Log($"Finished adding elements in {stopwatch.Elapsed}", LoggingTarget.Information);
                stopwatch.Restart();
            });

            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);

            Logger.Log($"Finished wait for load in {stopwatch.Elapsed}", LoggingTarget.Information);
            stopwatch.Restart();

            await Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < addedNodes.Count; i++)
                {
                    var nodeVm = addedNodes[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var nodeContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    if (nodeVm is DisplayNodeBaseViewModel dnbvm) dnbvm.Start();

                    nodeVm.Control = nodeContainer;
                    populateNodeViewModel(nodeVm);
                    updateGridGraphElementPosition(nodeVm, nodeVm.Position);
                }

                Logger.Log($"Finished populating nodes in {stopwatch.Elapsed}", LoggingTarget.Information);
                stopwatch.Restart();

                offset += addedNodes.Count;

                for (var i = 0; i < addedConnections.Count; i++)
                {
                    var connectionVm = addedConnections[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var connectionContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    connectionVm.Control = connectionContainer;
                    connectionVm.ZIndex = -10;
                    updateConnectionViewModelPoints(connectionVm);
                    connectionVm.IsVisible = true;
                }

                Logger.Log($"Finished populating connections in {stopwatch.Elapsed}", LoggingTarget.Information);
                stopwatch.Restart();

                offset += addedConnections.Count;

                for (var i = 0; i < addedComments.Count; i++)
                {
                    var commentVm = addedComments[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var commentContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    commentVm.Control = commentContainer;
                    updateCommentSnap(commentVm);
                }

                Logger.Log($"Finished populating comments in {stopwatch.Elapsed}", LoggingTarget.Information);
                stopwatch.Restart();

                offset += addedComments.Count;

                for (var i = 0; i < addedGroups.Count; i++)
                {
                    var groupVm = addedGroups[i];
                    var itemContainer = (FrameworkElement)GraphElementItemsControl.ItemContainerGenerator.ContainerFromIndex(offset + i);
                    var groupContainer = (FrameworkElement)VisualTreeHelper.GetChild(itemContainer, 0);

                    groupVm.Control = groupContainer;
                    updateGroupViewModel(groupVm, false);
                }

                foreach (var groupVm in addedGroups)
                {
                    updateGroupViewModel(groupVm);
                }

                Logger.Log($"Finished populating groups in {stopwatch.Elapsed}", LoggingTarget.Information);
                stopwatch.Restart();
            }, DispatcherPriority.Background);

            if (!hasLoaded)
            {
                Dispatcher.Invoke(() =>
                {
                    hasLoaded = true;
                    LoadingOverlay.FadeOut(250);
                    GraphContainer.Focus();
                });

                NodeManager.GetInstance().SlowUpdateThread(false);
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
                var slotElement = searchList.SingleOrDefault(c => c.Name == control_name && c.Tag?.GetType() == viewModelType && ((ConnectionPointViewModel)c.Tag).Element.Metadata.Shared.Slot == slot);

                // ValueNode doesn't need a connection point for collapsed inputs
                if (slotElement is null) continue;

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

    private static FrameworkElement getSnappingControl(NodeViewModel vm)
    {
        var metadata = vm.Node.Metadata.Shared;

        if (metadata.IsFlowOutput) return vm.FlowOutputControls[0][0];
        if (metadata.IsFlowInput) return vm.FlowInputControls[0][0];

        if (metadata is { IsCollapsed: true, IsValueInput: true, IsValueOutput: true })
        {
            var outputIsOdd = metadata.ValueOutputCount % 2 == 1;
            var inputIsOdd = metadata.ValueInputCount % 2 == 1;

            if (outputIsOdd) return vm.ValueOutputControls[(int)MathF.Round(metadata.ValueOutputCount / 2f)][0];
            if (inputIsOdd) return vm.ValueInputControls[(int)MathF.Round(metadata.ValueInputCount / 2f)][0];
        }

        if (metadata.IsValueOutput) return vm.ValueOutputControls[0][0];
        if (metadata.IsValueInput) return vm.ValueInputControls[0][0];

        throw new Exception($"Unable to get snapping control for node {vm.Node}");
    }

    #endregion

    #region GraphControl

    private Point lastGraphPointerPos;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!hasLoaded) return;

        if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
        {
            executeCopy();
            e.Handled = true;
            return;
        }

        if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
        {
            executePaste().Forget();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Space)
        {
            centerGraph();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete)
        {
            if (elementsSelection is not null)
            {
                deleteSelection().Forget();
                e.Handled = true;
            }

            return;
        }
    }

    private NodePreset? copyPasteHolder;

    private void executeCopy()
    {
        if (elementsSelection is null) return;

        var position = (TranslateTransform)SelectionVisual.RenderTransform;

        var nodeVms = elementsSelection.Items.OfType<NodeViewModel>().ToList();
        var nodeIds = nodeVms.Select(vm => vm.Node.Id).ToList();

        copyPasteHolder = new NodePreset
        {
            Structure =
            {
                Nodes = nodeVms.Select(nodeVm => new SerialisableNode((Node)nodeVm.Node)).ToList(),
                Connections = Graph.Connections.Where(c => nodeIds.Contains(c.OutputId) && nodeIds.Contains(c.InputId)).Select(c => new SerialisableConnection(c)).ToList(),
                Groups = Graph.Groups.Values.Where(g => g.Nodes.All(nodeId => nodeVms.Select(item => item.Node.Id).Contains(nodeId))).Select(group => new SerialisableGroup(group)).ToList(),
                Comments = elementsSelection.Items.OfType<CommentViewModel>().Select(commentVm => new SerialisableComment(commentVm.Comment)).ToList()
            }
        };

        foreach (var node in copyPasteHolder.Structure.Nodes)
        {
            node.Position = new Vector2(node.Position.X - (float)position.X, node.Position.Y - (float)position.Y);
        }

        foreach (var comment in copyPasteHolder.Structure.Comments)
        {
            comment.Position = new Vector2(comment.Position.X - (float)position.X, comment.Position.Y - (float)position.Y);
        }
    }

    private async Task executePaste()
    {
        if (copyPasteHolder is null) return;

        var offset = getSnappedMousePos();
        Logger.Log($"Pasting at {offset}", LoggingTarget.Information);
        var newElements = copyPasteHolder.SpawnTo(Graph, offset);
        await Graph.MarkDirtyAsync();
        shrinkWrapSelection(newElements);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (!hasLoaded) return;

        lastGraphPointerPos = e.GetPosition(GraphContainer);
        if (e.Handled) return;

        // StandardColorPicker doesn't block events. We'll do it manually
        if (IsMouseCapturedByDescendantOf<PickerControlBase>()) return;

        if (!GraphContainer.IsFocused)
        {
            GraphContainer.Focus();
            Graph.Serialise();
        }

        handleMouseUpdates(e);
    }

    public static DependencyObject? GetParent(DependencyObject obj) =>
        obj is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(obj) : LogicalTreeHelper.GetParent(obj);

    public static bool IsMouseCapturedByDescendantOf<T>() where T : DependencyObject
    {
        DependencyObject? current = null;

        if (Mouse.Captured is not DependencyObject)
            return false;

        current = (DependencyObject)Mouse.Captured;

        while (current != null)
        {
            if (current is T)
                return true;

            current = GetParent(current);
        }

        return false;
    }

    private Point getCenterPoint()
    {
        var container = this.FindVisualParent<Grid>("NodeViewContainer")!;
        var snappedCenter = new Point(snapToGrid(container.ActualWidth / 2d), snapToGrid(container.ActualHeight / 2d));
        var windowToGraph = container.TransformToDescendant(GraphContainer);
        return windowToGraph.Transform(snappedCenter);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!hasLoaded) return;

        updateGridGraphElementDrag(e);
        updateGraphDrag(e);
        updateConnectionDrag();
        updateGroupDrag();
        updateSelectionCreate();
        updateSelectionDrag();
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (!hasLoaded) return;

        handleMouseUpdates(e);
    }

    private void handleMouseUpdates(MouseButtonEventArgs e)
    {
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
            Graph.Serialise();
        }

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Released } && selectionDrag is not null)
        {
            selectionDrag = null;
            GraphContainer.ReleaseMouseCapture();
            Graph.Serialise();
        }

        if (e is { ChangedButton: GRAPH_SECONDARY_BUTTON, ButtonState: MouseButtonState.Released } && connectionDrag is not null)
        {
            var success = createNodeFromDrag();

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

        var addComment = new MenuItem
        {
            Header = "Add Comment"
        };

        addComment.Click += AddComment_OnClick;

        contextMenu.Items.Add(addComment);
    }

    private void AddComment_OnClick(object sender, RoutedEventArgs e)
    {
        var comment = Graph.AddComment();
        comment.Position.Value = getSnappedMousePos();
        Graph.MarkDirty();
    }

    private void updateCommentSnap(CommentViewModel vm)
    {
        vm.SnapOffset = new Point(vm.Control.ActualWidth / 2d, 0d);
        vm.SetPosition(new Point(snapToGrid(vm.Position.X + vm.SnapOffset.X) - vm.SnapOffset.X, snapToGrid(vm.Position.Y + vm.SnapOffset.Y) - vm.SnapOffset.Y));
    }

    #endregion

    #region GraphTransform

    private Vector2 getSnappedMousePos()
    {
        var mousePos = Mouse.GetPosition(GraphContainer);
        return new Vector2((float)snapToGrid(mousePos.X), (float)snapToGrid(mousePos.Y));
    }

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
        m.Scale(1d, 1d);
        ShowDetails.Value = true;
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
        GraphContainer.Focus();

        var m = graphTransform.Matrix;

        var zoomFactor = double.Pow(1.1d, e.Delta / 120d);
        var oldScale = m.M11;
        var newScale = double.Clamp(oldScale * zoomFactor, 0.025d, 5.0d);
        var k = newScale / oldScale;
        if (double.Abs(k - 1.0d) < 1e-6d) return;

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

        // StandardColorPicker doesn't block events. We'll do it manually
        if (IsMouseCapturedByDescendantOf<PickerControlBase>()) return;

        if (e is not { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Pressed }) return;

        var control = (FrameworkElement)sender!;
        var graphElementViewModel = (GridGraphElementViewModel)control.Tag!;
        control.CaptureMouse();
        e.Handled = true;

        var index = GraphElements.IndexOf(graphElementViewModel);
        GraphElements.Move(index, GraphElements.Count - 1);

        var offset = e.GetPosition(GraphContainer) - graphElementViewModel.Position;
        draggingGridGraphElement = new ElementOffset(graphElementViewModel, new Point(offset.X, offset.Y));
    }

    private void GridGraphElementContainer_OnMouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        if (e is { ChangedButton: GRAPH_INTERACT_BUTTON, ButtonState: MouseButtonState.Released } && connectionDrag is null)
        {
            var control = (FrameworkElement)sender!;
            checkForGroupAdditions();
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

        if (draggingGridGraphElement.ViewModel is NodeViewModel nodeVm)
            updateGroupOfNode(nodeVm);

        if (draggingGridGraphElement.ViewModel is CommentViewModel commentVm)
            updateGroupOfComment(commentVm);
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

    private void checkForGroupAdditions()
    {
        if (draggingGridGraphElement?.ViewModel is NodeViewModel nodeVm)
        {
            if (Graph.Groups.Values.Any(nodeGroup => nodeGroup.Nodes.Contains(nodeVm.Node.Id))) return;

            GroupViewModel? groupToUpdate = null;

            foreach (var groupItem in GraphElements.OfType<GroupViewModel>())
            {
                var mousePos = Mouse.GetPosition(GraphContainer);
                var bounds = new Rect(groupItem.Position.X, groupItem.Position.Y, groupItem.Width, groupItem.Height);

                if (!bounds.Contains(mousePos)) continue;

                groupToUpdate = groupItem;
            }

            if (groupToUpdate is not null)
            {
                groupToUpdate.Group.Nodes.Add(nodeVm.Node.Id);
                updateGroupViewModel(groupToUpdate);
            }
        }

        if (draggingGridGraphElement?.ViewModel is CommentViewModel commentVm)
        {
            if (Graph.Groups.Values.Any(nodeGroup => nodeGroup.Comments.Contains(commentVm.Comment.Id))) return;

            GroupViewModel? groupToUpdate = null;

            foreach (var groupItem in GraphElements.OfType<GroupViewModel>())
            {
                var mousePos = Mouse.GetPosition(GraphContainer);
                var bounds = new Rect(groupItem.Position.X, groupItem.Position.Y, groupItem.Width, groupItem.Height);

                if (!bounds.Contains(mousePos)) continue;

                groupToUpdate = groupItem;
            }

            if (groupToUpdate is not null)
            {
                groupToUpdate.Group.Comments.Add(commentVm.Comment.Id);
                updateGroupViewModel(groupToUpdate);
            }
        }
    }

    private void updateGroupViewModel(GroupViewModel groupVm, bool updateIndexes = true)
    {
        var nodeVms = GraphElements.OfType<NodeViewModel>().Where(nodeVm => groupVm.Group.Nodes.Contains(nodeVm.Node.Id)).ToList();
        var commentVms = GraphElements.OfType<CommentViewModel>().Where(commentVm => groupVm.Group.Comments.Contains(commentVm.Comment.Id)).ToList();

        if (updateIndexes)
        {
            foreach (var nodeGraphItem in nodeVms)
            {
                var index = GraphElements.IndexOf(nodeGraphItem);
                GraphElements.Move(index, GraphElements.Count - 1);
            }

            foreach (var commentGraphItem in commentVms)
            {
                var index = GraphElements.IndexOf(commentGraphItem);
                GraphElements.Move(index, GraphElements.Count - 1);
            }
        }

        var topLeft = new Point(GraphContainer.ActualWidth, GraphContainer.ActualHeight);
        var bottomRight = new Point(0, 0);

        foreach (var nodeGraphItem in nodeVms)
        {
            topLeft.X = Math.Min(topLeft.X, nodeGraphItem.Position.X - GroupPadding.Left);
            topLeft.Y = Math.Min(topLeft.Y, nodeGraphItem.Position.Y - GroupPadding.Top);
            bottomRight.X = Math.Max(bottomRight.X, nodeGraphItem.Position.X + nodeGraphItem.Control.ActualWidth + GroupPadding.Right);
            bottomRight.Y = Math.Max(bottomRight.Y, nodeGraphItem.Position.Y + nodeGraphItem.Control.ActualHeight + GroupPadding.Bottom);
        }

        foreach (var commentGraphItem in commentVms)
        {
            topLeft.X = Math.Min(topLeft.X, commentGraphItem.Position.X - GroupPadding.Left);
            topLeft.Y = Math.Min(topLeft.Y, commentGraphItem.Position.Y - GroupPadding.Top);
            bottomRight.X = Math.Max(bottomRight.X, commentGraphItem.Position.X + commentGraphItem.Control.ActualWidth + GroupPadding.Right);
            bottomRight.Y = Math.Max(bottomRight.Y, commentGraphItem.Position.Y + commentGraphItem.Control.ActualHeight + GroupPadding.Bottom);
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

        var rawGroupPos = new Point(
            mousePos.X - groupDrag.Offset.X,
            mousePos.Y - groupDrag.Offset.Y
        );

        var clampedGroupPos = new Point(
            double.Clamp(rawGroupPos.X, 0, GraphContainer.ActualWidth - groupDrag.GroupVm.Width),
            double.Clamp(rawGroupPos.Y, 0, GraphContainer.ActualHeight - groupDrag.GroupVm.Height)
        );

        var rawDesiredPos = new Point(
            clampedGroupPos.X - groupDrag.OffsetFromGrid.X,
            clampedGroupPos.Y - groupDrag.OffsetFromGrid.Y
        );

        var snappedDesiredPos = new Point(
            snapToGrid(rawDesiredPos.X),
            snapToGrid(rawDesiredPos.Y)
        );

        var currentGroupPos = groupDrag.GroupVm.Position;

        var compensatedCurrentPos = new Point(
            currentGroupPos.X - groupDrag.OffsetFromGrid.X,
            currentGroupPos.Y - groupDrag.OffsetFromGrid.Y
        );

        var snappedCurrentPos = new Point(
            snapToGrid(compensatedCurrentPos.X),
            snapToGrid(compensatedCurrentPos.Y)
        );

        if (double.Abs(snappedDesiredPos.X - snappedCurrentPos.X) < 0.01 &&
            double.Abs(snappedDesiredPos.Y - snappedCurrentPos.Y) < 0.01)
            return;

        var delta = new Vector(
            snappedDesiredPos.X - snappedCurrentPos.X,
            snappedDesiredPos.Y - snappedCurrentPos.Y
        );

        foreach (var graphItem in groupDrag.Items)
        {
            graphItem.SetPosition(new Point(
                graphItem.Position.X + delta.X,
                graphItem.Position.Y + delta.Y
            ));

            if (graphItem is NodeViewModel nodeVm)
                updateNodeViewModelConnections(nodeVm);
        }

        updateGroupViewModel(groupDrag.GroupVm);
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

    private bool createNodeFromDrag()
    {
        Debug.Assert(connectionDrag is not null);

        var element = connectionDrag.Element;
        var slotIndex = connectionDrag.SlotIndex;
        var position = getSnappedMousePos();

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

        if (isFlowOutput)
        {
            var nodeResult = Graph.AddNode(typeof(FlowDisplayNode));
            if (!nodeResult.IsSuccess) return false;

            var node = nodeResult.Value;
            node.Metadata.Position = position;

            var connectionResult = Graph.CreateConnection(element, slotIndex, node.Metadata.ElementInstancesFor(ConnectionPoint.FlowInput)[0], 0);
            if (!connectionResult.IsSuccess) return false;

            return true;
        }

        if (isValueInput && NodeConstants.IsInputType(Nullable.GetUnderlyingType(element.Metadata.Shared.ValueType) ?? element.Metadata.Shared.ValueType))
        {
            var nodeType = typeof(ValueNode<>).MakeGenericType(element.Metadata.Shared.ValueType);
            var nodeResult = Graph.AddNode(nodeType);
            if (!nodeResult.IsSuccess) return false;

            var node = nodeResult.Value;
            node.Metadata.Position = position;

            if (!element.Metadata.Shared.IsList)
                ((IValueInput)node.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[0]).SetField(((IValueInput)element).GetField());

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

    private void updateGroupOfNode(NodeViewModel nodeVm)
    {
        var groupVm = GraphElements.OfType<GroupViewModel>().SingleOrDefault(groupVm => groupVm.Group.Nodes.Contains(nodeVm.Node.Id));

        if (groupVm is not null)
            updateGroupViewModel(groupVm, false);
    }

    private void updateGroupOfComment(CommentViewModel commentVm)
    {
        var groupVm = GraphElements.OfType<GroupViewModel>().SingleOrDefault(groupVm => groupVm.Group.Comments.Contains(commentVm.Comment.Id));

        if (groupVm is not null)
            updateGroupViewModel(groupVm, false);
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
        var control = (FrameworkElement)sender;
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

        if (!vm.Element.Owner.Metadata.Shared.IsFlowOutput)
            Graph.TriggerTree(vm.Element.Owner).Forget();

        Graph.MarkDirty();
    }

    private void ListElementRemove_OnClick(object sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender;
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

        if (!vm.Element.Owner.Metadata.Shared.IsFlowOutput)
            Graph.TriggerTree(vm.Element.Owner).Forget();

        Graph.MarkDirty();
    }

    private void ButtonNode_OnClick(object sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender;
        var vm = (NodeViewModel)control.Tag!;

        e.Handled = true;
        _ = Graph.TriggerTree(vm.Node);
    }

    private void ElementContextMenu_DeleteClick(object? sender, RoutedEventArgs e)
    {
        var control = (FrameworkElement)sender!;
        var vm = (GridGraphElementViewModel)control.Tag!;

        e.Handled = true;

        if (vm is NodeViewModel nodeVm)
        {
            var groupVm = GraphElements.OfType<GroupViewModel>().SingleOrDefault(groupVm => groupVm.Group.Nodes.Contains(nodeVm.Node.Id));
            Graph.RemoveNode(nodeVm.Node.Id);

            if (groupVm is not null)
                updateGroupViewModel(groupVm, false);

            Graph.MarkDirty();
        }

        if (vm is CommentViewModel commentVm)
        {
            var groupVm = GraphElements.OfType<GroupViewModel>().SingleOrDefault(groupVm => groupVm.Group.Comments.Contains(commentVm.Comment.Id));
            Graph.RemoveComment(commentVm.Comment.Id);

            if (groupVm is not null)
                updateGroupViewModel(groupVm, false);

            Graph.MarkDirty();
        }
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

        var rawSelectionPos = new Point(
            mousePos.X - selectionDrag.Offset.X,
            mousePos.Y - selectionDrag.Offset.Y
        );

        var clampedSelectionPos = new Point(
            double.Clamp(rawSelectionPos.X, 0, GraphContainer.ActualWidth - SelectionVisual.ActualWidth),
            double.Clamp(rawSelectionPos.Y, 0, GraphContainer.ActualHeight - SelectionVisual.ActualHeight)
        );

        var rawDesiredPos = new Point(
            clampedSelectionPos.X - selectionDrag.OffsetFromGrid.X,
            clampedSelectionPos.Y - selectionDrag.OffsetFromGrid.Y
        );

        var snappedDesiredPos = new Point(
            snapToGrid(rawDesiredPos.X),
            snapToGrid(rawDesiredPos.Y)
        );

        var currentSelectionPos = new Point(transform.X, transform.Y);

        var compensatedCurrentPos = new Point(
            currentSelectionPos.X - selectionDrag.OffsetFromGrid.X,
            currentSelectionPos.Y - selectionDrag.OffsetFromGrid.Y
        );

        var snappedCurrentPos = new Point(
            snapToGrid(compensatedCurrentPos.X),
            snapToGrid(compensatedCurrentPos.Y)
        );

        if (double.Abs(snappedDesiredPos.X - snappedCurrentPos.X) < 0.01 &&
            double.Abs(snappedDesiredPos.Y - snappedCurrentPos.Y) < 0.01)
            return;

        var delta = new Point(
            snappedDesiredPos.X - snappedCurrentPos.X,
            snappedDesiredPos.Y - snappedCurrentPos.Y
        );

        transform.X = currentSelectionPos.X + delta.X;
        transform.Y = currentSelectionPos.Y + delta.Y;

        var groupVms = GraphElements.OfType<GroupViewModel>();
        var groupUpdates = new List<GroupViewModel>();

        foreach (var item in elementsSelection.Items)
        {
            updateGridGraphElementPosition(item, new Point(item.Position.X + delta.X, item.Position.Y + delta.Y));

            if (item is NodeViewModel nodeVm)
            {
                updateNodeViewModelConnections(nodeVm);

                foreach (var groupVm in groupVms)
                {
                    if (groupVm.Group.Nodes.Contains(nodeVm.Node.Id)) groupUpdates.Add(groupVm);
                }
            }

            if (item is CommentViewModel commentVm)
            {
                foreach (var groupVm in groupVms)
                {
                    if (groupVm.Group.Comments.Contains(commentVm.Comment.Id)) groupUpdates.Add(groupVm);
                }
            }
        }

        foreach (var groupVm in groupUpdates.DistinctBy(groupVm => groupVm.Group.Id))
        {
            updateGroupViewModel(groupVm);
        }
    }

    private void shrinkWrapSelection(IEnumerable<Guid>? forceElements = null)
    {
        var bounds = new Rect(0, 0, SelectionVisual.ActualWidth, SelectionVisual.ActualHeight);

        var topLeft = new Point(GraphContainer.ActualWidth, GraphContainer.ActualHeight);
        var bottomRight = new Point(0, 0);

        var elements = new List<GridGraphElementViewModel>();

        foreach (var graphItem in GraphElements.Where(vm => vm.GetType() != typeof(GroupViewModel)).OfType<GridGraphElementViewModel>())
        {
            var element = graphItem.Control;
            var position = graphItem.Position;
            var startPoint = element.TranslatePoint(new Point(0, 0), SelectionVisual);
            var endPoint = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), SelectionVisual);

            if (forceElements is null)
            {
                if (bounds.Contains(startPoint) && bounds.Contains(endPoint))
                {
                    topLeft.X = Math.Min(topLeft.X, position.X);
                    topLeft.Y = Math.Min(topLeft.Y, position.Y);
                    bottomRight.X = Math.Max(bottomRight.X, position.X + element.ActualWidth);
                    bottomRight.Y = Math.Max(bottomRight.Y, position.Y + element.ActualHeight);
                    elements.Add(graphItem);
                }
            }
            else
            {
                if (graphItem is NodeViewModel nodeVm && forceElements.Contains(nodeVm.Node.Id)
                    || graphItem is CommentViewModel commentVm && forceElements.Contains(commentVm.Comment.Id))
                {
                    topLeft.X = Math.Min(topLeft.X, position.X);
                    topLeft.Y = Math.Min(topLeft.Y, position.Y);
                    bottomRight.X = Math.Max(bottomRight.X, position.X + element.ActualWidth);
                    bottomRight.Y = Math.Max(bottomRight.Y, position.Y + element.ActualHeight);
                    elements.Add(graphItem);
                }
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

    private void deselectGraphItems()
    {
        elementsSelection = null;
        SelectionVisual.Visibility = Visibility.Collapsed;
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
                                       .Cast<GridGraphElementViewModel>()
                                       .ToList();

            var commentVms = GraphElements.OfType<CommentViewModel>()
                                          .Where(commentVm => groupVm.Group.Comments.Contains(commentVm.Comment.Id))
                                          .Cast<GridGraphElementViewModel>()
                                          .ToList();

            var elementVms = nodeVms.Concat(commentVms);

            var offsetFromGrid = new Vector(groupPos.X % SNAP_DISTANCE, groupPos.Y % SNAP_DISTANCE);

            var groupGraphItemIndex = GraphElements.IndexOf(groupVm);
            GraphElements.Move(groupGraphItemIndex, GraphElements.Count - 1);

            foreach (var elementVm in elementVms)
            {
                var index = GraphElements.IndexOf(elementVm);
                GraphElements.Move(index, GraphElements.Count - 1);
            }

            groupDrag = new GroupDrag(offset, offsetFromGrid, groupVm, elementVms);
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
            var offsetFromGrid = new Vector(selectionPos.X % SNAP_DISTANCE, selectionPos.Y % SNAP_DISTANCE);

            selectionDrag = new SelectionDrag(offset, offsetFromGrid);
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
        var offset = window.TranslatePoint(new Point(window.ActualWidth / 2d, window.ActualHeight / 2d), GraphContainer);
        var nodeResult = Graph.AddNode(type, null, new Vector2((float)snapToGrid(offset.X), (float)snapToGrid(offset.Y)));
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

        foreach (var comment in groupVm.Group.Comments.ToList())
        {
            Graph.RemoveComment(comment);
        }

        Graph.DeleteGroup(groupVm.Group.Id);
        Graph.MarkDirty();
    }

    private void SelectionContextMenu_CreateGroupClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(elementsSelection is not null);

        var nodes = elementsSelection.Items.OfType<NodeViewModel>().Select(item => item.Node.Id);
        var comments = elementsSelection.Items.OfType<CommentViewModel>().Select(item => item.Comment.Id);
        Graph.AddGroup(nodes, comments);
        Graph.MarkDirty();
        deselectGraphItems();
    }

    private void SelectionContextMenu_SaveAsPresetClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(elementsSelection is not null);

        var selectedNodes = elementsSelection.Items.OfType<NodeViewModel>().Select(nodeVm => nodeVm.Node.Id).ToList();
        var selectedComments = elementsSelection.Items.OfType<CommentViewModel>().Select(commentVm => commentVm.Comment.Id).ToList();
        var position = (TranslateTransform)SelectionVisual.RenderTransform;

        var presetCreatorWindow = new PresetCreatorWindow();

        presetCreatorWindow.Closed += (_, _) =>
        {
            if (string.IsNullOrEmpty(presetCreatorWindow.PresetName)) return;

            Graph.CreatePreset(presetCreatorWindow.PresetName, selectedNodes, selectedComments, (float)position.X, (float)position.Y);
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

        var groups = GraphElements.OfType<GroupViewModel>().ToList();
        var groupsToUpdate = new List<GroupViewModel>();

        foreach (var item in elementsSelection.Items)
        {
            if (item is NodeViewModel nodeVm)
            {
                var groupVm = groups.SingleOrDefault(groupVm => groupVm.Group.Nodes.Contains(nodeVm.Node.Id));

                if (groupVm is not null)
                {
                    groupVm.Group.Nodes.Remove(nodeVm.Node.Id);

                    if (!groupsToUpdate.Contains(groupVm))
                        groupsToUpdate.Add(groupVm);
                }

                Graph.RemoveNode(nodeVm.Node.Id);
            }

            if (item is CommentViewModel commentVm)
            {
                var groupVm = groups.SingleOrDefault(groupVm => groupVm.Group.Comments.Contains(commentVm.Comment.Id));

                Graph.RemoveComment(commentVm.Comment.Id);

                if (groupVm is not null)
                {
                    groupVm.Group.Comments.Remove(commentVm.Comment.Id);

                    if (!groupsToUpdate.Contains(groupVm))
                        groupsToUpdate.Add(groupVm);
                }
            }
        }

        foreach (var groupVm in groupsToUpdate.Where(vm => vm.Group.Nodes.Count == 0 && vm.Group.Comments.Count == 0).ToList())
        {
            Graph.DeleteGroup(groupVm.Group.Id);
            groupsToUpdate.Remove(groupVm);
        }

        deselectGraphItems();

        await Graph.MarkDirtyAsync();

        foreach (var groupVm in groupsToUpdate)
        {
            updateGroupViewModel(groupVm);
        }
    }

    private void OnGraphTitleEditCompleted(object sender, RoutedEventArgs e)
    {
        Graph.Serialise();
    }

    private void OnGroupTitleEditCompleted(object sender, RoutedEventArgs e)
    {
        Graph.Serialise();
    }

    private void VariableName_EditCompleted(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variable = (IGraphVariable)element.Tag;

        foreach (var nodeVm in GraphElements.OfType<NodeViewModel>().Where(nodeVm => nodeVm.Node is IHasVariableReference varRef && varRef.VariableId == variable.GetId()))
        {
            nodeVm.NotifyProperty(nameof(NodeViewModel.DisplayName));
        }

        Graph.Serialise();
    }

    private void Comment_EditCompleted(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var commentVm = (CommentViewModel)element.Tag;

        Dispatcher.Invoke(() => { }, DispatcherPriority.Loaded);
        updateCommentSnap(commentVm);

        Graph.Serialise();
    }

    public async void SpawnPreset(NodePreset preset)
    {
        var newNodes = preset.SpawnTo(Graph, getCenterPoint().AsVector);
        await Graph.MarkDirtyAsync();
        shrinkWrapSelection(newNodes);
    }

    private void PickerControlBase_OnColorChanged(object sender, RoutedEventArgs e)
    {
        var element = (PickerControlBase)sender;

        if (element.Tag is NodeViewModel nodeVm)
        {
            Debug.Assert(nodeVm.Node.Metadata.Shared.Type.GetGenericTypeDefinition() == typeof(ValueNode<>));

            if (nodeVm.Node.Metadata.Shared.TypeGenerics[0] == typeof(Color))
            {
                nodeVm.Node.GetType().GetProperty(nameof(ValueNode<>.Value))!.SetValue(nodeVm.Node, new Color(element.SelectedColor));
            }

            if (nodeVm.Node.Metadata.Shared.TypeGenerics[0] == typeof(ColorHSL))
            {
                nodeVm.Node.GetType().GetProperty(nameof(ValueNode<>.Value))!.SetValue(nodeVm.Node, new Color(element.SelectedColor).AsColorHSL);
            }

            if (!nodeVm.Node.Metadata.Shared.IsFlowOutput)
                Graph.TriggerTree(nodeVm.Node).Forget();
        }

        if (element.Tag is NodeValueInputViewModel valueInputVm)
        {
            if (valueInputVm.Element.Metadata.Shared.ValueType == typeof(Color))
            {
                ((IValueInput)valueInputVm.Element).SetField(new Color(element.SelectedColor));
            }

            if (valueInputVm.Element.Metadata.Shared.ValueType == typeof(ColorHSL))
            {
                ((IValueInput)valueInputVm.Element).SetField(new Color(element.SelectedColor).AsColorHSL);
            }

            if (!valueInputVm.Element.Owner.Metadata.Shared.IsFlowOutput)
                Graph.TriggerTree(valueInputVm.Element.Owner).Forget();
        }
    }

    private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var inputVm = (NodeValueInputViewModel)textBox.Tag!;

        var type = inputVm.Element.Metadata.Shared.ValueType;
        var text = textBox.Text;

        try
        {
            var iNumberType = typeof(INumber<>).MakeGenericType(type);
            if (!iNumberType.IsAssignableFrom(type)) return;

            var expression = new Expression(text);
            expression.disableImpliedMultiplicationMode();
            var result = expression.calculate();

            textBox.Text = Convert.ChangeType(result, type).ToString() ?? textBox.Text;
        }
        catch
        {
        }
    }
}