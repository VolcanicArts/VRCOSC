// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Strings;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeGraph : IVRCClientEventHandler
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("New Graph");
    public Observable<bool> Selected { get; set; } = new();

    private readonly SerialisationManager serialiser;

    public ConcurrentDictionary<Guid, Node> Nodes { get; } = [];
    public ConcurrentDictionary<Guid, NodeConnection> Connections { get; } = [];
    public ConcurrentDictionary<Guid, NodeGroup> Groups { get; } = [];
    public ConcurrentDictionary<Guid, NodeVariableSize> VariableSizes = [];
    public ConcurrentDictionary<Guid, IGraphVariable> GraphVariables { get; } = [];

    public readonly Dictionary<Type, NodeMetadata> Metadata = [];
    public readonly ConcurrentDictionary<Guid, Dictionary<IStore, IRef>> GlobalStores = [];

    private bool running;

    public List<Node> AddedNodes = [];
    public List<Node> RemovedNodes = [];
    public List<NodeConnection> AddedConnections = [];
    public List<NodeConnection> RemovedConnections = [];
    public List<NodeGroup> AddedGroups = [];
    public List<NodeGroup> RemovedGroups = [];

    public Func<Task>? OnMarkedDirty;
    public bool UILoaded { get; set; }

    public NodeGraph()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodeGraphSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load(string importPath = "")
    {
        if (string.IsNullOrEmpty(importPath))
            serialiser.Deserialise();
        else
            serialiser.Deserialise(false, importPath);
    }

    public void Serialise()
    {
        serialiser.Serialise();
    }

    public async Task Start()
    {
        running = true;
        await triggerOnStartNodes();
        await processAllTriggerNodes();
        startUpdate();
        VRChatLogReader.Register(this);
    }

    public async Task Stop()
    {
        VRChatLogReader.Deregister(this);
        await updateTokenSource!.CancelAsync();
        await updateTask!;

        try
        {
            // TODO: Improve to know if a flow contains any nodes that loop, indefinitely, if so cancel and wait otherwise wait
            await Task.WhenAll(tasks.Values.Select(t => t.Context.Source.CancelAsync()));
            await Task.WhenAll(tasks.Values.Select(t => t.Task));
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }

        await triggerOnStopNodes();
        clearDisplayNodes();

        tasks.Clear();
        GlobalStores.Clear();
        GraphVariables.ForEach(v => v.Value.Reset());
        running = false;

        Serialise();
    }

    #region Management

    public void MarkDirty() => MarkDirtyAsync().Forget();

    public async Task MarkDirtyAsync()
    {
        if (OnMarkedDirty is not null)
            await OnMarkedDirty.Invoke();

        AddedNodes.Clear();
        RemovedNodes.Clear();
        AddedConnections.Clear();
        RemovedConnections.Clear();
        AddedGroups.Clear();
        RemovedGroups.Clear();

        Serialise();
    }

    public Node AddNode(Type nodeType, Point initialPosition, Guid? id = null)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
        node.Init();
        node.NodePosition = initialPosition;

        if (id.HasValue)
            node.Id = id.Value;

        NodeMetadata metadata;

        if (Metadata.TryGetValue(nodeType, out var foundMetadata))
        {
            metadata = foundMetadata;
        }
        else
        {
            metadata = NodeMetadataBuilder.BuildFrom(node);
            Metadata.Add(nodeType, metadata);
        }

        if (metadata.ValueInputHasVariableSize || metadata.ValueOutputHasVariableSize)
            VariableSizes.TryAdd(node.Id, new NodeVariableSize());

        node.NodeGraph = this;

        Nodes.TryAdd(node.Id, node);
        AddedNodes.Add(node);

        return node;
    }

    public void DeleteNode(Guid nodeId)
    {
        var node = Nodes[nodeId];
        var nodesToTrigger = new List<Guid>();

        foreach (var connection in Connections.Values.Where(connection => connection.OutputNodeId == nodeId || connection.InputNodeId == nodeId).ToList())
        {
            Connections.TryRemove(connection.Id, out _);
            RemovedConnections.Add(connection);
            nodesToTrigger.Add(connection.InputNodeId);
        }

        var group = Groups.Values.SingleOrDefault(group => group.Nodes.Contains(nodeId));

        if (group is not null)
        {
            group.Nodes.Remove(nodeId);
            if (group.Nodes.Count == 0) DeleteGroup(group.Id);
        }

        Nodes.TryRemove(nodeId, out _);
        RemovedNodes.Add(node);

        foreach (var nodeToTriggerId in nodesToTrigger.Distinct())
        {
            if (nodeToTriggerId == nodeId) continue;

            TriggerTree(Nodes[nodeToTriggerId]).Forget();
        }
    }

    public void CreateFlowConnection(Guid outputNodeId, int outputFlowSlot, Guid inputNodeId)
    {
        if (outputNodeId == inputNodeId) return;

        var outputAlreadyHasConnection =
            Connections.Values.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.OutputNodeId == outputNodeId && connection.OutputSlot == outputFlowSlot);

        var newConnection = new NodeConnection(Guid.NewGuid(), ConnectionType.Flow, outputNodeId, outputFlowSlot, null, inputNodeId, 0, null);
        Connections.TryAdd(newConnection.Id, newConnection);
        AddedConnections.Add(newConnection);

        if (outputAlreadyHasConnection is not null)
        {
            RemoveConnection(outputAlreadyHasConnection);
        }
    }

    public void CreateValueConnection(Guid outputNodeId, int outputValueSlot, Guid inputNodeId, int inputValueSlot)
    {
        if (outputNodeId == inputNodeId) return;

        var outputNode = Nodes[outputNodeId];
        var inputNode = Nodes[inputNodeId];

        var outputType = outputNode.GetTypeOfOutputSlot(outputValueSlot);
        var inputType = inputNode.GetTypeOfInputSlot(inputValueSlot);
        var existingConnection = Connections.Values.FirstOrDefault(con => con.ConnectionType == ConnectionType.Value && con.InputNodeId == inputNodeId && con.InputSlot == inputValueSlot);

        NodeConnection? newConnection = null;
        var newConnectionMade = false;

        if (outputType.IsAssignableTo(inputType))
        {
            newConnection = new NodeConnection(Guid.NewGuid(), ConnectionType.Value, outputNodeId, outputValueSlot, outputType, inputNodeId, inputValueSlot, inputType);
            newConnectionMade = true;
        }
        else
        {
            var group = Groups.Values.SingleOrDefault(group => group.Nodes.Contains(outputNodeId) && group.Nodes.Contains(inputNodeId));

            if (ConversionHelper.HasImplicitConversion(outputType, inputType))
            {
                var castNode = AddNode(typeof(CastNode<,>).MakeGenericType(outputType, inputType), new Point((outputNode.NodePosition.X + inputNode.NodePosition.X) / 2f, (outputNode.NodePosition.Y + inputNode.NodePosition.Y) / 2f));
                CreateValueConnection(outputNodeId, outputValueSlot, castNode.Id, 0);
                CreateValueConnection(castNode.Id, 0, inputNodeId, inputValueSlot);
                group?.Nodes.Add(castNode.Id);
                newConnectionMade = true;
            }

            if (outputType.IsEnum && inputType == typeof(int))
            {
                var castNode = AddNode(typeof(CastNode<,>).MakeGenericType(outputType, inputType), new Point((outputNode.NodePosition.X + inputNode.NodePosition.X) / 2f, (outputNode.NodePosition.Y + inputNode.NodePosition.Y) / 2f));
                CreateValueConnection(outputNodeId, outputValueSlot, castNode.Id, 0);
                CreateValueConnection(castNode.Id, 0, inputNodeId, inputValueSlot);
                group?.Nodes.Add(castNode.Id);
                newConnectionMade = true;
            }

            if (inputType == typeof(string))
            {
                var toStringNode = AddNode(typeof(ToStringNode<>).MakeGenericType(outputType), new Point((outputNode.NodePosition.X + inputNode.NodePosition.X) / 2f, (outputNode.NodePosition.Y + inputNode.NodePosition.Y) / 2f));
                CreateValueConnection(outputNodeId, outputValueSlot, toStringNode.Id, 0);
                CreateValueConnection(toStringNode.Id, 0, inputNodeId, inputValueSlot);
                group?.Nodes.Add(toStringNode.Id);
                newConnectionMade = true;
            }

            if (outputType == typeof(double) && inputType == typeof(float))
            {
                var castNode = AddNode(typeof(CastNode<,>).MakeGenericType(outputType, inputType), new Point((outputNode.NodePosition.X + inputNode.NodePosition.X) / 2f, (outputNode.NodePosition.Y + inputNode.NodePosition.Y) / 2f));
                CreateValueConnection(outputNodeId, outputValueSlot, castNode.Id, 0);
                CreateValueConnection(castNode.Id, 0, inputNodeId, inputValueSlot);
                group?.Nodes.Add(castNode.Id);
                newConnectionMade = true;
            }
        }

        // if the input already had a connection, disconnect it
        if (newConnectionMade && existingConnection is not null)
        {
            RemoveConnection(existingConnection);
        }

        if (newConnection is not null)
        {
            Connections.TryAdd(newConnection.Id, newConnection);
            AddedConnections.Add(newConnection);
        }

        if (newConnectionMade)
            TriggerTree(inputNode).Forget();
    }

    public void RemoveConnection(NodeConnection connection)
    {
        Connections.TryRemove(connection.Id, out _);
        RemovedConnections.Add(connection);

        var affectedNode = Nodes[connection.InputNodeId];
        TriggerTree(affectedNode).Forget();
    }

    public NodeGroup AddGroup(IEnumerable<Guid> initialNodes, Guid? id = null)
    {
        var nodeGroup = new NodeGroup();
        nodeGroup.Nodes.AddRange(initialNodes);
        if (id.HasValue) nodeGroup.Id = id.Value;
        Groups.TryAdd(nodeGroup.Id, nodeGroup);
        AddedGroups.Add(nodeGroup);
        return nodeGroup;
    }

    public void DeleteGroup(Guid id)
    {
        Groups.TryRemove(id, out var group);
        RemovedGroups.Add(group!);
    }

    #endregion

    public void CreateVariable(Type variableType, string name, bool persistent)
    {
        var variable = (IGraphVariable)Activator.CreateInstance(typeof(GraphVariable<>).MakeGenericType(variableType), args: [name, persistent])!;
        GraphVariables.TryAdd(variable.GetId(), variable);

        Serialise();
    }

    public void DeleteVariable(IGraphVariable variable)
    {
        foreach (var (_, node) in Nodes)
        {
            if (node is IHasVariableReference variableReferenceNode && variableReferenceNode.VariableId == variable.GetId())
            {
                DeleteNode(node.Id);
            }
        }

        GraphVariables.Remove(variable.GetId(), out _);

        Serialise();
    }

    private async Task processAllTriggerNodes()
    {
        foreach (var node in Nodes.Values.Where(node => triggerCriteria(node) && node.Metadata.Inputs.Any(input => input.IsReactive)))
        {
            await TriggerTree(node);
        }
    }

    private void clearDisplayNodes()
    {
        foreach (var displayNode in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IDisplayNode))).Cast<IDisplayNode>())
        {
            displayNode.Clear();
        }
    }

    private Task? updateTask;
    private CancellationTokenSource? updateTokenSource;

    private IEnumerable<Node> updateNodes => Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IUpdateNode))).OrderBy(node => ((IUpdateNode)node).UpdateOffset);
    private IEnumerable<Node> activeUpdateNodes => Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IActiveUpdateNode))).OrderBy(node => ((IActiveUpdateNode)node).UpdateOffset);

    private void startUpdate()
    {
        updateTokenSource = new();

        updateTask = Task.Run(async () =>
        {
            try
            {
                while (!updateTokenSource.IsCancellationRequested)
                {
                    foreach (var node in activeUpdateNodes)
                    {
                        var c = new PulseContext(this);
                        var hasProcessed = await processNode(node, c, () => ((IActiveUpdateNode)node).OnUpdate(c));

                        if (!hasProcessed) continue;

                        if (!node.Metadata.IsFlowOutput && node.Metadata.IsValueOutput)
                            await TriggerTree(node, c);
                    }

                    foreach (var node in updateNodes)
                    {
                        var c = new PulseContext(this);
                        c.Push(node);
                        // Not needed but just in case a write happens so errors don't throw
                        c.CreateMemory(node);
                        ((IUpdateNode)node).OnUpdate(c);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1d / 100d));
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }, updateTokenSource.Token);
    }

    public void CreatePreset(string name, List<Guid> nodeIds, float posX, float posY)
    {
        var nodePreset = new NodePreset
        {
            Name = { Value = name },
            Nodes = nodeIds.Select(id => new SerialisableNode(Nodes[id])).ToList(),
            Connections = Connections.Values.Where(c => nodeIds.Contains(c.OutputNodeId) && nodeIds.Contains(c.InputNodeId)).Select(c => new SerialisableConnection(c)).ToList(),
            Groups = Groups.Values.Where(g => g.Nodes.All(nodeIds.Contains)).Select(g => new SerialisableNodeGroup(g)).ToList(),
            Variables = nodeIds.Select(id => Nodes[id]).OfType<IHasVariableReference>().Select(node => new SerialisableGraphVariable(GraphVariables[node.VariableId])).ToList()
        };

        foreach (var node in nodePreset.Nodes)
        {
            node.Position = new Vector2(node.Position.X - posX, node.Position.Y - posY);
        }

        NodeManager.GetInstance().Presets.Add(nodePreset);
        nodePreset.Serialise();
    }

    public NodeConnection? FindConnectionFromValueInput(Guid nodeId, int index)
    {
        return Connections.Values.SingleOrDefault(c => c.ConnectionType == ConnectionType.Value && c.InputNodeId == nodeId && c.InputSlot == index);
    }

    public NodeConnection? FindConnectionFromFlowOutput(Guid nodeId, int index)
    {
        return Connections.Values.SingleOrDefault(c => c.ConnectionType == ConnectionType.Flow && c.OutputNodeId == nodeId && c.OutputSlot == index);
    }

    public NodeMetadata GetMetadata(Node node) => Metadata[node.GetType()];

    public void WriteStore<T>(GlobalStore<T> globalStore, T value, PulseContext c)
    {
        var currentNode = c.Peek();

        if (!GlobalStores.ContainsKey(currentNode.Id))
            GlobalStores.TryAdd(currentNode.Id, new Dictionary<IStore, IRef>());

        GlobalStores[currentNode.Id][globalStore] = new Ref<T>(value);
    }

    public T ReadStore<T>(GlobalStore<T> globalStore, PulseContext c)
    {
        var currentNode = c.Peek();

        if (!GlobalStores.TryGetValue(currentNode.Id, out var nodeStore))
        {
            var value = new Dictionary<IStore, IRef>();
            GlobalStores.TryAdd(currentNode.Id, value);
            nodeStore = value;
        }

        if (!nodeStore.TryGetValue(globalStore, out var iRef))
        {
            return default!;
        }

        return (T)iRef.GetValue()!;
    }

    private record FlowTask(Task Task, PulseContext Context);

    private readonly ConcurrentDictionary<Node, FlowTask> tasks = [];

    private async Task triggerOnStartNodes()
    {
        var startTasks = new List<Task>();

        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(INodeEventHandler))))
        {
            var c = new PulseContext(this);
            var nodeTask = processNode(node, c, () => ((INodeEventHandler)node).HandleNodeStart(c));
            startTasks.Add(nodeTask);
        }

        await Task.WhenAll(startTasks);
    }

    private async Task triggerOnStopNodes()
    {
        var stopTasks = new List<Task>();

        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(INodeEventHandler))))
        {
            var c = new PulseContext(this);
            var nodeTask = processNode(node, c, () => ((INodeEventHandler)node).HandleNodeStop(c));
            stopTasks.Add(nodeTask);
        }

        await Task.WhenAll(stopTasks);
    }

    private async Task handleNodeEvent(Func<PulseContext, INodeEventHandler, Task<bool>> shouldHandleEvent)
    {
        Debug.Assert(running);

        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(INodeEventHandler))))
        {
            var c = new PulseContext(this);
            var hasProcessed = await processNode(node, c, () => shouldHandleEvent.Invoke(c, (INodeEventHandler)node));

            if (!hasProcessed) continue;

            if (!node.Metadata.IsFlowOutput)
                await TriggerTree(node, c);
        }
    }

    public void OnPartialSpeechResult(string result)
    {
        handleNodeEvent((c, node) => node.HandlePartialSpeechResult(c, result)).Forget();
    }

    public void OnFinalSpeechResult(string result)
    {
        handleNodeEvent((c, node) => node.HandleFinalSpeechResult(c, result)).Forget();
    }

    public void OnInstanceJoined(VRChatClientEventInstanceJoined eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnInstanceJoined(c, eventArgs)).Forget();
    }

    public void OnInstanceLeft(VRChatClientEventInstanceLeft eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnInstanceLeft(c, eventArgs)).Forget();
    }

    public void OnUserJoined(VRChatClientEventUserJoined eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnUserJoined(c, eventArgs)).Forget();
    }

    public void OnUserLeft(VRChatClientEventUserLeft eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnUserLeft(c, eventArgs)).Forget();
    }

    public void OnAvatarPreChange(VRChatClientEventAvatarPreChange eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnAvatarPreChange(c, eventArgs)).Forget();
    }

    private async Task startFlow(Node node, PulseContext? baseContext = null)
    {
        var c = baseContext is null ? new PulseContext(this) : new PulseContext(baseContext, this);

        // display node, drive node, etc... Don't bother making a FlowTask
        if (node.Metadata.IsValueInput && !node.Metadata.IsValueOutput && !node.Metadata.IsFlow)
        {
            await processNode(node, c);
            return;
        }

        var shouldProcess = await checkShouldProcess(node, c);
        if (!shouldProcess) return;

        if (tasks.TryGetValue(node, out var existingTask))
        {
            await existingTask.Context.Source.CancelAsync();
            await existingTask.Task;
            tasks.TryRemove(node, out _);
        }

        var newTask = Task.Run(async () =>
        {
            await node.InternalProcess(c);

            if (!node.Metadata.MultiFlow)
                tasks.TryRemove(node, out _);
        });

        if (!node.Metadata.MultiFlow)
            tasks.TryAdd(node, new FlowTask(newTask, c));
    }

    public Task ProcessNode(Guid nodeId, PulseContext c) => processNode(Nodes[nodeId], c);

    private async Task<bool> checkShouldProcess(Node node, PulseContext c)
    {
        await backtrackNode(node, c);
        c.Push(node);
        c.CreateMemory(node);
        return node.InternalShouldProcess(c);
    }

    /// <summary>
    /// Processes a node, with an optional preprocess step after <see cref="Node.ShouldProcess"/> returns true
    /// </summary>
    private async Task<bool> processNode(Node node, PulseContext c, Func<Task<bool>>? onPreProcess = null)
    {
        if (c.IsCancelled) return false;
        if (c.HasMemory(node.Id) && !node.Metadata.ForceReprocess) return false;

        await backtrackNode(node, c);
        if (c.IsCancelled) return false;

        c.Push(node);
        c.CreateMemory(node);
        if (c.IsCancelled) return false;

        if (!node.InternalShouldProcess(c))
        {
            c.Pop();
            return false;
        }

        if (c.IsCancelled) return false;

        if (onPreProcess is not null)
        {
            var result = await onPreProcess.Invoke();

            if (!result)
            {
                c.Pop();
                return false;
            }
        }

        if (c.IsCancelled) return false;

        await node.InternalProcess(c);
        c.Pop();

        return true;
    }

    private async Task backtrackNode(Node node, PulseContext c)
    {
        for (var index = 0; index < node.VirtualValueInputCount(); index++)
        {
            var connection = FindConnectionFromValueInput(node.Id, index);
            if (connection is null) continue;

            var outputNode = Nodes[connection.OutputNodeId];

            if (outputNode.Metadata.IsFlow)
            {
                // we want to create default memory so that backtracking the outputs exists
                if (!c.HasMemory(outputNode.Id))
                    c.CreateMemory(outputNode);

                continue;
            }

            await processNode(outputNode, c);
        }
    }

    private readonly Func<Node, bool> triggerCriteria = node => node.Metadata.IsTrigger || (node.Metadata.IsValueInput && !node.Metadata.IsValueOutput && !node.Metadata.IsFlow);

    /// <summary>
    /// Triggers the source node immediately if <see cref="triggerCriteria"/> applies, otherwise walks forward to get all the nodes that <see cref="triggerCriteria"/> applies to and triggers those
    /// </summary>
    public async Task TriggerTree(Node sourceNode, PulseContext? baseContext = null)
    {
        if (!running) return;

        if (triggerCriteria(sourceNode))
        {
            await startFlow(sourceNode, baseContext);
            return;
        }

        var c = baseContext is null ? new PulseContext(this) : new PulseContext(baseContext, this);
        var pathList = new List<Node[]>();
        var currPath = new Stack<Node>();

        currPath.Push(sourceNode);

        for (var i = 0; i < sourceNode.VirtualValueOutputCount(); i++)
        {
            await walkForward(pathList, currPath, sourceNode, i);
        }

        // we want to process every non-trigger node before processing the trigger nodes so that
        // every non-trigger node in the tree only runs once, even if non-trigger node isn't part
        // of the current path but will be down one of the trigger node's flows

        foreach (var path in pathList)
        {
            // traverse backwards as ToArray on a stack reverses the order
            for (var i = path.Length - 1; i > 0; i--)
            {
                var node = path[i];
                await processNode(node, c);
            }
        }

        foreach (var node in pathList.Select(path => path.First()).DistinctBy(node => node.Id))
        {
            await startFlow(node, c);
        }
    }

    private async Task walkForward(List<Node[]> nodeList, Stack<Node> currPath, Node sourceNode, int outputValueSlot)
    {
        var connections = Connections.Values.Where(con => con.ConnectionType == ConnectionType.Value && con.OutputNodeId == sourceNode.Id && con.OutputSlot == outputValueSlot);

        foreach (var connection in connections)
        {
            var inputNode = Nodes[connection.InputNodeId];
            var inputSlot = connection.InputSlot;

            if (inputNode.Metadata.IsFlowInput)
                continue;

            if (inputSlot >= inputNode.Metadata.InputsCount) inputSlot = inputNode.Metadata.InputsCount - 1;

            currPath.Push(inputNode);

            if (triggerCriteria(inputNode) && inputNode.Metadata.Inputs[inputSlot].IsReactive)
            {
                nodeList.Add(currPath.ToArray());
                continue;
            }

            for (var i = 0; i < inputNode.VirtualValueOutputCount(); i++)
            {
                await walkForward(nodeList, currPath, inputNode, i);
            }

            currPath.Pop();
        }
    }

    public async Task TriggerImpulse(ImpulseDefinition definition, PulseContext c)
    {
        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IImpulseReceiver))))
        {
            var impulseNode = (IImpulseReceiver)node;

            if (!string.Equals(definition.Name, impulseNode.Text, StringComparison.CurrentCulture)) continue;

            var type = node.GetType();

            if (definition.Values.Length == 0 && type.IsGenericType) continue;

            if (definition.Values.Length != 0)
            {
                if (!type.IsGenericType) continue;
                if (!type.GenericTypeArguments.SequenceEqual(definition.Values.Select(o => o.GetType()))) continue;
            }

            var newC = new PulseContext(c, this);

            await processNode(node, newC, () =>
            {
                impulseNode.WriteOutputs(definition.Values, c);
                return Task.FromResult(true);
            });
        }
    }
}

public record NodeConnection(Guid Id, ConnectionType ConnectionType, Guid OutputNodeId, int OutputSlot, Type? OutputType, Guid InputNodeId, int InputSlot, Type? InputType);

public enum ConnectionType
{
    Flow,
    Value
}

[Flags]
public enum ConnectionSide
{
    Input = 1 << 0,
    Output = 1 << 1
}