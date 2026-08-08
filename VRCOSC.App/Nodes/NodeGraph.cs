// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MeaMod.DNS.Server;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.ChatBox.Clips.Variables.Instances;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Serialisation.V1;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Events;
using VRCOSC.App.Nodes.Types.Strings;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;
using Node = VRCOSC.App.Nodes.Types.Node;

namespace VRCOSC.App.Nodes;

public class NodeGraph : INotifyPropertyChanged
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

    public readonly bool FromImport;

    public NodeGraph(bool fromImport = false)
    {
        FromImport = fromImport;
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodeGraphSerialiserV1(AppManager.GetInstance().Storage, this));
        serialiser.RegisterSerialiser(2, new NodeGraphSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load(string importPath = "")
    {
        if (string.IsNullOrEmpty(importPath))
            serialiser.Deserialise();
        else
            serialiser.Deserialise(false, importPath);

        Enabled.Subscribe(Serialise);
    }

    public void Serialise()
    {
        Logger.Log($"Serialising graph with {Elements.Count} elements", LoggingTarget.Information);
        serialiser.Serialise();
    }

    public async Task Start()
    {
        Running.Value = true;
        CurrentSpeechText = null;
        cachedValueOutputs.Clear();

        cacheNodeTypes();
        await triggerOnStartNodes();
        await processAllTriggerNodes();
        updateStopwatch = new Stopwatch();
    }

    public async Task Stop()
    {
        if (!Running.Value) return;

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
        GraphVariables.ForEach(v => v.Value.Reset());
        HighestUpdateTime = TimeSpan.Zero;
        LowestUpdateTime = TimeSpan.Zero;
        CurrentUpdateTime = TimeSpan.Zero;
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
        cachedPaths.Clear();
        cachedBacktracks.Clear();
        cacheNodeTypes();

        if (OnMarkedDirty is not null)
            await OnMarkedDirty.Invoke(graphChanges);

        graphChanges = new();
        Serialise();
    }

    private void cacheNodeTypes()
    {
        var nodes = Elements.Values.OfType<INode>().ToArray();
        continuousNodes = nodes.OfType<IContinuousNode>().OrderBy(node => node.UpdateOffset).Cast<INode>().ToArray();
        activeUpdateNodes = nodes.OfType<IActiveUpdateNode>().OrderBy(node => node.UpdateOffset).Cast<INode>().ToArray();
        updateNodes = nodes.OfType<IUpdateNode>().OrderBy(node => node.UpdateOffset).Cast<INode>().ToArray();
        startNodes = nodes.OfType<OnStartNode>().ToArray<INode>();
        stopNodes = nodes.OfType<OnStopNode>().ToArray<INode>();
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

        var moduleNodeInterface = type.GetInterfaces().SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModuleNode<>));

        if (moduleNodeInterface is not null)
        {
            var moduleType = moduleNodeInterface.GetGenericArguments()[0];
            var module = ModuleManager.GetInstance().GetModuleInstanceFromType(moduleType);
            type.GetProperty("Module")!.SetValue(node, module);
        }

        return node;
    }

    public bool RemoveNode(Guid id)
    {
        var nodeResult = GetNode(id);
        if (!nodeResult.IsSuccess) return false;

        if (!Elements.Remove(id, out var element)) return false;

        var node = (INode)element;

        var removedInputConnections = Connections.RemoveIf(c => c.OutputId == id);
        var removedOutputConnections = Connections.RemoveIf(c => c.InputId == id);

        foreach (var inputConnection in removedInputConnections)
        {
            var inputNodeResult = GetNode(inputConnection.InputId);
            Debug.Assert(inputNodeResult.IsSuccess);
            var inputNode = inputNodeResult.Value;

            if (inputConnection is IValueConnection)
                inputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[inputConnection.InputSlot].IsConnected = false;
        }

        var group = Groups.Values.SingleOrDefault(g => g.Nodes.Contains(id));
        group?.Nodes.Remove(id);

        if (group?.Nodes.Count == 0 && group.Comments.Count == 0)
        {
            Groups.TryRemove(group.Id, out _);
            graphChanges.RemovedGroups.Add(group);
        }

        graphChanges.RemovedConnections.AddRange(removedInputConnections);
        graphChanges.RemovedConnections.AddRange(removedOutputConnections);
        graphChanges.RemovedNodes.Add(node);
        return true;
    }

    public void RemoveComment(Guid id)
    {
        if (!Elements.TryRemove(id, out var comment)) return;

        var group = Groups.Values.SingleOrDefault(g => g.Comments.Contains(id));
        group?.Comments.Remove(id);

        if (group?.Nodes.Count == 0 && group.Comments.Count == 0)
        {
            Groups.TryRemove(group.Id, out _);
            graphChanges.RemovedGroups.Add(group);
        }

        graphChanges.RemovedComments.Add((Comment)comment);
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
        // temp casting before relays
        var outputElement = (INode)Elements[outputId];
        var inputElement = (INode)Elements[inputId];

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

            if (inputElement.Metadata.Shared.ReceivesValueUpdates)
                TriggerTree(inputElement).Forget();

            return Result<IValueConnection[]>.Success([connection]);
        }

        if (inputType == typeof(string))
        {
            var result = AddNode(typeof(ToStringNode<>).MakeGenericType(outputType));
            if (!result.IsSuccess) return result.Exception;

            var toStringNode = result.Value;
            toStringNode.Metadata.Position = (inputElement.Metadata.Position - outputElement.Metadata.Position) / new Vector2(2) + outputElement.Metadata.Position;

            var conn1Result = createValueConnection(outputId, outputSlot, outputSlotIndex, outputType, toStringNode.Id, 0, 0, outputType);
            if (!conn1Result.IsSuccess) return conn1Result.Exception;

            var conn2Result = createValueConnection(toStringNode.Id, 0, 0, inputType, inputId, inputSlot, inputSlotIndex, inputType);
            if (!conn2Result.IsSuccess) return conn2Result.Exception;

            if (inputElement.Metadata.Shared.ReceivesValueUpdates)
                TriggerTree(inputElement).Forget();

            return Result<IValueConnection[]>.Success([conn1Result.Value[0], conn2Result.Value[0]]);
        }

        if (outputType.TryCreateConverter(inputType, out _))
        {
            var result = AddNode(typeof(CastNode<,>).MakeGenericType(outputType, inputType));
            if (!result.IsSuccess) return result.Exception;

            var castNode = result.Value;
            castNode.Metadata.Position = (inputElement.Metadata.Position - outputElement.Metadata.Position) / new Vector2(2) + outputElement.Metadata.Position;

            var conn1Result = createValueConnection(outputId, outputSlot, outputSlotIndex, outputType, castNode.Id, 0, 0, outputType);
            if (!conn1Result.IsSuccess) return conn1Result.Exception;

            var conn2Result = createValueConnection(castNode.Id, 0, 0, inputType, inputId, inputSlot, inputSlotIndex, inputType);
            if (!conn2Result.IsSuccess) return conn2Result.Exception;

            if (inputElement.Metadata.Shared.ReceivesValueUpdates)
                TriggerTree(inputElement).Forget();

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

        if (!affectedNode.Metadata.Shared.IsSelfUpdating && !affectedNode.Metadata.Shared.IsFlowInput)
            TriggerTree(affectedNode).Forget();
    }

    #endregion

    public NodeGroup AddGroup(IEnumerable<Guid> initialNodes, IEnumerable<Guid> initialComments, Guid? id = null)
    {
        var nodeGroup = new NodeGroup();
        nodeGroup.Nodes.AddRange(initialNodes);
        nodeGroup.Comments.AddRange(initialComments);
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

    public void CreateVariable(Type variableType, string name, bool persistent, Guid? idOverride = null, object? initialValue = null)
    {
        IGraphVariable variable;

        if (idOverride is null)
            variable = (IGraphVariable)Activator.CreateInstance(typeof(GraphVariable<>).MakeGenericType(variableType), args: [name, persistent])!;
        else
            variable = (IGraphVariable)Activator.CreateInstance(typeof(GraphVariable<>).MakeGenericType(variableType), args: [idOverride, name, persistent, initialValue])!;

        GraphVariables.TryAdd(variable.GetId(), variable);

        if (variableType != typeof(string)) return;

        var displayName = new Observable<string>();
        variable.Name.Subscribe(newName => displayName.Value = $"Pulse {newName}", true);

        var reference = new ClipVariableReference
        {
            ModuleID = "internal.pulse",
            VariableID = variable.GetId().ToString(),
            DisplayName = variable.Name,
            ClipVariableType = typeof(PulseClipVariable),
            ValueType = typeof(string),
            Value =
            {
                // References are required to have a value to be executed, but PulseClipVariable doesn't use it
                Value = "INTERNAL"
            }
        };

        ChatBoxManager.GetInstance().CreateVariable(reference);
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
    }

    private async Task processAllTriggerNodes()
    {
        foreach (var node in Elements.Values.OfType<INode>().Where(node => node.Metadata.Shared.IsValueInputTrigger))
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

    private INode[] continuousNodes { get; set; } = [];
    private INode[] activeUpdateNodes { get; set; } = [];
    private INode[] updateNodes { get; set; } = [];
    private INode[] startNodes { get; set; } = [];
    private INode[] stopNodes { get; set; } = [];

    private Dictionary<Guid, Dictionary<int, Dictionary<int, IRef?>>> cachedValueOutputs { get; } = [];

    internal void CacheValueOutput(Guid nodeId, int slot, int index, IRef value)
    {
        cachedValueOutputs[nodeId][slot][index] = value;
    }

    internal IRef? GetCachedValueOutput(Guid nodeId, int slot, int index)
    {
        if (!cachedValueOutputs.TryGetValue(nodeId, out var nodeOutputs))
        {
            nodeOutputs = new Dictionary<int, Dictionary<int, IRef?>>();
            cachedValueOutputs[nodeId] = nodeOutputs;
        }

        if (!nodeOutputs.TryGetValue(slot, out var nodeOutputsSlot))
        {
            nodeOutputsSlot = new Dictionary<int, IRef?>();
            nodeOutputs[slot] = nodeOutputsSlot;
        }

        if (!nodeOutputsSlot.TryGetValue(index, out var value))
        {
            nodeOutputsSlot[index] = value;
        }

        return value;
    }

    public TimeSpan HighestUpdateTime
    {
        get;
        set
        {
            if (value.Equals(field)) return;

            field = value;
            OnPropertyChanged();
        }
    } = TimeSpan.Zero;

    public TimeSpan LowestUpdateTime
    {
        get;
        set
        {
            if (value.Equals(field)) return;

            field = value;
            OnPropertyChanged();
        }
    } = TimeSpan.Zero;

    public TimeSpan CurrentUpdateTime
    {
        get;
        private set
        {
            if (value.Equals(field)) return;

            field = value;
            OnPropertyChanged();
        }
    } = TimeSpan.Zero;

    private long updateCount;
    private TimeSpan recentUpdateTotal = TimeSpan.Zero;
    private const int fps_update_count = 10;
    private const int fps_extra_update_count = 250;
    private Stopwatch updateStopwatch = null!;

    public async Task Update()
    {
        Debug.Assert(Running.Value);

        var targetUpdateDelay = NodeManager.UPDATE_DELAY.TotalMilliseconds;
        updateStopwatch.Restart();

        foreach (var node in continuousNodes)
        {
            var c = new PulseContext(this)
            {
                DeltaTimeInternal = targetUpdateDelay
            };

            await TriggerTree(node, c, null, postC =>
            {
                var isDirty = false;

                foreach (var valueOutputMetadata in node.Metadata.Elements[ConnectionPoint.ValueOutput])
                {
                    for (var i = 0; i < valueOutputMetadata.WorkingSize; i++)
                    {
                        var value = postC.Memory[node.Id][valueOutputMetadata.Shared.Slot][i];
                        var cachedValue = GetCachedValueOutput(node.Id, valueOutputMetadata.Shared.Slot, i);

                        if (cachedValue is null || !cachedValue.Equals(value))
                            isDirty = true;

                        CacheValueOutput(node.Id, valueOutputMetadata.Shared.Slot, i, value);
                    }
                }

                return isDirty;
            });
        }

        foreach (var node in activeUpdateNodes)
        {
            var c = new PulseContext(this)
            {
                DeltaTimeInternal = targetUpdateDelay
            };
            await TriggerTree(node, c, newC => ((IActiveUpdateNode)node).OnUpdate(newC));
        }

        foreach (var node in updateNodes)
        {
            var c = new PulseContext(this)
            {
                DeltaTimeInternal = targetUpdateDelay
            };
            c.Push(node.Id);
            ((IUpdateNode)node).OnUpdate(c);
        }

        updateCount++;
        var elapsed = updateStopwatch.Elapsed;

        recentUpdateTotal += elapsed;

        if (updateCount % fps_update_count == 0)
        {
            CurrentUpdateTime = recentUpdateTotal / fps_update_count;
            recentUpdateTotal = TimeSpan.Zero;
        }

        if (updateCount % fps_extra_update_count == 0)
        {
            LowestUpdateTime = TimeSpan.Zero;
            HighestUpdateTime = TimeSpan.Zero;
        }

        if (elapsed.TotalMilliseconds < LowestUpdateTime.TotalMilliseconds || LowestUpdateTime == TimeSpan.Zero)
            LowestUpdateTime = elapsed;

        if (elapsed.TotalMilliseconds > HighestUpdateTime.TotalMilliseconds || HighestUpdateTime == TimeSpan.Zero)
            HighestUpdateTime = elapsed;
    }

    public void CreatePreset(string name, List<Guid> nodeIds, List<Guid> commentIds, float posX, float posY)
    {
        var nodePreset = new NodePreset
        {
            Name = { Value = name },
            Structure =
            {
                Nodes = nodeIds.Select(id => new SerialisableNode((Node)Elements[id])).ToList(),
                Connections = Connections.Where(c => nodeIds.Contains(c.OutputId) && nodeIds.Contains(c.InputId)).Select(c => new SerialisableConnection(c)).ToList(),
                Groups = Groups.Values.Where(g => g.Nodes.All(nodeIds.Contains)).Select(g => new SerialisableGroup(g)).ToList(),
                Variables = nodeIds.Select(id => (Node)Elements[id]).OfType<IHasVariableReference>().Select(node => new SerialisableGraphVariable(GraphVariables[node.VariableId])).ToList(),
                Comments = commentIds.Select(id => new SerialisableComment((Comment)Elements[id])).ToList()
            }
        };

        foreach (var node in nodePreset.Structure.Nodes)
        {
            node.Position = new Vector2(node.Position.X - posX, node.Position.Y - posY);
        }

        foreach (var comment in nodePreset.Structure.Comments)
        {
            comment.Position = new Vector2(comment.Position.X - posX, comment.Position.Y - posY);
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
        var currentId = c.Peek();
        GlobalStores.TryAdd(currentId, new Dictionary<IStore, IRef>());
        GlobalStores[currentId][globalStore] = new Ref<T>(value);
    }

    public T ReadStore<T>(IGlobalStore<T> globalStore, PulseContext c)
    {
        var currentId = c.Peek();

        if (!GlobalStores.TryGetValue(currentId, out var nodeStore))
        {
            var value = new Dictionary<IStore, IRef>();
            GlobalStores.TryAdd(currentId, value);
            nodeStore = value;
        }

        if (!nodeStore.TryGetValue(globalStore, out var iRef))
            return globalStore.DefaultValue;

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

        var c = baseContext is null ? new PulseContext(this) : new PulseContext(baseContext, this);

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
        c.Push(node.Id);
        return node.IShouldProcess(c);
    }

    private async Task<bool> processNode(INode node, PulseContext c, Func<PulseContext, Task<bool>>? onPreProcess = null, Func<PulseContext, bool>? onPostProcess = null)
    {
        if (c.IsCancelled) return false;
        if (c.HasMemory(node.Id) && !node.Metadata.Shared.Reprocess) return false;

        c.CreateMemory(node);
        await backtrackNode(node, c);
        if (c.IsCancelled) return false;

        c.Push(node.Id);
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

    private readonly Dictionary<Guid, INode[]> cachedBacktracks = [];

    private async Task backtrackNode(INode node, PulseContext c)
    {
        if (cachedBacktracks.TryGetValue(node.Id, out var nodes))
        {
            foreach (var outputNode in nodes)
            {
                if (outputNode.Metadata.Shared.IsFlow)
                {
                    c.CreateMemory(outputNode);
                    continue;
                }

                await processNode(outputNode, c);
            }
        }
        else
        {
            var metadata = node.Metadata;
            var slotElements = metadata.Elements[ConnectionPoint.ValueInput];

            var backtrackList = new List<INode>();

            for (var slot = 0; slot < metadata.Shared.ValueInputCount; slot++)
            {
                var slotMetadata = slotElements[slot];

                for (var index = 0; index < slotMetadata.WorkingSize; index++)
                {
                    var connectionResult = FindConnectionFromValueInput(node.Id, slot, index);
                    if (!connectionResult.IsSuccess) continue;

                    var outputNodeResult = getGraphElement<Node>(connectionResult.Value.OutputId);
                    if (!outputNodeResult.IsSuccess) throw outputNodeResult.Exception;

                    var outputNode = outputNodeResult.Value;
                    backtrackList.Add(outputNode);

                    if (outputNode.Metadata.Shared.IsFlow)
                    {
                        c.CreateMemory(outputNode);
                        continue;
                    }

                    await processNode(outputNode, c);
                }
            }

            cachedBacktracks[node.Id] = backtrackList.ToArray();
        }
    }

    private readonly Dictionary<Guid, List<INode[]>> cachedPaths = [];

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

        var hasProcessed = await processNode(sourceNode, c, onPreProcess, onPostProcess);
        if (!hasProcessed) return;

        var pathList = new List<INode[]>();

        if (cachedPaths.TryGetValue(sourceNode.Id, out var localPathList))
        {
            pathList = localPathList;
        }
        else
        {
            var currPath = new Stack<INode>();

            for (var slot = 0; slot < sourceNode.Metadata.Shared.ValueOutputCount; slot++)
            {
                var valueOutput = sourceNode.Metadata.Elements[ConnectionPoint.ValueOutput][slot];

                for (var index = 0; index < valueOutput.WorkingSize; index++)
                {
                    await walkForward(pathList, currPath, sourceNode, slot, index);
                }
            }

            cachedPaths[sourceNode.Id] = pathList;
        }

        // we want to process every non-trigger node before processing the trigger nodes so that
        // every non-trigger node in the tree only runs once, even if non-trigger node isn't part
        // of the current path but will be down one of the trigger node's flows

        foreach (var path in pathList)
        {
            if (path.Length == 1) continue;

            // traverse backwards as ToArray on a stack reverses the order
            for (var i = path.Length - 1; i > 0; i--)
            {
                var node = path[i];
                await processNode(node, c);
            }
        }

        foreach (var node in pathList.Select(path => path.First()).DistinctBy(node => node.Id))
        {
            await StartFlow(node, c);
        }
    }

    private async Task walkForward(List<INode[]> triggerStacks, Stack<INode> pathStack, INode currentNode, int outputSlot, int outputIndex)
    {
        var connections = Connections.Where(con => con is IValueConnection && con.OutputId == currentNode.Id && con.OutputSlot == outputSlot && con.OutputSlotIndex == outputIndex);

        foreach (var connection in connections)
        {
            var inputNode = (INode)Elements[connection.InputId];

            if (pathStack.Contains(inputNode)) continue;
            if (!inputNode.Metadata.Shared.ReceivesValueUpdates) continue;

            pathStack.Push(inputNode);

            if (inputNode.Metadata.Shared.IsAnyTrigger)
            {
                triggerStacks.Add(pathStack.ToArray());
                continue;
            }

            for (var slot = 0; slot < inputNode.Metadata.Shared.ValueOutputCount; slot++)
            {
                var valueOutput = inputNode.Metadata.Elements[ConnectionPoint.ValueOutput][slot];

                for (var index = 0; index < valueOutput.WorkingSize; index++)
                {
                    await walkForward(triggerStacks, pathStack, inputNode, slot, index);
                }
            }

            pathStack.Pop();
        }
    }

    public async Task TriggerImpulse(ImpulseDefinition definition, IPulseContext c)
    {
        foreach (var receiveNode in Elements.Values.Where(node => node.GetType().IsAssignableTo(typeof(IImpulseReceiver))).Cast<INode>())
        {
            var receiveNodeAsImpulse = (IImpulseReceiver)receiveNode;
            if (!receiveNodeAsImpulse.CanReceive(definition.Name, c)) continue;

            var type = receiveNode.GetType();

            if (definition.Values.Length == 0 && type.IsGenericType) continue;

            if (definition.Values.Length != 0)
            {
                if (!type.IsGenericType) continue;
                if (!type.GenericTypeArguments.SequenceEqual(definition.Values.Select(o => o.GetType()))) continue;
            }

            var newC = new PulseContext((PulseContext)c, this);

            await processNode(receiveNode, newC, newNewC =>
            {
                receiveNodeAsImpulse.WriteOutputs(definition.Values, newNewC);
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

    public IComment AddComment(Guid? idOverride = null)
    {
        var comment = new Comment();

        if (idOverride.HasValue)
            comment.Id = idOverride.Value;

        Elements.TryAdd(comment.Id, comment);
        graphChanges.AddedComments.Add(comment);
        return comment;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}