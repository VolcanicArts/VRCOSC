// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRCOSC.App.SDK.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public class NodeField
{
    public Dictionary<Guid, Node> Nodes = [];
    public List<NodeConnection> FlowConnections = [];
    public List<NodeConnection> ValueConnections = [];
    public Dictionary<Guid, object?[]> NodeOutputs = [];

    private readonly Dictionary<Guid, NodeMetadata> metadata = [];

    public void CreateFlowConnection(Guid outputNode, int outputFlowSlot, Guid inputNode)
    {
        FlowConnections.Add(new NodeConnection(outputNode, outputFlowSlot, inputNode, 0));
    }

    public void CreateValueConnection(Guid outputNodeId, int outputValueSlot, Guid inputNodeId, int inputValueSlot)
    {
        var outputMetadata = metadata[outputNodeId];
        var inputMetadata = metadata[inputNodeId];

        var outputType = outputMetadata.ValueOutputTypes[outputValueSlot];

        var isValid = inputMetadata.ValueInputTypes.Any(pair => outputType == pair.Key[inputValueSlot]);
        if (!isValid) throw new Exception("Value connection type pairing is invalid");

        ValueConnections.Add(new NodeConnection(outputNodeId, outputValueSlot, inputNodeId, inputValueSlot, outputType));
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

    private MethodInfo? getProcessMethod(Node currentNode)
    {
        var methods = currentNode.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                 .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>());

        var isFlowNode = currentNode.GetType().HasCustomAttribute<NodeFlowAttribute>();
        var isValueNode = currentNode.GetType().HasCustomAttribute<NodeValueAttribute>();

        var nodeMetadata = metadata[currentNode.Id];

        var connectionTypes = ValueConnections.Where(connection => connection.InputNodeId == currentNode.Id).OrderBy(connection => connection.InputSlot).Select(connection => connection.SharedType)
                                              .ToList();

        if (isValueNode)
        {
            foreach (var (types, methodInfo) in nodeMetadata.ValueInputTypes)
            {
                if (types.SequenceEqual(connectionTypes))
                {
                    return methodInfo;
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
        var nodeConnection = ValueConnections.FirstOrDefault(connection => connection.InputNodeId == node.Id && connection.InputSlot == inputValueSlot);
        if (nodeConnection is null) return null;

        var outputNode = Nodes[nodeConnection.OutputNodeId];
        var outputSlot = nodeConnection.OutputSlot;

        var isFlowNode = outputNode.GetType().HasCustomAttribute<NodeFlowAttribute>();
        var isValueNode = outputNode.GetType().HasCustomAttribute<NodeValueAttribute>();
        var valueInputCount = metadata[outputNode.Id].ValueInputCount;

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
        var triggerNode = new TriggerNode(this);
        var textNode = new StringTextNode(this);
        var textNode2 = new StringTextNode(this);
        var branchNode = new BranchNode(this);
        var isEqualToNode = new IsEqualNode(this);
        var printNode = new PrintNode(this);
        var printNode2 = new PrintNode(this);

        textNode.Text.Value = "Matches!";
        textNode2.Text.Value = "Matches!";

        Nodes.Add(triggerNode.Id, triggerNode);
        Nodes.Add(textNode.Id, textNode);
        Nodes.Add(textNode2.Id, textNode2);
        Nodes.Add(branchNode.Id, branchNode);
        Nodes.Add(isEqualToNode.Id, isEqualToNode);
        Nodes.Add(printNode.Id, printNode);
        Nodes.Add(printNode2.Id, printNode2);

        generateMetadata();

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

    private void generateMetadata()
    {
        foreach (var (nodeId, node) in Nodes)
        {
            var nodeMetadata = new NodeMetadata();

            // TODO: validate to make sure there can't be more than 1 method with the same parameter types, or parameters

            validateOnlyOneProcessForNonValueNodes(node);
            validateNoValueInputsForNonValueNodes(node);

            calculateNodeValueInputCount(node, nodeMetadata);
            retrieveNodeValueOutputTypes(node, nodeMetadata);
            retrieveNodeValueInputTypes(node, nodeMetadata);

            metadata.Add(nodeId, nodeMetadata);
        }
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

    private void calculateNodeValueInputCount(Node node, NodeMetadata nodeMetadata)
    {
        if (!node.GetType().HasCustomAttribute<NodeValueAttribute>()) return;

        var numParametersList = new List<int>();

        foreach (var methodInfo in node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()))
        {
            numParametersList.Add(methodInfo.GetParameters().Length);
        }

        if (numParametersList.Count == 0)
        {
            nodeMetadata.ValueInputCount = 0;
            return;
        }

        var isValid = numParametersList.All(o => o == numParametersList[0]);
        if (!isValid) throw new Exception($"Node of type {node.GetType()} has differing inputs for each {nameof(NodeProcessAttribute)}");

        nodeMetadata.ValueInputCount = numParametersList[0];
    }

    private void retrieveNodeValueOutputTypes(Node node, NodeMetadata nodeMetadata)
    {
        if (!node.GetType().TryGetCustomAttribute<NodeValueAttribute>(out var attribute)) return;

        nodeMetadata.ValueOutputTypes = attribute.ValueOutputTypes;
    }

    private void retrieveNodeValueInputTypes(Node node, NodeMetadata nodeMetadata)
    {
        if (!node.GetType().HasCustomAttribute<NodeValueAttribute>()) return;

        foreach (var methodInfo in node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()))
        {
            nodeMetadata.ValueInputTypes.Add(methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray(), methodInfo);
        }
    }

    private void update()
    {
        foreach (var (nodeId, _) in Nodes.Where(pair => pair.Value.GetType().TryGetCustomAttribute<NodeFlowAttribute>(out var nodeFlowAttribute) && nodeFlowAttribute.IsTrigger))
        {
            Guid? nextNodeId = nodeId;

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

                    var flowConnection = FlowConnections.FirstOrDefault(connection => connection.OutputNodeId == currentNode.Id && connection.OutputSlot == outputNodeSlot);

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

public record NodeConnection(Guid OutputNodeId, int OutputSlot, Guid InputNodeId, int InputSlot, Type? SharedType = null);

public class NodeMetadata
{
    public int ValueInputCount { get; set; } = 0;
    public Type[] ValueOutputTypes { get; set; } = [];
    public Dictionary<Type[], MethodInfo> ValueInputTypes { get; set; } = [];
}