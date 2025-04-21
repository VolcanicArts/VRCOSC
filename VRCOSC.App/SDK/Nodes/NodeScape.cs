// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes.Types.Constants;
using VRCOSC.App.SDK.Nodes.Types.Debug;
using VRCOSC.App.SDK.Nodes.Types.Flow;
using VRCOSC.App.SDK.Nodes.Types.Operators;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public class NodeScape
{
    public ObservableDictionary<Guid, Node> Nodes { get; } = [];
    public ObservableCollection<NodeConnection> Connections { get; } = [];
    public ObservableCollection<NodeGroup> Groups { get; } = [];

    private readonly Dictionary<Guid, object?[]> nodeOutputMemory = [];

    public int ZIndex { get; set; } = 0;

    public readonly Dictionary<Type, NodeMetadata> Metadata = [];

    public void RegisterNode<T>() where T : Node
    {
        if (Metadata.ContainsKey(typeof(T))) return;

        validate<T>();
        generateMetadata<T>();
    }

    public void CreateFlowConnection(Guid outputNodeId, int outputFlowSlot, Guid inputNodeId)
    {
        Logger.Log($"Creating flow connection from {Nodes[outputNodeId].GetType().Name} slot {outputFlowSlot} to {Nodes[inputNodeId].GetType().Name}");
        Connections.Add(new NodeConnection(ConnectionType.Flow, outputNodeId, outputFlowSlot, inputNodeId, 0));
    }

    public void CreateValueConnection(Guid outputNodeId, int outputValueSlot, Guid inputNodeId, int inputValueSlot)
    {
        var outputMetadata = Metadata[Nodes[outputNodeId].GetType()];
        var inputMetadata = Metadata[Nodes[inputNodeId].GetType()];

        // TODO: Filter this to the specific slots to see if the slots are method-generic
        var outputMethodIsGeneric = outputMetadata.Process.GenericTypes.Length > 0;
        var inputMethodIsGeneric = inputMetadata.Process.GenericTypes.Length > 0;

        // TODO: If slots aren't method generic, just check to see if the types are assignable to generics
        // if the slots are method generic, then do all the fancy logic

        Type outputType = outputMetadata.Process.OutputTypes[outputValueSlot];
        Type inputType = inputMetadata.Process.InputTypes[inputValueSlot];
        Logger.Log($"Creating value connection from {outputType.GetFriendlyName()} slot {outputValueSlot} to {inputType.GetFriendlyName()} slot {inputValueSlot}");

        if (outputMethodIsGeneric && !inputMethodIsGeneric)
        {
            var resolvedType = false;

            if (resolvedType)
            {
            }
            else
            {
                if (outputType.IsGenericType)
                {
                    if (inputType.IsGenericType)
                    {
                        outputType = outputType.GetGenericTypeDefinition().MakeGenericType(inputType.GenericTypeArguments);
                    }
                    else
                    {
                        outputType = outputType.GetGenericTypeDefinition().MakeGenericType(inputType);
                    }
                }
                else if (outputType.IsGenericParameter)
                {
                    outputType = inputType;
                }
            }
        }

        if (!outputMethodIsGeneric && inputMethodIsGeneric)
        {
            var resolvedType = false;

            if (resolvedType)
            {
            }
            else
            {
                if (inputType.IsGenericType)
                {
                    if (outputType.IsGenericType)
                    {
                        inputType = inputType.GetGenericTypeDefinition().MakeGenericType(outputType.GenericTypeArguments);
                    }
                    else
                    {
                        inputType = inputType.GetGenericTypeDefinition().MakeGenericType(outputType);
                    }
                }
                else if (inputType.IsGenericParameter)
                {
                    inputType = outputType;
                }
            }
        }

        if (!outputMethodIsGeneric && !inputMethodIsGeneric)
        {
        }

        if (outputMethodIsGeneric && inputMethodIsGeneric)
        {
            // TODO: Resolve output and input if possible
        }

        Logger.Log($"Creating value connection from {outputType.GetFriendlyName()} slot {outputValueSlot} to {inputType.GetFriendlyName()} slot {inputValueSlot}");
        var typeMatch = outputType.IsAssignableTo(inputType);
        if (!typeMatch) return;

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
        Connections.Add(new NodeConnection(ConnectionType.Value, outputNodeId, outputValueSlot, inputNodeId, inputValueSlot));
    }

    public T AddNode<T>() where T : Node
    {
        if (!Metadata.ContainsKey(typeof(T))) throw new Exception("Please register a node before attempting to add it");

        var node = Activator.CreateInstance<T>();
        node.ZIndex = ZIndex++;
        node.NodeScape = this;
        Nodes.Add(node.Id, node);

        return node;
    }

    private NodeProcessMetadata getProcessMethod(Node currentNode)
    {
        var metadata = Metadata[currentNode.GetType()];
        return metadata.Process;
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

        var processMethod = getProcessMethod(outputNode);

        if (isValueNode && !isFlowNode)
        {
            var inputValues = new object?[Metadata[outputNode.GetType()].Process.InputCount];

            for (var i = 0; i < inputValues.Length; i++)
            {
                inputValues[i] = getValueForInput(outputNode, i);
            }

            var output = processMethod.Method.Invoke(outputNode, inputValues);

            if (output is not null)
            {
                if (processMethod.OutputTypes.Length == 1)
                {
                    nodeOutputMemory[outputNode.Id] = [output];
                }

                if (processMethod.OutputTypes.Length > 1)
                {
                    nodeOutputMemory[outputNode.Id] = expandTuple(output);
                }
            }

            return nodeOutputMemory[outputNode.Id][outputSlot];
        }

        if (isFlowNode)
        {
            return nodeOutputMemory[outputNode.Id][outputSlot];
        }

        throw new Exception("How are you here");
    }

    public void Test()
    {
        foreach (var type in Assembly.GetCallingAssembly().GetExportedTypes().Where(type => type.IsAssignableTo(typeof(Node)) && !type.IsAbstract))
        {
            // lol
            var method = GetType().GetMethod("RegisterNode")!;
            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(this, null);
        }

        var triggerNode = AddNode<TriggerNode>();
        var textNode = AddNode<StringTextNode>();
        var textNode2 = AddNode<StringTextNode>();
        var branchNode = AddNode<IfNode>();
        var isEqualNode = AddNode<StringEqualsNode>();
        var printNode = AddNode<PrintNode>();
        var printNode2 = AddNode<PrintNode>();
        var forNode = AddNode<ForNode>();
        var intNode = AddNode<IntTextNode>();
        var delayNode = AddNode<DelayNode>();
        var timeSpanNode = AddNode<TimeSpanNode>();

        textNode.Text.Value = "Looping!";
        textNode2.Text.Value = "Finished!";
        intNode.Int.Value = 5;
        timeSpanNode.TimeSpan.Value = TimeSpan.FromSeconds(2);

        CreateFlowConnection(triggerNode.Id, 0, forNode.Id);
        CreateFlowConnection(forNode.Id, 1, branchNode.Id);
        CreateFlowConnection(branchNode.Id, 1, printNode.Id);
        CreateFlowConnection(forNode.Id, 0, delayNode.Id);
        CreateFlowConnection(delayNode.Id, 0, printNode2.Id);

        CreateValueConnection(timeSpanNode.Id, 0, delayNode.Id, 0);

        CreateValueConnection(intNode.Id, 0, forNode.Id, 0);

        CreateValueConnection(textNode.Id, 0, isEqualNode.Id, 0);
        CreateValueConnection(textNode2.Id, 0, isEqualNode.Id, 1);
        CreateValueConnection(isEqualNode.Id, 0, branchNode.Id, 0);

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

    private void validate<T>() where T : Node
    {
        if (!typeof(T).HasCustomAttribute<NodeAttribute>()) throw new Exception();

        var isValueInput = NodeExtensions.IsValueNode<T>(ConnectionSide.Input);
        var isValueOutput = NodeExtensions.IsValueNode<T>(ConnectionSide.Output);

        if (isValueInput)
        {
            validateNodeValueInputCount<T>();
            validateAllProcessMethodsAreDifferent<T>();
        }

        if (!isValueInput && !isValueOutput)
        {
            validateOnlyOneProcessForNonValueInputNodes<T>();
            validateNoValueInputsForNonValueInputNodes<T>();
        }
    }

    #region Metadata

    private void generateMetadata<T>() where T : Node
    {
        var context = new NodeContext<T>();
        var metadata = new NodeMetadata(typeof(T).GetCustomAttribute<NodeAttribute>()!.Title);

        var isValueInput = NodeExtensions.IsValueNode<T>(ConnectionSide.Input);
        var isValueOutput = NodeExtensions.IsValueNode<T>(ConnectionSide.Output);

        retrieveNodeProcess(context, metadata);

        if (isValueInput)
        {
            retrieveNodeInputTypes(context, metadata);
        }

        if (isValueOutput)
        {
            retrieveNodeOutputTypes(context, metadata);
        }

        Metadata.Add(typeof(T), metadata);
    }

    private void retrieveNodeProcess<T>(NodeContext<T> context, NodeMetadata metadata) where T : Node
    {
        var process = context.ProcessMethod;
        if (!process.HasValue) throw new Exception("A node must have a node process");

        metadata.Process = new NodeProcessMetadata(process.Value.Item1, process.Value.Item1.IsGenericMethod ? process.Value.Item1.GetGenericArguments() : []);
    }

    private void retrieveNodeInputTypes<T>(NodeContext<T> context, NodeMetadata metadata) where T : Node
    {
        metadata.Process.InputTypes = metadata.Process.Method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray();
    }

    private void retrieveNodeOutputTypes<T>(NodeContext<T> context, NodeMetadata metadata) where T : Node
    {
        var returnType = metadata.Process.Method.ReturnType;

        if (returnType.IsGenericType)
        {
            if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                metadata.Process.IsAsync = true;
                // get the inner type
                returnType = returnType.GenericTypeArguments[0];
            }
        }
        else
        {
            if (returnType == typeof(Task))
            {
                metadata.Process.IsAsync = true;
                returnType = typeof(void);
            }
        }

        if (returnType == typeof(void))
        {
            metadata.Process.OutputTypes = [];
            return;
        }

        // Tuples are used to return multiple values, where the item position is the slot of the output
        if (returnType.IsTuple())
        {
            metadata.Process.OutputTypes = returnType.GenericTypeArguments;
            return;
        }

        metadata.Process.OutputTypes = [returnType];
    }

    #endregion

    private void validateAllProcessMethodsAreDifferent<T>() where T : Node
    {
        var processMethods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                                      .ToList();
        var processMethodsDistinct = processMethods.DistinctBy(methodInfo => methodInfo.GetParameters()).ToList();

        if (processMethods.Count != processMethodsDistinct.Count) throw new Exception("All process methods on a node must have different input values");
    }

    private void validateOnlyOneProcessForNonValueInputNodes<T>() where T : Node
    {
        var processMethodCount = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                          .Count(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>());

        if (processMethodCount > 1) throw new Exception("Cannot have more than 1 process method on a non-value node");
    }

    private void validateNoValueInputsForNonValueInputNodes<T>() where T : Node
    {
        var isValid = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                               .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                               .All(methodInfo => methodInfo.GetParameters().Length == 0);

        if (!isValid) throw new Exception("Cannot have value inputs on a non-value node");
    }

    private void validateNodeValueInputCount<T>() where T : Node
    {
        var numParametersList = new List<int>();

        foreach (var methodInfo in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                            .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()))
        {
            numParametersList.Add(methodInfo.GetParameters().Length);
        }

        if (numParametersList.Count == 0) return;

        var isValid = numParametersList.All(o => o == numParametersList[0]);
        if (!isValid) throw new Exception($"Node of type {typeof(T).Name} has differing inputs for each {nameof(NodeProcessAttribute)}");
    }

    private readonly Stack<Guid> returnNodes = [];

    private void update()
    {
        foreach (var node in Nodes.Values.Where(node => node.GetType().TryGetCustomAttribute<NodeFlowInputAttribute>(out var nodeFlowAttribute) && nodeFlowAttribute.IsTrigger))
        {
            var processMethod = getProcessMethod(node)!;
            processMethod.Method.Invoke(node, []);

            var outputFlowSlot = node.NextFlowSlot;
            if (outputFlowSlot < 0) continue;

            var flowConnection = Connections.FirstOrDefault(connection =>
                connection.ConnectionType == ConnectionType.Flow && connection.OutputNodeId == node.Id && connection.OutputSlot == outputFlowSlot);
            if (flowConnection is null) continue;

            // TODO: Manage this task
            _ = startFlow(flowConnection.InputNodeId);
        }
    }

    private async Task startFlow(Guid? nextNodeId)
    {
        while (nextNodeId is not null)
        {
            var currentNode = Nodes[nextNodeId.Value];
            var metadata = Metadata[currentNode.GetType()];
            var processMethod = getProcessMethod(currentNode);

            var inputValues = new object?[metadata.Process.InputCount];

            for (var i = 0; i < inputValues.Length; i++)
            {
                inputValues[i] = getValueForInput(currentNode, i);
            }

            // TODO: Make generic method if needed
            var output = processMethod.Method.Invoke(currentNode, inputValues);

            if (processMethod.IsAsync)
            {
                var outputTask = (Task)output!;
                await outputTask.ConfigureAwait(false);
                output = processMethod.OutputTypes.Length == 0 ? null : outputTask.GetType().GetProperty("Result")!.GetValue(outputTask);
            }

            if (processMethod.OutputTypes.Length == 1)
            {
                Debug.Assert(output is not null);
                nodeOutputMemory[nextNodeId.Value] = [output];
            }

            if (processMethod.OutputTypes.Length > 1)
            {
                Debug.Assert(output is not null);
                var outputValues = expandTuple(output);
                nodeOutputMemory[nextNodeId.Value] = outputValues;
            }

            var outputFlowSlot = currentNode.NextFlowSlot;

            var flowConnection = Connections.FirstOrDefault(connection =>
                connection.OutputNodeId == currentNode.Id && connection.OutputSlot == outputFlowSlot && connection.ConnectionType == ConnectionType.Flow);

            currentNode.NextFlowSlot = -1;

            if (flowConnection is null)
            {
                nextNodeId = null;
                if (returnNodes.Count > 0) nextNodeId = returnNodes.Pop();
                continue;
            }

            nextNodeId = flowConnection.InputNodeId;

            if (currentNode.GetType().TryGetCustomAttribute<NodeFlowLoop>(out var attribute) && attribute.FlowSlots.Contains(outputFlowSlot))
            {
                returnNodes.Push(currentNode.Id);
            }
        }
    }

    private static object?[] expandTuple(object tuple)
    {
        if (tuple is ITuple iTuple)
        {
            var result = new object?[iTuple.Length];

            for (int i = 0; i < iTuple.Length; i++)
            {
                result[i] = iTuple[i];
            }

            return result;
        }

        throw new ArgumentException("The provided object is not a tuple", nameof(tuple));
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

public class NodeContext<T>
{
    private MethodInfo[]? processMethods;

    public MethodInfo[] ProcessMethods => processMethods ??= typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                                      .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>()).ToArray();

    public (MethodInfo, NodeProcessAttribute)? ProcessMethod => ProcessMethods.Select(methodInfo => (methodInfo, methodInfo.GetCustomAttribute<NodeProcessAttribute>()!)).SingleOrDefault();
}

public class NodeMetadata
{
    public string Title { get; }
    public NodeProcessMetadata Process { get; set; }

    public NodeMetadata(string title)
    {
        Title = title;
    }
}

public class NodeProcessMetadata
{
    public MethodInfo Method { get; }
    public Type[] GenericTypes { get; }
    public bool IsAsync { get; set; }
    public Type[] InputTypes { get; set; } = [];
    public Type[] OutputTypes { get; set; } = [];

    public int InputCount => InputTypes.Length;
    public int OutputCount => OutputTypes.Length;

    public NodeProcessMetadata(MethodInfo method, Type[] genericTypes)
    {
        Method = method;
        GenericTypes = genericTypes;
    }
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