// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.SDK.Nodes.Types.Base;
using VRCOSC.App.SDK.Nodes.Types.Strings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public class NodeScape
{
    private readonly object nodesLock = new();

    public ObservableDictionary<Guid, Node> Nodes { get; } = [];
    public ObservableCollection<NodeConnection> Connections { get; } = [];
    public ObservableCollection<NodeGroup> Groups { get; } = [];

    public int ZIndex { get; set; } = 0;

    public readonly Dictionary<Type, NodeMetadata> Metadata = [];
    public readonly Dictionary<string, object?> Variables = [];

    public int UpdateCount { get; private set; }

    public void RegisterNode(Node node)
    {
        if (Metadata.ContainsKey(node.GetType())) return;

        Metadata.Add(node.GetType(), NodeMetadataBuilder.BuildFrom(node));
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
        Connections.Add(new NodeConnection(ConnectionType.Flow, outputNodeId, outputFlowSlot, inputNodeId, 0));

        if (outputAlreadyHasConnection is not null)
        {
            Connections.Remove(outputAlreadyHasConnection);
        }
    }

    public void CreateValueConnection(Guid outputNodeId, int outputValueSlot, Guid inputNodeId, int inputValueSlot)
    {
        var outputMetadata = GetMetadata(Nodes[outputNodeId]);
        var inputMetadata = GetMetadata(Nodes[inputNodeId]);

        var outputType = outputMetadata.GetTypeOfOutputSlot(outputValueSlot);
        var inputType = inputMetadata.GetTypeOfInputSlot(inputValueSlot);
        var existingConnection = Connections.FirstOrDefault(con => con.ConnectionType == ConnectionType.Value && con.InputNodeId == inputNodeId && con.InputSlot == inputValueSlot);

        var newConnectionMade = false;

        if (outputType.IsAssignableTo(inputType))
        {
            Logger.Log(
                $"Creating value connection from {Nodes[outputNodeId].GetType().GetFriendlyName()} slot {outputValueSlot} to {Nodes[inputNodeId].GetType().GetFriendlyName()} slot {inputValueSlot}");
            Connections.Add(new NodeConnection(ConnectionType.Value, outputNodeId, outputValueSlot, inputNodeId, inputValueSlot));
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
        Nodes.Remove(node.Id);
        Connections.RemoveIf(connection => connection.OutputNodeId == node.Id || connection.InputNodeId == node.Id);

        // TODO: When a ValueRelayNode is removed, bridge connections
    }

    private Node addNode(Node node)
    {
        RegisterNode(node);

        node.ZIndex = ZIndex++;
        node.NodeScape = this;

        lock (nodesLock)
        {
            Nodes.Add(node.Id, node);
        }

        return node;
    }

    public Node AddNode(Type nodeType)
    {
        var node = (Node)Activator.CreateInstance(nodeType)!;
        return addNode(node);
    }

    private readonly NodeScapeMemory memory = new();

    public void Update()
    {
        memory.Reset();

        foreach (var (_, node) in Nodes)
        {
            var metadata = GetMetadata(node);
            if (!metadata.IsTrigger) continue;

            executeNode(node);
        }

        UpdateCount++;
    }

    public void TriggerOutputFlow(Node sourceNode, int flowSlot, bool shouldScope)
    {
        var connection = Connections.FirstOrDefault(con => con.OutputNodeId == sourceNode.Id && con.OutputSlot == flowSlot && con.ConnectionType == ConnectionType.Flow);
        if (connection is null) return;

        if (shouldScope)
        {
            memory.Push();
        }

        var destNode = Nodes[connection.InputNodeId];
        executeNode(destNode);

        if (shouldScope)
        {
            memory.Pop();
        }
    }

    private IRef getConnectedRef(Node inputNode, int inputSlot, Type inputType)
    {
        var connection = Connections.FirstOrDefault(con => con.InputNodeId == inputNode.Id && con.InputSlot == inputSlot && con.ConnectionType == ConnectionType.Value);
        if (connection is null) return (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(inputType), args: [inputType.IsValueType ? Activator.CreateInstance(inputType) : null])!;

        var outputNode = Nodes[connection.OutputNodeId];

        if (memory.HasEntry(outputNode.Id))
        {
            return memory.Read(outputNode.Id).Values[connection.OutputSlot];
        }

        if (outputNode.Metadata is { IsValue: true, IsFlow: false })
        {
            executeNode(outputNode);
            return memory.Read(outputNode.Id).Values[connection.OutputSlot];
        }

        return (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(inputType), args: [inputType.IsValueType ? Activator.CreateInstance(inputType) : null])!;
    }

    private object? getConnectedValue(Node inputNode, int inputSlot, Type inputType)
    {
        var metadata = inputNode.Metadata;

        if (metadata.InputHasVariableSize && metadata.InputsCount - 1 == inputSlot)
        {
            var arrSize = metadata.InputVariableSizeActual;
            var elementType = inputType.GetElementType()!;
            var values = Array.CreateInstance(elementType, arrSize);

            for (var i = 0; i < arrSize; i++)
            {
                values.SetValue(getConnectedRef(inputNode, inputSlot + i, elementType).GetValue(), i);
            }

            return values;
        }

        var connection = Connections.FirstOrDefault(con => con.InputNodeId == inputNode.Id && con.InputSlot == inputSlot && con.ConnectionType == ConnectionType.Value);
        if (connection is null) return inputType.IsValueType ? Activator.CreateInstance(inputType) : null;

        if (!memory.HasEntry(connection.OutputNodeId))
            executeNode(Nodes[connection.OutputNodeId]);

        var outputMetadata = Nodes[connection.OutputNodeId].Metadata;

        if (connection.OutputSlot >= outputMetadata.OutputsCount - 1 && outputMetadata.OutputHasVariableSize)
        {
            var arr = (Array)memory.Read(connection.OutputNodeId).Values[outputMetadata.OutputsCount - 1].GetValue()!;
            return arr.GetValue(connection.OutputSlot - (outputMetadata.OutputsCount - 1));
        }

        return getConnectedRef(inputNode, inputSlot, inputType).GetValue();
    }

    private void executeNode(Node node)
    {
        var metadata = GetMetadata(node);

        var inputValues = new object?[metadata.InputsCount];

        for (var i = 0; i < metadata.InputsCount; i++)
        {
            inputValues[i] = getConnectedValue(node, i, metadata.Inputs[i].Type);
        }

        if (!memory.HasEntry(node.Id))
            memory.CreateEntries(node);

        var entry = memory.Read(node.Id);
        var processDelegate = metadata.ProcessDelegate;

        if (metadata is { IsValueInput: false, IsValueOutput: false }) processDelegate.DynamicInvoke(node);
        if (metadata is { IsValueInput: true, IsValueOutput: false }) processDelegate.DynamicInvoke(node, inputValues);
        if (metadata is { IsValueInput: false, IsValueOutput: true }) processDelegate.DynamicInvoke(node, entry.Values);
        if (metadata is { IsValueInput: true, IsValueOutput: true }) processDelegate.DynamicInvoke(node, inputValues, entry.Values);
    }

    public NodeGroup AddGroup()
    {
        var nodeGroup = new NodeGroup();
        Groups.Add(nodeGroup);
        return nodeGroup;
    }
}

public class NodeGroup
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; set; } = "New Group";
    public ObservableCollection<Guid> Nodes { get; } = [];
}

public record NodeConnection(ConnectionType ConnectionType, Guid OutputNodeId, int OutputSlot, Guid InputNodeId, int InputSlot);

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