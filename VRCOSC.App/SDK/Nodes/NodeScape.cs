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
    public ObservableCollection<NodeGroup> Groups { get; } = [];

    private readonly Dictionary<Guid, object?[]> nodeOutputs = [];

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

        if (nodeOutputs.TryGetValue(node.Id, out var valueList))
        {
            valueList[slot] = value;
        }
        else
        {
            nodeOutputs.Add(node.Id, new object?[100]);
            nodeOutputs[node.Id][slot] = value;
        }
    }

    public void AddNode(Node node)
    {
        node.ZIndex = ZIndex++;
        node.NodeScape = this;
        Nodes.Add(node.Id, node);
        validate(node);
        generateMetadata(node);
    }

    private MethodInfo getProcessMethod(Node currentNode)
    {
        var methods = currentNode.GetProcessMethods();
        var isValueInputNode = currentNode.IsValueNode(ConnectionSide.Input);

        var nodeMetadata = Metadata[currentNode.Id];

        var connectionTypes = Connections.Where(connection => connection.InputNodeId == currentNode.Id && connection.ConnectionType == ConnectionType.Value)
                                         .OrderBy(connection => connection.InputSlot)
                                         .Select(connection => connection.SharedType)
                                         .ToList();

        if (isValueInputNode)
        {
            foreach (var processMetadata in nodeMetadata.Processes)
            {
                if (processMetadata.Types.SequenceEqual(connectionTypes))
                {
                    return processMetadata.Method;
                }
            }
        }

        return methods.First();
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

        var isFlowNode = outputNode.IsFlowNode(ConnectionSide.Input | ConnectionSide.Output);
        var isValueNode = outputNode.IsValueNode(ConnectionSide.Input | ConnectionSide.Output);
        var valueInputCount = Metadata[outputNode.Id].ValueInputTypeCount;

        var processMethod = getProcessMethod(outputNode);

        if (isValueNode && !isFlowNode)
        {
            var inputValues = new object?[valueInputCount];

            for (var i = 0; i < valueInputCount; i++)
            {
                inputValues[i] = getValueForInput(outputNode, i);
            }

            processMethod.Invoke(outputNode, inputValues);
            return nodeOutputs[outputNode.Id][outputSlot];
        }

        if (isFlowNode)
        {
            return nodeOutputs[outputNode.Id][outputSlot];
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
        var forNode = new ForNode();
        var intNode = new IntTextNode();

        textNode.Text.Value = "Looping!";
        textNode2.Text.Value = "Finished!";
        intNode.Int.Value = 5;

        AddNode(triggerNode);
        AddNode(textNode);
        AddNode(textNode2);
        AddNode(branchNode);
        AddNode(isEqualToNode);
        AddNode(printNode);
        AddNode(printNode2);
        AddNode(forNode);
        AddNode(intNode);

        CreateFlowConnection(triggerNode.Id, 0, forNode.Id);
        CreateFlowConnection(forNode.Id, 1, branchNode.Id);
        CreateFlowConnection(branchNode.Id, 1, printNode.Id);
        CreateFlowConnection(forNode.Id, 0, printNode2.Id);

        CreateValueConnection(intNode.Id, 0, forNode.Id, 0);

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

    private void validate(Node node)
    {
        var isValueInput = node.IsValueNode(ConnectionSide.Input);
        var isValueOutput = node.IsValueNode(ConnectionSide.Output);

        if (isValueInput)
        {
            validateNodeValueInputCount(node);
            validateAllProcessMethodsAreDifferent(node);
        }

        if (!isValueInput && !isValueOutput)
        {
            validateOnlyOneProcessForNonValueInputNodes(node);
            validateNoValueInputsForNonValueInputNodes(node);
        }
    }

    private void generateMetadata(Node node)
    {
        var nodeMetadata = new NodeMetadata();

        var isValueInput = node.IsValueNode(ConnectionSide.Input);
        var isValueOutput = node.IsValueNode(ConnectionSide.Output);

        if (isValueInput)
        {
            retrieveNodeValueInputTypes(node, nodeMetadata);
        }

        if (isValueOutput)
        {
            retrieveNodeValueOutputTypes(node, nodeMetadata);
        }

        Metadata.Add(node.Id, nodeMetadata);
    }

    private void validateAllProcessMethodsAreDifferent(Node node)
    {
        var processMethods = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()).ToList();
        var processMethodsDistinct = processMethods.DistinctBy(methodInfo => methodInfo.GetParameters()).ToList();

        if (processMethods.Count != processMethodsDistinct.Count) throw new Exception("All process methods on a node must have different input values");
    }

    private void validateOnlyOneProcessForNonValueInputNodes(Node node)
    {
        var processMethodCount = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                     .Count(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>());

        if (processMethodCount > 1) throw new Exception("Cannot have more than 1 process method on a non-value node");
    }

    private void validateNoValueInputsForNonValueInputNodes(Node node)
    {
        var isValid = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                          .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                          .All(methodInfo => methodInfo.GetParameters().Length == 0);

        if (!isValid) throw new Exception("Cannot have value inputs on a non-value node");
    }

    private void validateNodeValueInputCount(Node node)
    {
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
        if (!node.IsValueNode(ConnectionSide.Output)) return;

        nodeMetadata.ValueOutputTypes = node.GetType().GetCustomAttribute<NodeValueOutputAttribute>()!.OutputTypes;
    }

    private void retrieveNodeValueInputTypes(Node node, NodeMetadata nodeMetadata)
    {
        if (!node.IsValueNode(ConnectionSide.Input)) return;

        var titles = node.GetType().GetCustomAttribute<NodeValueInputAttribute>()!.Titles;

        for (var slot = 0; slot < titles.Count; slot++)
        {
            var title = titles[slot];
            nodeMetadata.ConnectionPoints.Add(new NodeConnectionPointMetadata(ConnectionType.Value, ConnectionSide.Input, slot, title));
        }

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

    private readonly Stack<Guid> returnNodes = [];

    private void update()
    {
        foreach (var node in Nodes.Values.Where(node => node.GetType().TryGetCustomAttribute<NodeFlowInputAttribute>(out var nodeFlowAttribute) && nodeFlowAttribute.IsTrigger))
        {
            var triggerProcess = getProcessMethod(node);
            var outputFlowSlot = (int)triggerProcess.Invoke(node, [])!;
            if (outputFlowSlot < 0) continue;

            var flowConnection = Connections.FirstOrDefault(connection => connection.ConnectionType == ConnectionType.Flow && connection.OutputNodeId == node.Id && connection.OutputSlot == outputFlowSlot);
            if (flowConnection is null) continue;

            startFlow(flowConnection.InputNodeId);
        }
    }

    private void startFlow(Guid? nextNodeId)
    {
        while (nextNodeId is not null)
        {
            var currentNode = Nodes[nextNodeId.Value];
            var processMethod = getProcessMethod(currentNode);
            var inputValueCount = processMethod.GetParameters().Length;

            var inputValues = new object?[inputValueCount];

            for (var i = 0; i < inputValueCount; i++)
            {
                inputValues[i] = getValueForInput(currentNode, i);
            }

            if (currentNode.IsFlowNode(ConnectionSide.Output))
            {
                var outputFlowSlot = (int)processMethod.Invoke(currentNode, inputValues)!;

                var flowConnection = Connections.FirstOrDefault(connection =>
                    connection.OutputNodeId == currentNode.Id && connection.OutputSlot == outputFlowSlot && connection.ConnectionType == ConnectionType.Flow);

                if (flowConnection is null)
                {
                    nextNodeId = null;
                    if (returnNodes.Count > 0) nextNodeId = returnNodes.Pop();
                    continue;
                }

                if (currentNode.GetType().TryGetCustomAttribute<NodeFlowLoop>(out var attribute) && attribute.FlowSlots.Contains(outputFlowSlot))
                {
                    returnNodes.Push(currentNode.Id);
                }

                nextNodeId = flowConnection.InputNodeId;
                continue;
            }

            processMethod.Invoke(currentNode, inputValues);
            nextNodeId = null;

            if (returnNodes.Count > 0) nextNodeId = returnNodes.Pop();
        }
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

public class NodeMetadata
{
    public List<NodeProcessMetadata> Processes { get; set; } = [];
    public List<NodeConnectionPointMetadata> ConnectionPoints { get; set; } = [];

    public int ValueInputTypeCount => Processes.Count > 0 ? Processes[0].Types.Count : 0;

    public int ValueOutputCount => ValueOutputTypes.Count;
    public List<Type> ValueOutputTypes { get; set; } = [];
}

public class NodeProcessMetadata
{
    public required MethodInfo Method { get; set; }
    public required List<Type> Types { get; set; }
}

public class NodeConnectionPointMetadata
{
    public ConnectionType Type { get; }
    public ConnectionSide Side { get; }
    public int Slot { get; }
    public string Title { get; }

    public NodeConnectionPointMetadata(ConnectionType type, ConnectionSide side, int slot, string title)
    {
        Type = type;
        Side = side;
        Slot = slot;
        Title = title;
    }
}