// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Nodes.Types.Base;
using VRCOSC.App.Nodes.Types.Flow.Impulse;
using VRCOSC.App.Nodes.Types.Strings;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeField
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Node Field";

    private readonly SerialisationManager serialiser;

    public ObservableDictionary<Guid, Node> Nodes { get; } = [];
    public ObservableCollection<NodeConnection> Connections { get; } = [];
    public ObservableCollection<NodeGroup> Groups { get; } = [];
    public Dictionary<Guid, NodeVariableSize> VariableSizes = [];

    public int ZIndex { get; set; } = 0;

    public readonly Dictionary<Type, NodeMetadata> Metadata = [];
    public readonly Dictionary<string, object?> Variables = [];

    private bool running;

    public NodeField()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodeFieldSerialiser(AppManager.GetInstance().Storage, this));
        Deserialise();
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

    public NodeMetadata GetMetadata(Node node) => Metadata[node.GetType()];

    public void WriteVariable(string name, object? value, bool persistent)
    {
        Variables[name] = value;

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

        var outputMetadata = outputNode.Metadata;
        var inputMetadata = inputNode.Metadata;

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

        if (outputMetadata is { IsValueOutput: true, OutputsCount: 1 })
        {
            WalkForward(Nodes[outputNodeId]);
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

    public Node AddNode(Type nodeType)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
        addNode(node);
        Serialise();
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

        if (metadata.InputHasVariableSize || metadata.OutputHasVariableSize)
        {
            VariableSizes.Add(node.Id, new NodeVariableSize
            {
                ValueInputSize = metadata.InputVariableSize?.DefaultSize ?? 0,
                ValueOutputSize = metadata.OutputVariableSize?.DefaultSize ?? 0
            });
        }

        node.ZIndex = ZIndex++;
        node.NodeField = this;

        Nodes.Add(node.Id, node);
    }

    private record FlowTask(Task Task, CancellationTokenSource Source);

    private readonly NodeFieldMemory memory = new();
    private readonly Dictionary<Node, FlowTask> tasks = [];

    public void ParameterReceived(ReceivedParameter parameter)
    {
        if (!running) return;

        foreach (var node in Nodes.Values.Where(node => node.GetType().IsAssignableTo(typeof(IParameterReceiver))))
        {
            StartFlow(node, new ParameterReceiverFlowContext(parameter));
        }
    }

    public void StartFlow(Node triggerNode, FlowContext? context = null) => Task.Run(async () =>
    {
        context ??= new FlowContext();

        if (tasks.TryGetValue(triggerNode, out var existingTask))
        {
            await existingTask.Source.CancelAsync();
            memory.Reset();

            try
            {
                await existingTask.Task;
            }
            catch (TaskCanceledException)
            {
                // ignore it
            }

            tasks.Remove(triggerNode);
        }

        var task = Task.Run(() => executeFlowNode(triggerNode, context), context.Token).ContinueWith(_ => memory.Reset());
        tasks.Add(triggerNode, new FlowTask(task, context.Source));
    });

    public async Task TriggerOutputFlow(Node sourceNode, FlowContext context, int flowSlot, bool shouldScope)
    {
        if (context.Token.IsCancellationRequested) return;

        var connection = Connections.FirstOrDefault(con => con.OutputNodeId == sourceNode.Id && con.OutputSlot == flowSlot && con.ConnectionType == ConnectionType.Flow);
        if (connection is null) return;

        if (shouldScope)
        {
            memory.Push();
        }

        var destNode = Nodes[connection.InputNodeId];

        await executeFlowNode(destNode, context);

        if (shouldScope)
        {
            memory.Pop();
        }
    }

    public record WalkResult(Node ValueOutputNode, Node TriggerNode, int ValueOutputSlot);

    /// <summary>
    /// Walks forward from a source node with a single output to find the value nodes directly before any trigger node, and the trigger node
    /// </summary>
    public void WalkForward(Node sourceNode)
    {
        var results = new List<WalkResult>();
        walkForward(results, sourceNode, 0);

        var beforeValues = new Dictionary<Node, object?>();
        var afterValues = new Dictionary<Node, object?>();

        foreach (var walkResult in results)
        {
            if (beforeValues.ContainsKey(walkResult.ValueOutputNode)) continue;

            if (!memory.HasEntry(walkResult.ValueOutputNode.Id))
            {
                memory.CreateEntry(walkResult.ValueOutputNode);
                beforeValues.Add(walkResult.ValueOutputNode, walkResult.ValueOutputNode.Metadata.Outputs[walkResult.ValueOutputSlot].Type.CreateDefault());
            }
            else
            {
                beforeValues.Add(walkResult.ValueOutputNode, memory.Read(walkResult.ValueOutputNode.Id).Values[walkResult.ValueOutputSlot].GetValue());
            }

            executeValueNode(walkResult.ValueOutputNode);
            afterValues.Add(walkResult.ValueOutputNode, memory.Read(walkResult.ValueOutputNode.Id).Values[walkResult.ValueOutputSlot].GetValue());
        }

        foreach (var walkResult in results)
        {
            if (beforeValues[walkResult.ValueOutputNode] != afterValues[walkResult.ValueOutputNode])
            {
                StartFlow(walkResult.TriggerNode);
            }
        }
    }

    private void walkForward(List<WalkResult> results, Node sourceNode, int outputValueSlot)
    {
        var connections = Connections.Where(c => c.ConnectionType == ConnectionType.Value && c.OutputNodeId == sourceNode.Id && c.OutputSlot == outputValueSlot).ToList();
        if (connections.Count == 0) return;

        foreach (var connection in connections)
        {
            var inputNode = Nodes[connection.InputNodeId];

            if (inputNode.Metadata.IsTrigger && inputNode.Metadata.Inputs[connection.InputSlot].IsReactive)
            {
                results.Add(new(sourceNode, inputNode, outputValueSlot));
                continue;
            }

            for (var i = 0; i < inputNode.Metadata.OutputsCount; i++)
            {
                walkForward(results, inputNode, i);
            }
        }
    }

    private IRef getConnectedRef(Node inputNode, int inputSlot, Type inputType)
    {
        var connection = Connections.FirstOrDefault(con => con.InputNodeId == inputNode.Id && con.InputSlot == inputSlot && con.ConnectionType == ConnectionType.Value);
        if (connection is null) return (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(inputType), args: [inputType.CreateDefault()])!;

        var outputNode = Nodes[connection.OutputNodeId];

        if (memory.HasEntry(outputNode.Id))
        {
            return memory.Read(outputNode.Id).Values[connection.OutputSlot];
        }

        if (outputNode.Metadata is { IsValue: true, IsFlow: false })
        {
            executeValueNode(outputNode);
            return memory.Read(outputNode.Id).Values[connection.OutputSlot];
        }

        return (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(inputType), args: [inputType.CreateDefault()])!;
    }

    private object? getConnectedValue(Node inputNode, int inputSlot, Type inputType)
    {
        var metadata = inputNode.Metadata;

        if (metadata.InputHasVariableSize && metadata.InputsCount - 1 == inputSlot)
        {
            var arrSize = inputNode.VariableSize.ValueInputSize;
            var elementType = inputType.GetElementType()!;
            var values = Array.CreateInstance(elementType, arrSize);

            for (var i = 0; i < arrSize; i++)
            {
                values.SetValue(getConnectedRef(inputNode, inputSlot + i, elementType).GetValue(), i);
            }

            return values;
        }

        var connection = Connections.FirstOrDefault(con => con.InputNodeId == inputNode.Id && con.InputSlot == inputSlot && con.ConnectionType == ConnectionType.Value);
        if (connection is null) return inputType.CreateDefault();

        if (!memory.HasEntry(connection.OutputNodeId))
            executeValueNode(Nodes[connection.OutputNodeId]);

        var outputMetadata = Nodes[connection.OutputNodeId].Metadata;

        if (connection.OutputSlot >= outputMetadata.OutputsCount - 1 && outputMetadata.OutputHasVariableSize)
        {
            var arr = (Array)memory.Read(connection.OutputNodeId).Values[outputMetadata.OutputsCount - 1].GetValue()!;
            return arr.GetValue(connection.OutputSlot - (outputMetadata.OutputsCount - 1));
        }

        return getConnectedRef(inputNode, inputSlot, inputType).GetValue();
    }

    private async Task executeFlowNode(Node node, FlowContext context)
    {
        // TODO: Check if node has already been executed and refuse

        Logger.Log("Executing " + node.GetType().GetFriendlyName());

        var metadata = GetMetadata(node);

        var args = new List<object?>
        {
            context
        };

        if (metadata.InputsCount > 0)
        {
            var inputValues = new object?[metadata.InputsCount];

            for (var i = 0; i < metadata.InputsCount; i++)
            {
                inputValues[i] = getConnectedValue(node, i, metadata.Inputs[i].Type);
            }

            args.AddRange(inputValues);
        }

        if (metadata.OutputsCount > 0)
        {
            if (!memory.HasEntry(node.Id))
                memory.CreateEntry(node);

            var entry = memory.Read(node.Id);
            args.AddRange(entry.Values);
        }

        var argsArr = args.ToArray();

        var task = (Task)metadata.ProcessMethod.Invoke(node, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, argsArr, null)!;

        try
        {
            await task;
        }
        catch (TaskCanceledException)
        {
            // ignore it
        }
    }

    private void executeValueNode(Node node)
    {
        var metadata = GetMetadata(node);

        var args = new List<object?>();

        if (metadata.InputsCount > 0)
        {
            var inputValues = new object?[metadata.InputsCount];

            for (var i = 0; i < metadata.InputsCount; i++)
            {
                inputValues[i] = getConnectedValue(node, i, metadata.Inputs[i].Type);
            }

            args.AddRange(inputValues);
        }

        if (metadata.OutputsCount > 0)
        {
            if (!memory.HasEntry(node.Id))
                memory.CreateEntry(node);

            var entry = memory.Read(node.Id);
            args.AddRange(entry.Values);
        }

        var argsArr = args.ToArray();
        metadata.ProcessMethod.Invoke(node, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, argsArr, null);
    }

    public NodeGroup AddGroup()
    {
        var nodeGroup = new NodeGroup();
        Groups.Add(nodeGroup);
        return nodeGroup;
    }

    internal async Task TriggerImpulse(FlowContext context, string impulseName)
    {
        foreach (var receiveImpulseNode in Nodes.Values.OfType<ReceiveImpulseNode>().Where(node => !string.IsNullOrEmpty(node.ImpulseName) && node.ImpulseName == impulseName))
        {
            await executeFlowNode(receiveImpulseNode, context);
        }
    }
}

public class NodeGroup
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; set; } = "New Group";
    public ObservableCollection<Guid> Nodes { get; } = [];
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