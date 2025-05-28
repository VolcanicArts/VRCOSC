// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Nodes.Types.Base;
using VRCOSC.App.Nodes.Types.Strings;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeField
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("Default");

    private readonly SerialisationManager serialiser;

    public ObservableDictionary<Guid, Node> Nodes { get; } = [];
    public ObservableCollection<NodeConnection> Connections { get; } = [];
    public ObservableCollection<NodeGroup> Groups { get; } = [];
    public Dictionary<Guid, NodeVariableSize> VariableSizes = [];

    public int ZIndex { get; set; } = 0;

    public readonly Dictionary<Type, NodeMetadata> Metadata = [];
    public readonly Dictionary<string, IRef> Variables = [];
    public readonly Dictionary<Guid, Dictionary<IStore, IRef>> GlobalStores = [];

    private bool running;

    public NodeField()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodeFieldSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load()
    {
        Deserialise();

        Nodes.OnCollectionChanged((newNodes, _) =>
        {
            foreach (var pair in newNodes)
            {
                pair.Value.ZIndex.Subscribe(_ => Serialise());
            }
        }, true);

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
        running = true;
    }

    public void Stop()
    {
        running = false;
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

        GlobalStores.TryAdd(currentNode.Id, new Dictionary<IStore, IRef>());

        if (!GlobalStores[currentNode.Id].TryGetValue(globalStore, out var iRef))
        {
            return default!;
        }

        return (T)iRef.GetValue()!;
    }

    public void WriteVariable<T>(string name, T value, bool persistent)
    {
        Variables[name] = new Ref<T>(value);

        // TODO: Implement persistence saving to file
    }

    public void CreateFlowConnection(Guid outputNodeId, int outputFlowSlot, Guid inputNodeId)
    {
        var outputAlreadyHasConnection =
            Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.OutputNodeId == outputNodeId && connection.OutputSlot == outputFlowSlot);

        Logger.Log($"Creating flow connection from {Nodes[outputNodeId].GetType().GetFriendlyName()} slot {outputFlowSlot} to {Nodes[inputNodeId].GetType().GetFriendlyName()}");
        Connections.Add(new NodeConnection(ConnectionType.Flow, outputNodeId, outputFlowSlot, inputNodeId, 0, null));

        if (outputAlreadyHasConnection is not null)
        {
            Connections.Remove(outputAlreadyHasConnection);
        }
    }

    public void CreateValueConnection(Guid outputNodeId, int outputValueSlot, Guid inputNodeId, int inputValueSlot)
    {
        var outputNode = Nodes[outputNodeId];
        var inputNode = Nodes[inputNodeId];

        var outputType = outputNode.GetTypeOfOutputSlot(outputValueSlot);
        var inputType = inputNode.GetTypeOfInputSlot(inputValueSlot);
        var existingConnection = Connections.FirstOrDefault(con => con.ConnectionType == ConnectionType.Value && con.InputNodeId == inputNodeId && con.InputSlot == inputValueSlot);

        var newConnectionMade = false;

        if (outputType.IsAssignableTo(inputType))
        {
            Logger.Log(
                $"Creating value connection from {Nodes[outputNodeId].GetType().GetFriendlyName()} slot {outputValueSlot} to {Nodes[inputNodeId].GetType().GetFriendlyName()} slot {inputValueSlot}");
            Connections.Add(new NodeConnection(ConnectionType.Value, outputNodeId, outputValueSlot, inputNodeId, inputValueSlot, outputType));
            newConnectionMade = true;
        }
        else
        {
            if (ConversionHelper.HasImplicitConversion(outputType, inputType))
            {
                Logger.Log($"Inserting cast node from {outputType.GetFriendlyName()} to {inputType.GetFriendlyName()}");

                var castNode = (Node)Activator.CreateInstance(typeof(CastNode<,>).MakeGenericType(outputType, inputType))!;
                addNode(castNode);
                castNode.Position.X = (Nodes[outputNodeId].Position.X + Nodes[inputNodeId].Position.X) / 2f;
                castNode.Position.Y = (Nodes[outputNodeId].Position.Y + Nodes[inputNodeId].Position.Y) / 2f;
                CreateValueConnection(outputNodeId, outputValueSlot, castNode.Id, 0);
                CreateValueConnection(castNode.Id, 0, inputNodeId, inputValueSlot);
                newConnectionMade = true;
            }

            if (inputType == typeof(string))
            {
                Logger.Log($"Inserting {typeof(ToStringNode<>).MakeGenericType(outputType).GetFriendlyName()}");
                var toStringNode = (Node)Activator.CreateInstance(typeof(ToStringNode<>).MakeGenericType(outputType))!;
                addNode(toStringNode);
                toStringNode.Position.X = (Nodes[outputNodeId].Position.X + Nodes[inputNodeId].Position.X) / 2f;
                toStringNode.Position.Y = (Nodes[outputNodeId].Position.Y + Nodes[inputNodeId].Position.Y) / 2f;
                CreateValueConnection(outputNodeId, outputValueSlot, toStringNode.Id, 0);
                CreateValueConnection(toStringNode.Id, 0, inputNodeId, inputValueSlot);
                newConnectionMade = true;
            }
        }

        // if the input already had a connection, disconnect it
        if (newConnectionMade && existingConnection is not null)
        {
            Connections.Remove(existingConnection);
        }
    }

    public void DeleteNode(Node node)
    {
        Connections.RemoveIf(connection => connection.OutputNodeId == node.Id || connection.InputNodeId == node.Id);
        Groups.ForEach(group => group.Nodes.Remove(node.Id));
        Nodes.Remove(node.Id);

        Groups.RemoveIf(group => group.Nodes.Count == 0);

        // TODO: When a ValueRelayNode is removed, bridge connections
    }

    public Node AddNode(Guid id, Type nodeType)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
        node.Id = id;
        addNode(node);
        return node;
    }

    public Node AddNode(Type nodeType)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
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

        node.ZIndex.Value = ZIndex++;
        node.NodeField = this;

        Nodes.Add(node.Id, node);
    }

    private record FlowTask(Task Task, PulseContext Context);

    private readonly Dictionary<Node, FlowTask> tasks = [];

    public void OnParameterReceived(VRChatParameter parameter)
    {
        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IParameterHandler))))
        {
            var tempContext = new PulseContext(this)
            {
                CurrentNode = node
            };

            backtrackNode(node, tempContext);

            var parameterReceiver = (IParameterHandler)node;
            if (!parameterReceiver.HandlesParameter(tempContext, parameter)) continue;

            Task.Run(() => WalkForward(node));
        }
    }

    public void StartFlow(Node node) => Task.Run(async () =>
    {
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

            tasks.Remove(node);
        }

        var newTask = Task.Run(() => node.InternalProcess(c), c.Token);
        tasks.Add(node, new FlowTask(newTask, c));
    });

    public void ProcessNode(Guid nodeId, PulseContext c)
    {
        ProcessNode(Nodes[nodeId], c);
    }

    public void ProcessNode(Node node, PulseContext c)
    {
        if (c.IsCancelled) return;
        if (c.HasRan(node.Id)) return;

        backtrackNode(node, c);
        c.CreateMemory(node);
        var currentBefore = c.CurrentNode;
        c.CurrentNode = node;
        node.InternalProcess(c);
        c.MarkRan(node.Id);
        c.CurrentNode = currentBefore;
    }

    private void backtrackNode(Node node, PulseContext c)
    {
        for (var index = 0; index < node.VirtualValueInputCount(); index++)
        {
            var connection = FindConnectionFromValueInput(node.Id, index);
            if (connection is null) continue;

            var outputNode = Nodes[connection.OutputNodeId];
            if (outputNode.Metadata.IsFlow) continue;

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
        walkForward(triggerNodes, sourceNode, 0);

        foreach (var node in triggerNodes)
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

            if (inputNode.Metadata.IsTrigger && inputNode.Metadata.Inputs[connection.InputSlot].IsReactive)
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
}

public record NodeConnection(ConnectionType ConnectionType, Guid OutputNodeId, int OutputSlot, Guid InputNodeId, int InputSlot, Type? OutputType);

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