// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes.Types.Converters;
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

    public void RegisterNode(Node node)
    {
        if (Metadata.ContainsKey(node.GetType())) return;

        Metadata.Add(node.GetType(), NodeMetadataBuilder.BuildFrom(node));
    }

    public NodeMetadata GetMetadata(Node node) => Metadata[node.GetType()];

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

        Type outputType = outputMetadata.Process.Outputs[outputValueSlot].Type;
        Type inputType = inputMetadata.Process.Inputs[inputValueSlot].Type;

        var inputAlreadyHasConnection =
            Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Value && connection.InputNodeId == inputNodeId && connection.InputSlot == inputValueSlot);

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
        if (newConnectionMade && inputAlreadyHasConnection is not null)
        {
            Connections.Remove(inputAlreadyHasConnection);
        }
    }

    public void DeleteNode(Node node)
    {
        Nodes.Remove(node.Id);
        Connections.RemoveIf(connection => connection.OutputNodeId == node.Id || connection.InputNodeId == node.Id);
    }

    private Node addNode(Node node)
    {
        RegisterNode(node);

        node.ZIndex = ZIndex++;
        node.NodeScape = this;

        if (node.Metadata.IsFlowOutput)
        {
            var flowOutputs = ((IFlowOutput)node).FlowOutputs;

            // set the size of the array to the default size
            if (node.GetType().GetProperty(nameof(IFlowOutput.FlowOutputs))!.TryGetCustomAttribute<NodeVariableSizeAttribute>(out var variableSize))
            {
                flowOutputs = new NodeFlowRef[variableSize.DefaultSize];
            }

            // fill in defaults if only a size has been specified
            if (flowOutputs.Any(flowOutputRef => flowOutputRef is null))
            {
                for (var i = 0; i < flowOutputs.Length; i++)
                {
                    flowOutputs[i] = new NodeFlowRef();
                }
            }
        }

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

    public T AddNode<T>() where T : Node => (T)AddNode(typeof(T));

    /// <summary>
    /// Takes the value input slot and returns the value from the output slot of the connected node, doing so recursively when the backwards-connected nodes are value-only
    /// </summary>
    private object? getValueForInput(NodeScapeMemory memory, Node node, int inputValueSlot)
    {
        var nodeConnection = Connections.FirstOrDefault(connection => connection.InputNodeId == node.Id && connection.InputSlot == inputValueSlot && connection.ConnectionType == ConnectionType.Value);
        if (nodeConnection is null) return null;

        var outputNode = Nodes[nodeConnection.OutputNodeId];
        var outputSlot = nodeConnection.OutputSlot;
        var outputNodeMetadata = GetMetadata(outputNode);

        // if there's already a memory entry in this scope, don't run the output node again, just return its output
        if (memory.HasEntry(outputNode.Id))
        {
            return memory.Read(outputNode.Id, outputSlot);
        }

        var isFlowNode = outputNodeMetadata.IsFlow;
        var isValueNode = outputNodeMetadata.IsValue;

        var processMethod = outputNodeMetadata.Process;

        if (isValueNode && !isFlowNode)
        {
            var inputValues = new object?[outputNodeMetadata.Process.Inputs.Length];

            for (var i = 0; i < outputNodeMetadata.Process.Inputs.Length; i++)
            {
                inputValues[i] = getValueForInput(memory, outputNode, i);
            }

            var outputValues = processMethod.Outputs.Select(outputMetadata => getDefault(outputMetadata.Type)).ToArray();

            var valuesArray = new object?[outputNodeMetadata.Process.Inputs.Length + outputNodeMetadata.Process.Outputs.Length];
            inputValues.CopyTo(valuesArray, 0);
            outputValues.CopyTo(valuesArray, inputValues.Length);

            processMethod.Method.Invoke(outputNode, valuesArray);

            outputValues = valuesArray[inputValues.Length..];

            memory.Write(outputNode.Id, outputValues);

            return memory.Read(outputNode.Id, outputSlot);
        }

        if (isFlowNode)
        {
            return memory.Read(outputNode.Id, outputSlot);
        }

        throw new Exception("How are you here");
    }

    private readonly Stack<Guid> returnNodes = [];
    private readonly NodeScapeMemory memory = new();

    public void Update()
    {
        memory.Reset();

        lock (nodesLock)
        {
            foreach (var (_, node) in Nodes)
            {
                var metadata = GetMetadata(node);
                if (!metadata.IsTrigger) continue;

                _ = startFlow(memory, node.Id);
            }
        }
    }

    private async Task startFlow(NodeScapeMemory memory, Guid? nextNodeId)
    {
        while (nextNodeId is not null)
        {
            var currentNode = Nodes[nextNodeId.Value];
            var metadata = GetMetadata(currentNode);
            var processMethod = metadata.Process;

            var inputValues = new object?[metadata.Process.Inputs.Length];

            for (var i = 0; i < metadata.Process.Inputs.Length; i++)
            {
                inputValues[i] = getValueForInput(memory, currentNode, i);
            }

            var outputValues = processMethod.Outputs.Select(outputMetadata => getDefault(outputMetadata.Type)).ToArray();

            var valuesArray = new object?[metadata.Process.Inputs.Length + metadata.Process.Outputs.Length];
            inputValues.CopyTo(valuesArray, 0);
            outputValues.CopyTo(valuesArray, inputValues.Length);

            var result = processMethod.Method.Invoke(currentNode, valuesArray);

            NodeConnection? connection = null;

            if (metadata.IsFlowOutput)
            {
                outputValues = valuesArray[inputValues.Length..];

                var outputFlowSlot = -1;

                if (metadata.IsAsync)
                {
                    var outputTask = (Task<int>)result!;
                    await outputTask.ConfigureAwait(false);
                    outputFlowSlot = outputTask.Result;
                }
                else
                {
                    outputFlowSlot = (int)result!;
                }

                memory.Write(nextNodeId.Value, outputValues);

                if (outputFlowSlot >= 0 && metadata.FlowOutputs[outputFlowSlot].Scope)
                {
                    returnNodes.Push(currentNode.Id);
                    memory.Push();
                }

                connection = Connections.FirstOrDefault(con => con.OutputNodeId == currentNode.Id && con.OutputSlot == outputFlowSlot && con.ConnectionType == ConnectionType.Flow);
            }

            if (connection is null)
            {
                if (returnNodes.Count > 0)
                {
                    nextNodeId = returnNodes.Pop();
                    memory.Pop();
                }
                else
                {
                    nextNodeId = null;
                }
            }
            else
            {
                nextNodeId = connection.InputNodeId;
            }
        }
    }

    private object? getDefault(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

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

public record NodeConnection(ConnectionType ConnectionType, Guid OutputNodeId, int OutputSlot, Guid InputNodeId, int InputSlot, Type? SharedType = null);

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