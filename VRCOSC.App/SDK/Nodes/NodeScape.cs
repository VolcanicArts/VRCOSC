// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using VRCOSC.App.SDK.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public class NodeScape
{
    public ObservableDictionary<Guid, Node> Nodes { get; } = [];
    public ObservableCollection<NodeConnection> Connections { get; } = [];
    public Dictionary<Guid, object?[]> NodeOutputs = [];

    public int ZIndex { get; set; } = 0;

    public readonly Dictionary<Guid, NodeMetadata> Metadata = [];

    public void CreateFlowConnection(Guid outputNodeId, int outputFlowSlot, Guid inputNodeId)
    {
        Logger.Log($"Creating flow connection from {Nodes[outputNodeId].GetType().Name} slot {outputFlowSlot} to {Nodes[inputNodeId].GetType().Name}");

        Connections.Add(new NodeConnection(ConnectionType.Flow, outputNodeId, outputFlowSlot, inputNodeId, 0));
    }

    public void CreateValueConnection(Guid outputNodeId, int outputValueSlot, Guid inputNodeId, int inputValueSlot)
    {
        var outputMetadata = Metadata[outputNodeId];
        var inputMetadata = Metadata[inputNodeId];

        var outputType = outputMetadata.ValueOutputTypes[outputValueSlot];

        // TODO: Calculate if the value type being connected is allowed based on already existing connections linking to the process inputs

        var isValid = inputMetadata.Processes.Any(processMetadata => outputType == processMetadata.Types[inputValueSlot]);
        if (!isValid) return;

        // if the input already has a connection, disconnect it
        {
            var inputAlreadyHasConnection =
                Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Value && connection.InputNodeId == inputNodeId && connection.InputSlot == inputValueSlot);

            if (inputAlreadyHasConnection is not null)
            {
                Connections.Remove(inputAlreadyHasConnection);
            }
        }

        Logger.Log($"Creating value connection from {Nodes[outputNodeId].GetType().Name} slot {outputValueSlot} to {Nodes[inputNodeId].GetType().Name} slot {inputValueSlot}");
        Connections.Add(new NodeConnection(ConnectionType.Value, outputNodeId, outputValueSlot, inputNodeId, inputValueSlot, outputType));
    }

    public void SetOutputValue(Node node, int slot, object value)
    {
        // TODO: Validate that the value type for the slot is correct

        if (NodeOutputs.TryGetValue(node.Id, out var valueList))
        {
            valueList[slot] = value;
        }
        else
        {
            NodeOutputs.Add(node.Id, new object?[100]);
            NodeOutputs[node.Id][slot] = value;
        }
    }

    public void AddNode(Node node)
    {
        node.ZIndex = ZIndex++;
        node.NodeScape = this;
        Nodes.Add(node.Id, node);
        generateMetadata(node);
    }

    private MethodInfo? getProcessMethod(Node currentNode)
    {
        var methods = currentNode.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                 .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>());

        var isFlowNode = currentNode.GetType().HasCustomAttribute<NodeFlowAttribute>();
        var isValueNode = currentNode.GetType().HasCustomAttribute<NodeValueAttribute>();

        var nodeMetadata = Metadata[currentNode.Id];

        var connectionTypes = Connections.Where(connection => connection.InputNodeId == currentNode.Id && connection.ConnectionType == ConnectionType.Value)
                                         .OrderBy(connection => connection.InputSlot)
                                         .Select(connection => connection.SharedType)
                                         .ToList();

        if (isValueNode)
        {
            foreach (var processMetadata in nodeMetadata.Processes)
            {
                if (processMetadata.Types.SequenceEqual(connectionTypes))
                {
                    return processMetadata.Method;
                }
            }
        }

        if (isFlowNode) return methods.First();

        return null;
    }

    /// <summary>
    /// Takes the value input slot and returns the value from the output slot of the connected node, doing so recursively when the backwards-connected nodes are value-only
    /// </summary>
    private object? getValueForInput(Node node, int inputValueSlot)
    {
        var nodeConnection = Connections.FirstOrDefault(connection => connection.InputNodeId == node.Id && connection.InputSlot == inputValueSlot && connection.ConnectionType == ConnectionType.Value);
        if (nodeConnection is null) return null;

        var outputNode = Nodes[nodeConnection.OutputNodeId];
        var outputSlot = nodeConnection.OutputSlot;

        var isFlowNode = outputNode.GetType().HasCustomAttribute<NodeFlowAttribute>();
        var isValueNode = outputNode.GetType().HasCustomAttribute<NodeValueAttribute>();
        var valueInputCount = Metadata[outputNode.Id].ValueInputTypeCount;

        var processMethod = getProcessMethod(outputNode);

        if (isValueNode && !isFlowNode)
        {
            var inputValues = new object?[valueInputCount];

            for (var i = 0; i < valueInputCount; i++)
            {
                inputValues[i] = getValueForInput(outputNode, i);
            }

            processMethod!.Invoke(outputNode, inputValues);
            return NodeOutputs[outputNode.Id][outputSlot];
        }

        if (isFlowNode)
        {
            return NodeOutputs[outputNode.Id][outputSlot];
        }

        throw new Exception("How are you here");
    }

    public void Test()
    {
        var triggerNode = new TriggerNode();
        var textNode = new StringTextNode();
        var textNode2 = new StringTextNode();
        var branchNode = new BranchNode();
        var isEqualToNode = new IsEqualNode();
        var printNode = new PrintNode();
        var printNode2 = new PrintNode();

        textNode.Text.Value = "Matches!";
        textNode2.Text.Value = "Matches!";

        AddNode(triggerNode);
        AddNode(textNode);
        AddNode(textNode2);
        AddNode(branchNode);
        AddNode(isEqualToNode);
        AddNode(printNode);
        AddNode(printNode2);

        CreateFlowConnection(triggerNode.Id, 0, branchNode.Id);
        CreateFlowConnection(branchNode.Id, 0, printNode.Id);
        CreateFlowConnection(branchNode.Id, 1, printNode2.Id);

        CreateValueConnection(textNode.Id, 0, isEqualToNode.Id, 0);
        CreateValueConnection(textNode2.Id, 0, isEqualToNode.Id, 1);
        CreateValueConnection(isEqualToNode.Id, 0, branchNode.Id, 0);

        CreateValueConnection(textNode.Id, 0, printNode.Id, 0);
        CreateValueConnection(textNode2.Id, 0, printNode2.Id, 0);

        update();

        /***
        {
            var triggerNode = new TriggerNode(this);
            var printNode = new PrintNode(this);
            var intTextNode = new IntTextValueNode(this);
            var toStringNode = new ToStringNode(this);

            Nodes.Add(triggerNode.Id, triggerNode);
            Nodes.Add(printNode.Id, printNode);
            Nodes.Add(intTextNode.Id, intTextNode);
            Nodes.Add(toStringNode.Id, toStringNode);

            CreateFlowConnection(triggerNode.Id, 0, printNode.Id);

            CreateValueConnection(intTextNode.Id, 0, toStringNode.Id, 0);
            CreateValueConnection(toStringNode.Id, 0, printNode.Id, 0);

            intTextNode.Int.Value = 20;
        }
        ***/
    }

    private void generateMetadata(Node node)
    {
        var nodeMetadata = new NodeMetadata();

        // TODO: validate to make sure there can't be more than 1 method with the same parameter types, or parameters

        validateOnlyOneProcessForNonValueNodes(node);
        validateNoValueInputsForNonValueNodes(node);
        validateNodeValueInputCount(node);

        retrieveNodeValueOutputTypes(node, nodeMetadata);
        retrieveNodeValueInputTypes(node, nodeMetadata);

        Metadata.Add(node.Id, nodeMetadata);
    }

    private void validateOnlyOneProcessForNonValueNodes(Node node)
    {
        var isValueNode = node.GetType().HasCustomAttribute<NodeValueAttribute>();
        var isFlowNode = node.GetType().HasCustomAttribute<NodeFlowAttribute>();

        if (isValueNode || !isFlowNode) return;

        var processMethodCount = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                     .Count(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>());

        if (processMethodCount > 1) throw new Exception("Cannot have more than 1 process method on a non-value node");
    }

    private void validateNoValueInputsForNonValueNodes(Node node)
    {
        var isValueNode = node.GetType().HasCustomAttribute<NodeValueAttribute>();
        var isFlowNode = node.GetType().HasCustomAttribute<NodeFlowAttribute>();

        if (isValueNode || !isFlowNode) return;

        var isValid = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                          .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                          .All(methodInfo => methodInfo.GetParameters().Length == 0);

        if (!isValid) throw new Exception("Cannot have value inputs on a non-value node");
    }

    private void validateNodeValueInputCount(Node node)
    {
        if (!node.GetType().HasCustomAttribute<NodeValueAttribute>()) return;

        var numParametersList = new List<int>();

        foreach (var methodInfo in node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()))
        {
            numParametersList.Add(methodInfo.GetParameters().Length);
        }

        if (numParametersList.Count == 0) return;

        var isValid = numParametersList.All(o => o == numParametersList[0]);
        if (!isValid) throw new Exception($"Node of type {node.GetType()} has differing inputs for each {nameof(NodeProcessAttribute)}");
    }

    private void retrieveNodeValueOutputTypes(Node node, NodeMetadata nodeMetadata)
    {
        if (!node.GetType().TryGetCustomAttribute<NodeValueAttribute>(out var attribute)) return;

        nodeMetadata.ValueOutputTypes = attribute.ValueOutputTypes;
    }

    private void retrieveNodeValueInputTypes(Node node, NodeMetadata nodeMetadata)
    {
        if (!node.GetType().HasCustomAttribute<NodeValueAttribute>()) return;

        nodeMetadata.ValueInputTitles = node.GetType().GetCustomAttribute<NodeInputsAttribute>()?.Titles ?? [];

        foreach (var methodInfo in node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()))
        {
            nodeMetadata.Processes.Add(new NodeProcessMetadata
            {
                Method = methodInfo,
                Types = methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToList()
            });
        }
    }

    private void update()
    {
        foreach (var node in Nodes.Values.Where(node => node.GetType().TryGetCustomAttribute<NodeFlowAttribute>(out var nodeFlowAttribute) && nodeFlowAttribute.IsTrigger))
        {
            Guid? nextNodeId = node.Id;

            do
            {
                var currentNode = Nodes[nextNodeId.Value];
                var processMethod = getProcessMethod(currentNode);

                if (processMethod is null)
                {
                    nextNodeId = null;
                    continue;
                }

                var valuesToPass = new object?[processMethod.GetParameters().Length];

                for (var i = 0; i < processMethod.GetParameters().Length; i++)
                {
                    valuesToPass[i] = getValueForInput(currentNode, i);
                }

                if (processMethod.ReturnType == typeof(int?))
                {
                    var outputNodeSlot = (int?)processMethod.Invoke(currentNode, valuesToPass);

                    var flowConnection = Connections.FirstOrDefault(connection =>
                        connection.OutputNodeId == currentNode.Id && connection.OutputSlot == outputNodeSlot && connection.ConnectionType == ConnectionType.Flow);

                    if (flowConnection is null)
                    {
                        nextNodeId = null;
                        continue;
                    }

                    nextNodeId = flowConnection.InputNodeId;
                }
                else if (processMethod.ReturnType == typeof(void))
                {
                    processMethod.Invoke(currentNode, valuesToPass);
                    nextNodeId = null;
                }
            } while (nextNodeId is not null);
        }
    }
}

public record NodeConnection(ConnectionType ConnectionType, Guid OutputNodeId, int OutputSlot, Guid InputNodeId, int InputSlot, Type? SharedType = null);

public enum ConnectionType
{
    Flow,
    Value
}

public class NodeMetadata
{
    public List<NodeProcessMetadata> Processes { get; set; } = [];

    public int ValueInputTypeCount => Processes.Count > 0 ? Processes[0].Types.Count : 0;

    public int ValueOutputCount => ValueOutputTypes.Length;
    public Type[] ValueOutputTypes { get; set; } = [];

    public List<string> ValueInputTitles { get; set; }
}

public class NodeProcessMetadata
{
    public required MethodInfo Method { get; set; }
    public required List<Type> Types { get; set; }
}