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
using MeaMod.DNS.Server;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Events;
using VRCOSC.App.Nodes.Types.Strings;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;
using Node = VRCOSC.App.Nodes.Types.Node;

namespace VRCOSC.App.Nodes;

public class NodeGraph
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("New Graph");
    public Observable<bool> Selected { get; } = new();
    public Observable<bool> Enabled { get; } = new(true);
    public Observable<bool> Running { get; } = new();

    private readonly SerialisationManager serialiser;

    public ConcurrentDictionary<Guid, IGraphElement> Elements { get; } = [];
    public ConcurrentSet<IConnection> Connections { get; } = [];
    public ConcurrentDictionary<Guid, IGraphVariable> GraphVariables { get; } = [];
    public ConcurrentDictionary<Guid, NodeGroup> Groups { get; } = [];

    public readonly ConcurrentDictionary<Guid, Dictionary<IStore, IRef>> GlobalStores = [];

    private GraphChanges graphChanges = new();

    public Func<GraphChanges, Task>? OnMarkedDirty;
    public bool UILoaded { get; set; }

    public string? CurrentSpeechText { get; private set; }

    public NodeGraph()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodeGraphSerialiser(AppManager.GetInstance().Storage, this));
        serialiser.RegisterSerialiser(2, new NodeGraphSerialiserV2(AppManager.GetInstance().Storage, this));
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
        Logger.Log("Serialising graph");
        serialiser.Serialise();
    }

    public async Task Start()
    {
        Running.Value = true;
        CurrentSpeechText = null;
        await triggerOnStartNodes();
        await processAllTriggerNodes();
        startUpdate();
    }

    public async Task Stop()
    {
        await updateTokenSource!.CancelAsync();
        await updateTask!;

        try
        {
            await Task.WhenAll(cancelTasks.Values.Concat(tasks.Values).Select(t => t.Context.Source.CancelAsync()));
            await Task.WhenAll(cancelTasks.Values.Concat(tasks.Values).Select(t => t.Task));
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }

        await triggerOnStopNodes();
        clearDisplayNodes();

        cancelTasks.Clear();
        GlobalStores.Clear();
        continuousOutputs.Clear();
        GraphVariables.ForEach(v => v.Value.Reset());
        Running.Value = false;

        Serialise();
    }

    #region Management

    public Task MarkDirty() => MarkDirtyAsync();

    public IEnumerable<IConnection> GetConnectionsForNode(Guid id)
    {
        return Connections.Where(c => c.InputId == id || c.OutputId == id);
    }

    public async Task MarkDirtyAsync()
    {
        if (OnMarkedDirty is not null)
            await OnMarkedDirty.Invoke(graphChanges);

        graphChanges = new();
        Serialise();
    }

    public Result<INode> AddNode(Type type, Guid? idOverride = null, Vector2 position = default, object?[]? args = null)
    {
        var nodeResult = type.InstanceAs<Node>(args);
        if (!nodeResult.IsSuccess) return nodeResult.Exception;

        var id = idOverride ?? Guid.NewGuid();

        var node = nodeResult.Value;
        node.Id = id;
        node.Init(this);

        var metadataResult = NodeMetadataManager.GetFor(node);
        if (!metadataResult.IsSuccess) return metadataResult.Exception;

        node.Metadata.Position = position;

        var existingNodeResult = GetNode(id);
        if (existingNodeResult.IsSuccess) return new InvalidOperationException($"{nameof(INode)} with ID {node.Id} already exists", existingNodeResult.Exception);

        Elements[id] = node;
        graphChanges.AddedNodes.Add(node);

        return node;
    }

    public virtual bool RemoveNode(Guid id)
    {
        var nodeResult = GetNode(id);
        if (!nodeResult.IsSuccess) return false;

        if (!Elements.Remove(id, out var element)) return false;

        var node = (INode)element;

        var inputConnections = Connections.RemoveIf(c => c.OutputId == id);
        var outputConnections = Connections.RemoveIf(c => c.InputId == id);

        foreach (var inputConnection in inputConnections)
        {
            var inputNodeResult = GetNode(inputConnection.InputId);
            Debug.Assert(inputNodeResult.IsSuccess);
            var inputNode = inputNodeResult.Value;
            inputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[inputConnection.InputSlot].IsConnected = false;
        }

        graphChanges.RemovedConnections.AddRange(inputConnections);
        graphChanges.RemovedConnections.AddRange(outputConnections);
        graphChanges.RemovedNodes.Add(node);
        return true;
    }

    #region Connections

    private Result<IFlowConnection> createFlowConnection(Guid outputId, int outputSlot, int outputSlotIndex, Guid inputId, int inputSlot, int inputSlotIndex)
    {
        // If there's already a flow connection with this flow output slot, remove it
        var outputSlotExistingConnection = Connections.SingleOrDefault(c => c is IFlowConnection && c.OutputId == outputId && c.OutputSlot == outputSlot && c.OutputSlotIndex == outputSlotIndex);

        if (outputSlotExistingConnection is not null)
        {
            Connections.Remove(outputSlotExistingConnection);
            graphChanges.RemovedConnections.Add(outputSlotExistingConnection);
        }

        var connection = new FlowConnection(outputId, outputSlot, outputSlotIndex, inputId, inputSlot, inputSlotIndex);
        Connections.Add(connection);
        return connection;
    }

    private Result<IValueConnection[]> createValueConnection(Guid outputId, int outputSlot, int outputSlotIndex, Type outputType, Guid inputId, int inputSlot, int inputSlotIndex, Type inputType)
    {
        var outputElement = Elements[outputId];
        var inputElement = Elements[inputId];

        // If there's already a value connection with this value input slot, remove it
        var inputSlotExistingConnection = Connections.SingleOrDefault(c => c is IValueConnection && c.InputId == inputId && c.InputSlot == inputSlot && c.InputSlotIndex == inputSlotIndex);

        if (outputType.IsAssignableTo(inputType))
        {
            var connection = new ValueConnection(outputId, outputSlot, outputSlotIndex, outputType, inputId, inputSlot, inputSlotIndex, inputType);

            if (inputSlotExistingConnection is not null)
            {
                Connections.Remove(inputSlotExistingConnection);
                graphChanges.RemovedConnections.Add(inputSlotExistingConnection);
            }

            Connections.Add(connection);

            if (inputElement is INode inputNode && !inputNode.Metadata.Shared.IsFlow)
            {
                _ = TriggerTree(inputNode);
            }

            return Result<IValueConnection[]>.Success([connection]);
        }

        if (inputType == typeof(string))
        {
            var result = AddNode(typeof(ToStringNode<>).MakeGenericType(outputType));
            if (!result.IsSuccess) return result.Exception;

            var castNode = result.Value;

            var conn1Result = createValueConnection(outputId, outputSlot, outputSlotIndex, outputType, castNode.Id, 0, 0, outputType);
            if (!conn1Result.IsSuccess) return conn1Result.Exception;

            var conn2Result = createValueConnection(castNode.Id, 0, 0, inputType, inputId, inputSlot, inputSlotIndex, inputType);
            if (!conn2Result.IsSuccess) return conn2Result.Exception;

            if (inputElement is INode inputNode && !inputNode.Metadata.Shared.IsFlow)
            {
                _ = TriggerTree(inputNode);
            }

            return Result<IValueConnection[]>.Success([conn1Result.Value[0], conn2Result.Value[0]]);
        }

        if (outputType.TryCreateConverter(inputType, out _))
        {
            var result = AddNode(typeof(CastNode<,>).MakeGenericType(outputType, inputType));
            if (!result.IsSuccess) return result.Exception;

            var castNode = result.Value;

            var conn1Result = createValueConnection(outputId, outputSlot, outputSlotIndex, outputType, castNode.Id, 0, 0, outputType);
            if (!conn1Result.IsSuccess) return conn1Result.Exception;

            var conn2Result = createValueConnection(castNode.Id, 0, 0, inputType, inputId, inputSlot, inputSlotIndex, inputType);
            if (!conn2Result.IsSuccess) return conn2Result.Exception;

            if (inputElement is INode inputNode && !inputNode.Metadata.Shared.IsFlow)
            {
                _ = TriggerTree(inputNode);
            }

            return Result<IValueConnection[]>.Success([conn1Result.Value[0], conn2Result.Value[0]]);
        }

        return new Exception("Whoops");
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowOutput output, IFlowInput input)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var result = createFlowConnection(output.Owner.Id, output.Metadata.Shared.Slot, 0, input.Owner.Id, input.Metadata.Shared.Slot, 0);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        output.IsConnected = true;
        input.IsConnected = true;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowOutputList output, int outputIndex, IFlowInput input)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var outputSize = output.Metadata.Size;

        if (outputIndex >= outputSize)
            return new Exception($"{nameof(outputIndex)} is too large for size of {nameof(output)}");

        var result = createFlowConnection(output.Owner.Id, output.Metadata.Shared.Slot, outputIndex, input.Owner.Id, input.Metadata.Shared.Slot, 0);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        input.IsConnected = true;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowOutput output, IFlowInputList input, int inputIndex)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var inputSize = input.Metadata.Size;

        if (inputIndex >= inputSize)
            return new Exception($"{nameof(inputIndex)} is too large for size of {nameof(input)}");

        var result = createFlowConnection(output.Owner.Id, output.Metadata.Shared.Slot, 0, input.Owner.Id, input.Metadata.Shared.Slot, inputIndex);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        output.IsConnected = true;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowOutputList output, int outputIndex, IFlowInputList input, int inputIndex)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var outputSize = output.Metadata.Size;
        var inputSize = input.Metadata.Size;

        if (outputIndex >= outputSize)
            return new Exception($"{nameof(outputIndex)} is too large for size of {nameof(output)}");

        if (inputIndex >= inputSize)
            return new Exception($"{nameof(inputIndex)} is too large for size of {nameof(input)}");

        var result = createFlowConnection(output.Owner.Id, output.Metadata.Shared.Slot, outputIndex, input.Owner.Id, input.Metadata.Shared.Slot, inputIndex);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IValueConnection[]> CreateConnection(IValueOutput output, IValueInput input)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var result = createValueConnection(output.Owner.Id, output.Metadata.Shared.Slot, 0, output.Metadata.Shared.ValueType, input.Owner.Id, input.Metadata.Shared.Slot, 0, input.Metadata.Shared.ValueType);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        output.IsConnected = true;
        input.IsConnected = true;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueOutputList output, int outputIndex, IValueInput input)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var outputSize = output.Metadata.Size;

        if (outputIndex >= outputSize)
            return new Exception($"{nameof(outputIndex)} is too large for size of {nameof(output)}");

        var result = createValueConnection(output.Owner.Id, output.Metadata.Shared.Slot, outputIndex, output.Metadata.Shared.ValueType, input.Owner.Id, input.Metadata.Shared.Slot, 0,
            input.Metadata.Shared.ValueType);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        input.IsConnected = true;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueOutput output, IValueInputList input, int inputIndex)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var inputSize = input.Metadata.Size;

        if (inputIndex >= inputSize)
            return new Exception($"{nameof(inputIndex)} is too large for size of {nameof(input)}");

        var result = createValueConnection(output.Owner.Id, output.Metadata.Shared.Slot, 0, output.Metadata.Shared.ValueType, input.Owner.Id, input.Metadata.Shared.Slot, inputIndex, input.Metadata.Shared.ValueType);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        output.IsConnected = true;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueOutputList output, int outputIndex, IValueInputList input, int inputIndex)
    {
        if (output.Owner == input.Owner)
            return new Exception("Cannot create a connection to the same node");

        var outputSize = output.Metadata.Size;
        var inputSize = input.Metadata.Size;

        if (outputIndex >= outputSize)
            return new Exception($"{nameof(outputIndex)} is too large for size of {nameof(output)}");

        if (inputIndex >= inputSize)
            return new Exception($"{nameof(inputIndex)} is too large for size of {nameof(input)}");

        var result = createValueConnection(output.Owner.Id, output.Metadata.Shared.Slot, outputIndex, output.Metadata.Shared.ValueType, input.Owner.Id, input.Metadata.Shared.Slot, inputIndex, input.Metadata.Shared.ValueType);
        if (!result.IsSuccess) return result.Exception;

        var connections = result.Value;

        graphChanges.AddedConnections.AddRange(connections);
        return Result<IValueConnection[]>.Success(connections);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowRelay output, IFlowRelay input)
    {
        if (output == input)
            return new Exception("Cannot create a connection to the same relay");

        var result = createFlowConnection(output.Id, 0, 0, input.Id, 0, 0);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowOutput output, IFlowRelay input)
    {
        var result = createFlowConnection(output.Owner.Id, output.Metadata.Shared.Slot, 0, input.Id, 0, 0);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        output.IsConnected = true;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowRelay output, IFlowInput input)
    {
        var result = createFlowConnection(output.Id, 0, 0, input.Owner.Id, input.Metadata.Shared.Slot, 0);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        input.IsConnected = true;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowOutputList output, int outputIndex, IFlowRelay input)
    {
        var outputSize = output.Metadata.Size;

        if (outputIndex >= outputSize)
            return new Exception($"{nameof(outputIndex)} is too large for size of {nameof(output)}");

        var result = createFlowConnection(output.Owner.Id, output.Metadata.Shared.Slot, outputIndex, input.Id, 0, 0);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IFlowConnection[]> CreateConnection(IFlowRelay output, IFlowInputList input, int inputIndex)
    {
        var inputSize = input.Metadata.Size;

        if (inputIndex >= inputSize)
            return new Exception($"{nameof(inputIndex)} is too large for size of {nameof(input)}");

        var result = createFlowConnection(output.Id, 0, 0, input.Owner.Id, input.Metadata.Shared.Slot, inputIndex);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.Add(connection);
        return Result<IFlowConnection[]>.Success([connection]);
    }

    public Result<IValueConnection[]> CreateConnection(IValueRelay output, IValueRelay input)
    {
        if (output == input)
            return new Exception("Cannot create a connection to the same relay");

        var result = createValueConnection(output.Id, 0, 0, output.Type, input.Id, 0, 0, input.Type);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueOutput output, IValueRelay input)
    {
        var result = createValueConnection(output.Owner.Id, output.Metadata.Shared.Slot, 0, output.Metadata.Shared.ValueType, input.Id, 0, 0, input.Type);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        output.IsConnected = true;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueRelay output, IValueInput input)
    {
        var result = createValueConnection(output.Id, 0, 0, output.Type, input.Owner.Id, input.Metadata.Shared.Slot, 0, input.Metadata.Shared.ValueType);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        input.IsConnected = true;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueOutputList output, int outputIndex, IValueRelay input)
    {
        var outputSize = output.Metadata.Size;

        if (outputIndex >= outputSize)
            return new Exception($"{nameof(outputIndex)} is too large for size of {nameof(output)}");

        var result = createValueConnection(output.Owner.Id, output.Metadata.Shared.Slot, outputIndex, output.Metadata.Shared.ValueType, input.Id, 0, 0, input.Type);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IValueConnection[]> CreateConnection(IValueRelay output, IValueInputList input, int inputIndex)
    {
        var inputSize = input.Metadata.Size;

        if (inputIndex >= inputSize)
            return new Exception($"{nameof(inputIndex)} is too large for size of {nameof(input)}");

        var result = createValueConnection(output.Id, 0, 0, output.Type, input.Owner.Id, input.Metadata.Shared.Slot, inputIndex, input.Metadata.Shared.ValueType);
        if (!result.IsSuccess) return result.Exception;

        var connection = result.Value;

        graphChanges.AddedConnections.AddRange(connection);
        return Result<IValueConnection[]>.Success(connection);
    }

    public Result<IConnection[]> CreateConnection(object output, int outputIndex, object input, int inputIndex)
    {
        {
            if (output is IFlowOutput flowOutput && input is IFlowInput flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IFlowOutputList flowOutput && input is IFlowInput flowInput) return CreateConnection(flowOutput, outputIndex, flowInput).To<IConnection[]>();
        }

        {
            if (output is IFlowOutput flowOutput && input is IFlowInputList flowInput) return CreateConnection(flowOutput, flowInput, inputIndex).To<IConnection[]>();
        }

        {
            if (output is IFlowOutputList flowOutput && input is IFlowInputList flowInput) return CreateConnection(flowOutput, outputIndex, flowInput, inputIndex).To<IConnection[]>();
        }

        {
            if (output is IValueOutput valueOutput && input is IValueInput valueInput) return CreateConnection(valueOutput, valueInput).To<IConnection[]>();
        }

        {
            if (output is IValueOutputList valueOutput && input is IValueInput valueInput) return CreateConnection(valueOutput, outputIndex, valueInput).To<IConnection[]>();
        }

        {
            if (output is IValueOutput valueOutput && input is IValueInputList valueInput) return CreateConnection(valueOutput, valueInput, inputIndex).To<IConnection[]>();
        }

        {
            if (output is IValueOutputList valueOutput && input is IValueInputList valueInput) return CreateConnection(valueOutput, outputIndex, valueInput, inputIndex).To<IConnection[]>();
        }

        {
            if (output is IFlowRelay flowOutput && input is IFlowRelay flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IFlowOutput flowOutput && input is IFlowRelay flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IFlowRelay flowOutput && input is IFlowInput flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IFlowOutputList flowOutput && input is IFlowRelay flowInput) return CreateConnection(flowOutput, outputIndex, flowInput).To<IConnection[]>();
        }

        {
            if (output is IFlowRelay flowOutput && input is IFlowInputList flowInput) return CreateConnection(flowOutput, flowInput, inputIndex).To<IConnection[]>();
        }

        {
            if (output is IValueRelay flowOutput && input is IValueRelay flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IValueOutput flowOutput && input is IValueRelay flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IValueRelay flowOutput && input is IValueInput flowInput) return CreateConnection(flowOutput, flowInput).To<IConnection[]>();
        }

        {
            if (output is IValueOutputList flowOutput && input is IValueRelay flowInput) return CreateConnection(flowOutput, outputIndex, flowInput).To<IConnection[]>();
        }

        {
            if (output is IValueRelay flowOutput && input is IValueInputList flowInput) return CreateConnection(flowOutput, flowInput, inputIndex).To<IConnection[]>();
        }

        return new Exception("Unknown connection pair");
    }

    public void RemoveConnection(IConnection connection)
    {
        Connections.Remove(connection);
        graphChanges.RemovedConnections.Add(connection);

        var affectedNode = (INode)Elements[connection.InputId];

        if (connection is IValueConnection)
            affectedNode.Metadata.Elements[ConnectionPoint.ValueInput][connection.InputSlot].Instance.IsConnected = false;

        if (connection is IFlowConnection)
            affectedNode.Metadata.Elements[ConnectionPoint.FlowInput][connection.InputSlot].Instance.IsConnected = false;

        if (!affectedNode.Metadata.Shared.IsActiveUpdate && !affectedNode.Metadata.Shared.IsFlowInput)
            TriggerTree(affectedNode).Forget();
    }

    #endregion

    public NodeGroup AddGroup(IEnumerable<Guid> initialNodes, Guid? id = null)
    {
        var nodeGroup = new NodeGroup();
        nodeGroup.Nodes.AddRange(initialNodes);
        if (id.HasValue) nodeGroup.Id = id.Value;
        Groups.TryAdd(nodeGroup.Id, nodeGroup);
        graphChanges.AddedGroups.Add(nodeGroup);
        return nodeGroup;
    }

    public void DeleteGroup(Guid id)
    {
        Groups.TryRemove(id, out var group);
        graphChanges.RemovedGroups.Add(group!);
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
        foreach (var (_, element) in Elements)
        {
            if (element is IHasVariableReference variableReferenceNode && variableReferenceNode.VariableId == variable.GetId())
            {
                RemoveNode(element.Id);
            }
        }

        GraphVariables.Remove(variable.GetId(), out _);

        Serialise();
    }

    private async Task processAllTriggerNodes()
    {
        foreach (var node in Elements.Values.OfType<INode>().Where(node => node.Metadata.Shared.IsAnyTrigger && !node.Metadata.Shared.IsActiveUpdate))
        {
            await TriggerTree(node);
        }
    }

    private void clearDisplayNodes()
    {
        foreach (var displayNode in Elements.Values.OfType<IDisplayNode>())
        {
            displayNode.Clear();
        }
    }

    private Task? updateTask;
    private CancellationTokenSource? updateTokenSource;

    private IEnumerable<INode> continuousNodes => Elements.Values.OfType<IContinuousNode>().OrderBy(node => node.UpdateOffset).Cast<INode>();
    private IEnumerable<INode> activeUpdateNodes => Elements.Values.OfType<IActiveUpdateNode>().OrderBy(node => node.UpdateOffset).Cast<INode>();
    private IEnumerable<INode> updateNodes => Elements.Values.OfType<IUpdateNode>().OrderBy(node => node.UpdateOffset).Cast<INode>();
    private IEnumerable<INode> startNodes => Elements.Values.OfType<OnStartNode>();
    private IEnumerable<INode> stopNodes => Elements.Values.OfType<OnStopNode>();

    private Dictionary<Guid, IRef[][]> continuousOutputs { get; } = [];

    private void startUpdate()
    {
        updateTokenSource = new();

        updateTask = Task.Run(async () =>
        {
            try
            {
                while (!updateTokenSource.IsCancellationRequested)
                {
                    foreach (var node in continuousNodes)
                    {
                        await TriggerTree(node, null, null, newC =>
                        {
                            var nodeMemory = newC.Memory[node.Id];

                            if (continuousOutputs.TryGetValue(node.Id, out var outputs))
                            {
                                var wasChange = false;

                                for (var i = 0; i < node.Metadata.Shared.ValueOutputCount; i++)
                                {
                                    if (!outputs[i].Equals(nodeMemory[i])) wasChange = true;
                                }

                                continuousOutputs[node.Id] = nodeMemory;
                                return wasChange;
                            }

                            continuousOutputs[node.Id] = nodeMemory;
                            return true;
                        });
                    }

                    foreach (var node in activeUpdateNodes)
                    {
                        await TriggerTree(node, null, newC => ((IActiveUpdateNode)node).OnUpdate(newC));
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
            Nodes = nodeIds.Select(id => new SerialisableNode((Node)Elements[id])).ToList(),
            Connections = Connections.Where(c => nodeIds.Contains(c.OutputId) && nodeIds.Contains(c.InputId)).Select(c => new SerialisableConnection(c)).ToList(),
            Groups = Groups.Values.Where(g => g.Nodes.All(nodeIds.Contains)).Select(g => new SerialisableNodeGroup(g)).ToList(),
            Variables = nodeIds.Select(id => (Node)Elements[id]).OfType<IHasVariableReference>().Select(node => new SerialisableGraphVariable(GraphVariables[node.VariableId])).ToList()
        };

        foreach (var node in nodePreset.Nodes)
        {
            node.Position = new Vector2(node.Position.X - posX, node.Position.Y - posY);
        }

        NodeManager.GetInstance().Presets.Add(nodePreset);
        nodePreset.Serialise();
    }

    public Result<IValueConnection> FindConnectionFromValueInput(Guid nodeId, int slot, int slotIndex)
    {
        var connection = (IValueConnection?)Connections.SingleOrDefault(c => c is IValueConnection && c.InputId == nodeId && c.InputSlot == slot && c.InputSlotIndex == slotIndex);
        return connection is not null ? Result<IValueConnection>.Success(connection) : new Exception("Connection does not exist");
    }

    public Result<IFlowConnection> FindConnectionFromFlowOutput(Guid nodeId, int slot, int slotIndex)
    {
        var connection = (IFlowConnection?)Connections.SingleOrDefault(c => c is IFlowConnection && c.OutputId == nodeId && c.OutputSlot == slot && c.OutputSlotIndex == slotIndex);
        return connection is not null ? Result<IFlowConnection>.Success(connection) : new Exception("Connection does not exist");
    }

    public Result<INode> GetNode(Guid id) => getGraphElement<INode>(id);

    private Result<T> getGraphElement<T>(Guid id) where T : IGraphElement
    {
        if (Elements.TryGetValue(id, out var graphElement))
        {
            if (!graphElement.GetType().IsAssignableTo(typeof(T)))
                return new InvalidOperationException($"{nameof(IGraphElement)} of ID {id} is not {typeof(T).Name}");

            return (T)graphElement;
        }

        return new InvalidOperationException($"{nameof(IGraphElement)} of ID {id} doesn't exist");
    }

    public void WriteStore<T>(IGlobalStore<T> globalStore, T value, PulseContext c)
    {
        var currentNode = c.Peek();

        if (!GlobalStores.ContainsKey(currentNode.Id))
            GlobalStores.TryAdd(currentNode.Id, new Dictionary<IStore, IRef>());

        GlobalStores[currentNode.Id][globalStore] = new Ref<T>(value);
    }

    public T ReadStore<T>(IGlobalStore<T> globalStore, PulseContext c)
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

    // node-id
    private readonly ConcurrentDictionary<Guid, FlowTask> cancelTasks = [];

    // flowtask-id
    private readonly ConcurrentDictionary<Guid, FlowTask> tasks = [];

    private Task triggerOnStartNodes() => Task.WhenAll(startNodes.Select(node => processNode(node, new PulseContext(this))));
    private Task triggerOnStopNodes() => Task.WhenAll(stopNodes.Select(node => processNode(node, new PulseContext(this))));

    public void OnPartialSpeechResult(string result) => CurrentSpeechText = result;
    public void OnFinalSpeechResult(string result) => CurrentSpeechText = result;

    public async Task StartFlow(INode node, PulseContext? baseContext = null, Func<PulseContext, Task<bool>>? onPreProcess = null)
    {
        Debug.Assert(node.Metadata.Shared.IsAnyTrigger);

        var c = baseContext is null ? new PulseContext(this) : new PulseContext(baseContext, this, new CancellationTokenSource());

        if (node.Metadata.Shared.IsValueInputTrigger)
        {
            await processNode(node, c, onPreProcess);
            return;
        }

        var shouldProcess = await checkShouldProcess(node, c);
        if (!shouldProcess) return;

        if (onPreProcess is not null)
        {
            var preProcessResult = await onPreProcess.Invoke(c);
            if (!preProcessResult) return;
        }

        if (cancelTasks.TryGetValue(node.Id, out var existingTask))
        {
            await existingTask.Context.Source.CancelAsync();
            await existingTask.Task;
            cancelTasks.TryRemove(node.Id, out _);
        }

        if (!node.Metadata.Shared.NoCancel)
        {
            var newTask = Task.Run(() => node.IProcess(c)).ContinueWith(__ => { cancelTasks.TryRemove(node.Id, out _); }, TaskContinuationOptions.OnlyOnRanToCompletion);

            cancelTasks.TryAdd(node.Id, new FlowTask(newTask, c));
        }
        else
        {
            var flowTaskId = Guid.NewGuid();
            var newTask = Task.Run(() => node.IProcess(c)).ContinueWith(__ => { tasks.TryRemove(flowTaskId, out _); }, TaskContinuationOptions.OnlyOnRanToCompletion);
            tasks.TryAdd(flowTaskId, new FlowTask(newTask, c));
        }
    }

    public Task ProcessNode(Guid nodeId, PulseContext c)
    {
        var nodeResult = getGraphElement<Node>(nodeId);
        if (!nodeResult.IsSuccess) throw nodeResult.Exception;

        return processNode(nodeResult.Value, c);
    }

    private async Task<bool> checkShouldProcess(INode node, PulseContext c)
    {
        c.CreateMemory(node);
        await backtrackNode(node, c);
        c.Push(node);
        return node.IShouldProcess(c);
    }

    private async Task<bool> processNode(INode node, PulseContext c, Func<PulseContext, Task<bool>>? onPreProcess = null, Func<PulseContext, bool>? onPostProcess = null)
    {
        if (c.IsCancelled) return false;
        if (c.HasMemory(node.Id) && !node.Metadata.Shared.Reprocess) return false;

        c.CreateMemory(node);
        await backtrackNode(node, c);
        if (c.IsCancelled) return false;

        c.Push(node);
        if (c.IsCancelled) return false;

        if (!node.IShouldProcess(c))
        {
            c.Pop();
            return false;
        }

        if (c.IsCancelled) return false;

        if (onPreProcess is not null)
        {
            var result = await onPreProcess.Invoke(c);

            if (!result)
            {
                c.Pop();
                return false;
            }
        }

        if (c.IsCancelled) return false;

        await node.IProcess(c);

        if (onPostProcess is not null)
        {
            var result = onPostProcess.Invoke(c);
            if (!result) return false;
        }

        c.Pop();

        return true;
    }

    private async Task backtrackNode(INode node, PulseContext c)
    {
        var metadata = node.Metadata;
        var slotElements = metadata.Elements[ConnectionPoint.ValueInput];

        for (var slot = 0; slot < metadata.Shared.ValueInputCount; slot++)
        {
            var slotMetadata = slotElements[slot];
            var size = slotMetadata.Shared.IsList ? slotMetadata.Size : 1;

            for (var slotIndex = 0; slotIndex < size; slotIndex++)
            {
                var connectionResult = FindConnectionFromValueInput(node.Id, slot, slotIndex);
                if (!connectionResult.IsSuccess) continue;

                var outputNodeResult = getGraphElement<Node>(connectionResult.Value.OutputId);
                if (!outputNodeResult.IsSuccess) throw outputNodeResult.Exception;

                var outputNode = outputNodeResult.Value;

                if (outputNode.Metadata.Shared.IsFlow && !c.HasMemory(outputNode.Id))
                {
                    c.CreateMemory(outputNode);
                    continue;
                }

                await processNode(outputNode, c);
            }
        }
    }

    /// <summary>
    /// Triggers the source node immediately if <paramref name="sourceNode"/> is a trigger node, otherwise walks forward to get all the nodes that are trigger nodes and triggers those
    /// </summary>
    public async Task TriggerTree(INode sourceNode, PulseContext? baseContext = null, Func<PulseContext, Task<bool>>? onPreProcess = null, Func<PulseContext, bool>? onPostProcess = null)
    {
        if (!Running.Value) return;

        if (sourceNode.Metadata.Shared.IsAnyTrigger)
        {
            await StartFlow(sourceNode, baseContext, onPreProcess);
            return;
        }

        var c = baseContext is null ? new PulseContext(this) : new PulseContext(baseContext, this);

        if (onPreProcess is not null || onPostProcess is not null)
        {
            var hasProcessed = await processNode(sourceNode, c, onPreProcess, onPostProcess);
            if (!hasProcessed) return;
        }

        var triggerList = new List<INode>();
        var pathStack = new Stack<INode>();

        pathStack.Push(sourceNode);
        await processNode(sourceNode, c);

        var metadata = sourceNode.Metadata;
        var slotElements = metadata.Elements[ConnectionPoint.ValueOutput];

        for (var slot = 0; slot < metadata.Shared.ValueOutputCount; slot++)
        {
            var slotMetadata = slotElements[slot];
            var size = slotMetadata.Shared.IsList ? slotMetadata.Size : 1;

            for (var slotIndex = 0; slotIndex < size; slotIndex++)
            {
                await walkForward(triggerList, pathStack, slot, slotIndex, c);
            }
        }

        foreach (var node in triggerList)
        {
            await StartFlow(node, c);
        }
    }

    private async Task walkForward(List<INode> triggerList, Stack<INode> pathStack, int outputSlot, int outputSlotIndex, PulseContext c)
    {
        var currentNode = pathStack.Peek();
        var connections = Connections.Where(con => con is IValueConnection && con.OutputId == currentNode.Id && con.OutputSlot == outputSlot && con.OutputSlotIndex == outputSlotIndex);

        foreach (var connection in connections)
        {
            var inputNodeResult = getGraphElement<Node>(connection.InputId);
            Debug.Assert(inputNodeResult.IsSuccess);

            var inputNode = inputNodeResult.Value;
            var metadata = inputNode.Metadata;

            if (pathStack.Contains(inputNode)) continue;
            if (metadata.Shared.IsFlowInput || metadata.Shared.IsContinuous) continue;

            if (metadata.Shared.IsAnyTrigger && !triggerList.Contains(inputNode))
            {
                triggerList.Add(inputNode);
                continue;
            }

            pathStack.Push(inputNode);
            await processNode(inputNode, c);

            var slotElements = metadata.Elements[ConnectionPoint.ValueOutput];

            for (var slot = 0; slot < metadata.Shared.ValueOutputCount; slot++)
            {
                var slotMetadata = slotElements[slot];
                var size = slotMetadata.Shared.IsList ? slotMetadata.Size : 1;

                for (var slotIndex = 0; slotIndex < size; slotIndex++)
                {
                    await walkForward(triggerList, pathStack, slot, slotIndex, c);
                }
            }

            pathStack.Pop();
        }
    }

    public async Task TriggerImpulse(ImpulseDefinition definition, IPulseContext c)
    {
        foreach (var node in Elements.Values.Where(node => node.GetType().IsAssignableTo(typeof(IImpulseReceiver))).Cast<INode>())
        {
            var impulseNode = (IImpulseReceiver)node;

            //if (!string.Equals(definition.Name, impulseNode.Text, StringComparison.CurrentCulture)) continue;

            var type = node.GetType();

            if (definition.Values.Length == 0 && type.IsGenericType) continue;

            if (definition.Values.Length != 0)
            {
                if (!type.IsGenericType) continue;
                if (!type.GenericTypeArguments.SequenceEqual(definition.Values.Select(o => o.GetType()))) continue;
            }

            var newC = new PulseContext((PulseContext)c, this);

            await processNode(node, newC, newNewC =>
            {
                impulseNode.WriteOutputs(definition.Values, newNewC);
                return Task.FromResult(true);
            });
        }
    }

    public async Task TriggerModuleNode(Type nodeType, object[] data)
    {
        foreach (var node in Elements.Values.Where(element => element.GetType() == nodeType && element.GetType().IsAssignableTo(typeof(IModuleNodeEventHandler))).Cast<INode>())
        {
            var handler = (IModuleNodeEventHandler)node;

            await StartFlow(node, null, async c =>
            {
                await handler.Write(data, c);
                return true;
            });
        }
    }
}