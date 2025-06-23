// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Nodes.Types.Base;
using VRCOSC.App.Nodes.Types.Strings;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeGraph : IVRCClientEventHandler
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("Default");
    public Observable<bool> Selected { get; set; } = new(true);

    private readonly SerialisationManager serialiser;

    public ObservableDictionary<Guid, Node> Nodes { get; } = [];
    public ObservableCollection<NodeConnection> Connections { get; } = [];
    public ObservableCollection<NodeGroup> Groups { get; } = [];
    public Dictionary<Guid, NodeVariableSize> VariableSizes = [];

    public int ZIndex { get; set; } = 0;

    public readonly Dictionary<Type, NodeMetadata> Metadata = [];
    public readonly Dictionary<string, IRef> Variables = [];
    public readonly Dictionary<string, IRef> PersistentVariables = [];
    public readonly ConcurrentDictionary<Guid, Dictionary<IStore, IRef>> GlobalStores = [];

    private bool running;

    public NodeGraph()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodeGraphSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load()
    {
        Deserialise();

        Groups.OnCollectionChanged((newGroups, _) =>
        {
            foreach (var group in newGroups)
            {
                group.Nodes.OnCollectionChanged((_, _) => Serialise());
                group.Title.Subscribe(_ => Serialise());
            }
        }, true);

        Nodes.OnCollectionChanged((_, _) => Serialise());
        Connections.OnCollectionChanged((_, _) => Serialise());
        Groups.OnCollectionChanged((_, _) => Serialise());
    }

    public void Serialise()
    {
        serialiser.Serialise();
    }

    public void Deserialise(string importPath = "")
    {
        serialiser.Deserialise(true, importPath);
    }

    public void Start()
    {
        foreach (var (key, value) in PersistentVariables)
        {
            Variables.Add(key, value);
        }

        running = true;
        triggerOnStartNodes();
        walkForwardAllValueNodes();
        startUpdate();
        VRChatLogReader.Register(this);
    }

    public async Task Stop()
    {
        VRChatLogReader.Deregister(this);
        await updateTokenSource!.CancelAsync();
        await updateTask!;
        triggerOnStopNodes();
        clearDisplayNodes();

        try
        {
            await Task.WhenAll(tasks.Values.Select(t => t.Context.Source.CancelAsync()));
            await Task.WhenAll(tasks.Values.Select(t => t.Task).ToArray());
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }

        tasks.Clear();
        GlobalStores.Clear();
        running = false;
        Variables.Clear();
    }

    private void walkForwardAllValueNodes()
    {
        var triggerNodes = new List<Node>();

        foreach (var node in Nodes.Values.Where(node => !node.Metadata.IsFlow && !node.Metadata.IsValueInput && node.Metadata.IsValueOutput))
        {
            walkForward(triggerNodes, node, 0);
        }

        foreach (var triggerNode in triggerNodes.DistinctBy(node => node.Id))
        {
            StartFlow(triggerNode);
        }
    }

    private void clearDisplayNodes()
    {
        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IDisplayNode))))
        {
            ((IDisplayNode)node).Clear();
        }
    }

    private Task? updateTask;
    private CancellationTokenSource? updateTokenSource;

    private readonly object updateNodesLock = new();

    private IEnumerable<Node> updateNodes => Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IUpdateNode)));

    private void startUpdate()
    {
        updateTokenSource = new();

        updateTask = Task.Run(async () =>
        {
            try
            {
                while (!updateTokenSource.IsCancellationRequested)
                {
                    lock (updateNodesLock)
                    {
                        foreach (var node in updateNodes)
                        {
                            var c = new PulseContext(this)
                            {
                                CurrentNode = node
                            };

                            backtrackNode(node, c);

                            if (!((IUpdateNode)node).OnUpdate(c)) continue;
                            if (!node.InternalShouldProcess(c)) continue;

                            if (node.Metadata.IsTrigger || (node.Metadata.IsValueInput && !node.Metadata.IsValueOutput))
                            {
                                StartFlow(node);
                                continue;
                            }

                            if (node.GetType().IsAssignableTo(typeof(IDisplayNode)))
                            {
                                StartFlow(node);
                                continue;
                            }

                            _ = Task.Run(() => WalkForward(node), updateTokenSource.Token);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1d / 60d), updateTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
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
            Connections = Connections.Where(c => nodeIds.Contains(c.OutputNodeId) && nodeIds.Contains(c.InputNodeId)).Select(c => new SerialisableConnection(c)).ToList()
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
        return Connections.SingleOrDefault(c => c.ConnectionType == ConnectionType.Value && c.InputNodeId == nodeId && c.InputSlot == index);
    }

    public NodeConnection? FindConnectionFromFlowOutput(Guid nodeId, int index)
    {
        return Connections.SingleOrDefault(c => c.ConnectionType == ConnectionType.Flow && c.OutputNodeId == nodeId && c.OutputSlot == index);
    }

    public NodeMetadata GetMetadata(Node node) => Metadata[node.GetType()];

    public void WriteStore<T>(GlobalStore<T> globalStore, T value, PulseContext c)
    {
        var currentNode = c.CurrentNode;
        Debug.Assert(currentNode is not null);

        if (!GlobalStores.ContainsKey(currentNode.Id))
            GlobalStores.TryAdd(currentNode.Id, new Dictionary<IStore, IRef>());

        GlobalStores[currentNode.Id][globalStore] = new Ref<T>(value);
    }

    public T ReadStore<T>(GlobalStore<T> globalStore, PulseContext c)
    {
        var currentNode = c.CurrentNode;
        Debug.Assert(currentNode is not null);

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

    public void WriteVariable<T>(string name, T value, bool persistent)
    {
        Variables[name] = new Ref<T>(value);

        if (persistent)
        {
            PersistentVariables[name] = new Ref<T>(value);
            Serialise();
        }
    }

    public void RemoveConnection(NodeConnection connection)
    {
        Connections.Remove(connection);

        // update all the trigger nodes when we remove a connection
        var nodes = Nodes.Values.Where(node => connection.InputNodeId == node.Id && node.Metadata.IsTrigger);

        foreach (var node in nodes)
        {
            StartFlow(node);
        }
    }

    public void CreateFlowConnection(Guid outputNodeId, int outputFlowSlot, Guid inputNodeId)
    {
        if (outputNodeId == inputNodeId) return;

        var outputAlreadyHasConnection =
            Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.OutputNodeId == outputNodeId && connection.OutputSlot == outputFlowSlot);

        Connections.Add(new NodeConnection(ConnectionType.Flow, outputNodeId, outputFlowSlot, null, inputNodeId, 0, null));

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
        var existingConnection = Connections.FirstOrDefault(con => con.ConnectionType == ConnectionType.Value && con.InputNodeId == inputNodeId && con.InputSlot == inputValueSlot);

        NodeConnection? newConnection = null;
        var newConnectionMade = false;

        if (outputType.IsAssignableTo(inputType))
        {
            newConnection = new NodeConnection(ConnectionType.Value, outputNodeId, outputValueSlot, outputType, inputNodeId, inputValueSlot, inputType);
            newConnectionMade = true;
        }
        else
        {
            var group = Groups.SingleOrDefault(group => group.Nodes.Contains(outputNodeId) && group.Nodes.Contains(inputNodeId));

            if (ConversionHelper.HasImplicitConversion(outputType, inputType))
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
        }

        // if the input already had a connection, disconnect it
        if (newConnectionMade && existingConnection is not null)
        {
            RemoveConnection(existingConnection);
        }

        if (newConnection is not null)
        {
            Connections.Add(newConnection);
        }
    }

    public void DeleteNode(Guid nodeId)
    {
        Connections.RemoveIf(connection => connection.OutputNodeId == nodeId || connection.InputNodeId == nodeId);
        Groups.ForEach(group => group.Nodes.Remove(nodeId));
        Nodes.Remove(nodeId);

        Groups.RemoveIf(group => group.Nodes.Count == 0);

        // TODO: When a ValueRelayNode is removed, bridge connections
    }

    public Node AddNode(Guid id, Type nodeType, Point initialPosition)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
        node.Id = id;
        node.NodePosition = initialPosition;
        addNode(node);
        return node;
    }

    public Node AddNode(Type nodeType, Point initialPosition)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
        node.NodePosition = initialPosition;
        addNode(node);
        return node;
    }

    private void addNode(Node node)
    {
        NodeMetadata metadata;

        if (Metadata.TryGetValue(node.GetType(), out var foundMetadata))
        {
            metadata = foundMetadata;
        }
        else
        {
            metadata = NodeMetadataBuilder.BuildFrom(node);
            Metadata.Add(node.GetType(), metadata);
        }

        if (metadata.ValueInputHasVariableSize || metadata.ValueOutputHasVariableSize)
        {
            VariableSizes.Add(node.Id, new NodeVariableSize());
        }

        node.NodeGraph = this;
        Nodes.Add(node.Id, node);
    }

    private record FlowTask(Task Task, PulseContext Context);

    private readonly ConcurrentDictionary<Node, FlowTask> tasks = [];

    private void handleNodeEvent(Func<PulseContext, INodeEventHandler, bool> shouldPropagate)
    {
        if (!running) return;

        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(INodeEventHandler))))
        {
            var c = new PulseContext(this)
            {
                CurrentNode = node
            };

            backtrackNode(node, c);

            if (!shouldPropagate.Invoke(c, (INodeEventHandler)node)) continue;

            if (node.Metadata.IsTrigger)
            {
                StartFlow(node);
                continue;
            }

            Task.Run(() => WalkForward(node));
        }
    }

    public void OnParameterReceived(VRChatParameter parameter)
    {
        handleNodeEvent((c, node) => node.HandleParameterReceive(c, parameter));
    }

    public void OnAvatarChange(AvatarConfig? config)
    {
        handleNodeEvent((c, node) => node.HandleAvatarChange(c, config));
    }

    public void OnPartialSpeechResult(string result)
    {
        handleNodeEvent((c, node) => node.HandlePartialSpeechResult(c, result));
    }

    public void OnFinalSpeechResult(string result)
    {
        handleNodeEvent((c, node) => node.HandleFinalSpeechResult(c, result));
    }

    private void triggerOnStartNodes()
    {
        handleNodeEvent((c, node) => node.HandleNodeStart(c));
    }

    private void triggerOnStopNodes()
    {
        handleNodeEvent((c, node) => node.HandleNodeStop(c));
    }

    public void OnInstanceJoined(VRChatClientEventInstanceJoined eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnInstanceJoined(c, eventArgs));
    }

    public void OnInstanceLeft(VRChatClientEventInstanceLeft eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnInstanceLeft(c, eventArgs));
    }

    public void OnUserJoined(VRChatClientEventUserJoined eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnUserJoined(c, eventArgs));
    }

    public void OnUserLeft(VRChatClientEventUserLeft eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnUserLeft(c, eventArgs));
    }

    public void OnAvatarPreChange(VRChatClientEventAvatarPreChange eventArgs)
    {
        handleNodeEvent((c, node) => node.HandleOnAvatarPreChange(c, eventArgs));
    }

    public void StartFlow(Node node) => Task.Run(async () =>
    {
        if (!running) return;

        var c = new PulseContext(this)
        {
            CurrentNode = node
        };

        backtrackNode(node, c);

        if (!node.InternalShouldProcess(c)) return;

        c.CreateMemory(node);

        if (tasks.TryGetValue(node, out var existingTask))
        {
            await existingTask.Context.Source.CancelAsync();

            try
            {
                await existingTask.Task;
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }

            tasks.TryRemove(node, out _);
        }

        c.MarkRan(node.Id);

        var newTask = Task.Run(() => node.InternalProcess(c), c.Token);
        tasks.TryAdd(node, new FlowTask(newTask, c));
    });

    public void ProcessNode(Guid nodeId, PulseContext c)
    {
        ProcessNode(Nodes[nodeId], c);
    }

    public void ProcessNode(Node node, PulseContext c)
    {
        if (c.IsCancelled) return;
        if (c.HasRan(node.Id) && !node.Metadata.ForceReprocess) return;

        backtrackNode(node, c);
        c.CreateMemory(node);
        var currentBefore = c.CurrentNode;
        c.CurrentNode = node;
        c.MarkRan(node.Id);
        node.InternalProcess(c);
        c.CurrentNode = currentBefore;
    }

    private void processImpulseReceiver(Node node, object[] values, PulseContext c)
    {
        if (c.IsCancelled) return;

        backtrackNode(node, c);
        c.CreateMemory(node);
        var currentBefore = c.CurrentNode;
        c.CurrentNode = node;
        ((IImpulseReceiver)node).WriteOutputs(values, c);
        c.MarkRan(node.Id);
        node.InternalProcess(c);
        c.CurrentNode = currentBefore;
    }

    private void backtrackNode(Node node, PulseContext c)
    {
        for (var index = 0; index < node.VirtualValueInputCount(); index++)
        {
            var connection = FindConnectionFromValueInput(node.Id, index);
            if (connection is null) continue;

            var outputNode = Nodes[connection.OutputNodeId];

            if (outputNode.Metadata.IsFlow)
            {
                // we want to create default memory so that backtracking the outputs exists
                if (!c.HasRan(outputNode.Id))
                    c.CreateMemory(outputNode);

                continue;
            }

            ProcessNode(outputNode, c);
        }
    }

    /// <summary>
    /// Walks forward from a source node with a single output to find the trigger node to execute
    /// </summary>
    public void WalkForward(Node sourceNode)
    {
        if (!running) return;

        var triggerNodes = new List<Node>();

        for (var i = 0; i < sourceNode.VirtualValueOutputCount(); i++)
        {
            walkForward(triggerNodes, sourceNode, i);
        }

        foreach (var node in triggerNodes.DistinctBy(node => node.Id))
        {
            StartFlow(node);
        }
    }

    private void walkForward(List<Node> results, Node sourceNode, int outputValueSlot)
    {
        var connections = Connections.Where(c => c.ConnectionType == ConnectionType.Value && c.OutputNodeId == sourceNode.Id && c.OutputSlot == outputValueSlot);

        foreach (var connection in connections)
        {
            var inputNode = Nodes[connection.InputNodeId];
            var inputSlot = connection.InputSlot;

            if (inputSlot >= inputNode.Metadata.InputsCount) inputSlot = inputNode.Metadata.InputsCount - 1;

            if (inputNode.Metadata.IsTrigger && inputNode.Metadata.Inputs[inputSlot].IsReactive)
            {
                results.Add(inputNode);
                continue;
            }

            for (var i = 0; i < inputNode.Metadata.OutputsCount; i++)
            {
                walkForward(results, inputNode, i);
            }
        }
    }

    public NodeGroup AddGroup(Guid? id = null)
    {
        var nodeGroup = new NodeGroup();
        if (id.HasValue) nodeGroup.Id = id.Value;
        Groups.Add(nodeGroup);
        return nodeGroup;
    }

    public void TriggerImpulse(ImpulseDefinition definition, PulseContext c)
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

            processImpulseReceiver(node, definition.Values, c);
        }
    }
}

public record NodeConnection(ConnectionType ConnectionType, Guid OutputNodeId, int OutputSlot, Type? OutputType, Guid InputNodeId, int InputSlot, Type? InputType);

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