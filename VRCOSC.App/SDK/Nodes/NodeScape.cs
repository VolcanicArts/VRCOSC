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

    public void RegisterNode(Type type)
    {
        if (Metadata.ContainsKey(type)) return;

        validate(type);
        generateMetadata(type);
    }

    public NodeMetadata GetMetadata(Node node) => GetMetadata(node.GetType());
    public NodeMetadata GetMetadata(Type type) => Metadata[type];

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

        Type outputType = outputMetadata.Process.OutputTypes[outputValueSlot];
        Type inputType = inputMetadata.Process.InputTypes[inputValueSlot];

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
        if (!Metadata.ContainsKey(node.GetType())) RegisterNode(node.GetType());

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

    public T AddNode<T>() where T : Node => (T)AddNode(typeof(T));

    private NodeProcessMetadata getProcessMethod(Node currentNode)
    {
        var metadata = GetMetadata(currentNode);
        return metadata.Process;
    }

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

        var isFlowNode = outputNode.OutputFlows.Count != 0 || outputNode.InputFlows.Count != 0;
        var isValueNode = outputNodeMetadata.Process.InputCount + outputNodeMetadata.Process.OutputCount > 0;

        var processMethod = getProcessMethod(outputNode);

        if (isValueNode && !isFlowNode)
        {
            var inputValues = new object?[outputNodeMetadata.Process.InputCount];

            for (var i = 0; i < inputValues.Length; i++)
            {
                inputValues[i] = getValueForInput(memory, outputNode, i);
            }

            var output = processMethod.Method.Invoke(outputNode, inputValues);

            if (output is not null)
            {
                if (processMethod.OutputTypes.Length == 1)
                {
                    memory.Write(outputNode.Id, [output]);
                }

                if (processMethod.OutputTypes.Length > 1)
                {
                    memory.Write(outputNode.Id, expandTuple(output));
                }
            }

            return memory.Read(outputNode.Id, outputSlot);
        }

        if (isFlowNode)
        {
            return memory.Read(outputNode.Id, outputSlot);
        }

        throw new Exception("How are you here");
    }

    private void validate(Type type)
    {
        if (!type.HasCustomAttribute<NodeAttribute>()) throw new Exception();
    }

    #region Metadata

    private void generateMetadata(Type type)
    {
        var context = new NodeContext(type);
        var types = GetGenericArgumentsDisplay(type);
        var metadata = new NodeMetadata(type.GetCustomAttribute<NodeAttribute>()!.Title, types);
        //var metadata = new NodeMetadata(StringFormatter.SplitCamelAndFormatGeneric(type.GetFriendlyName().Replace("Node", "")));

        retrieveNodeTrigger(context, metadata);
        retrieveNodeProcess(context, metadata);
        retrieveNodeInputTypes(context, metadata);
        retrieveNodeOutputTypes(context, metadata);

        var processAttribute = metadata.Process.Method.GetCustomAttribute<NodeProcessAttribute>()!;
        metadata.ValueInputNames = processAttribute.Inputs;
        metadata.ValueOutputNames = processAttribute.Outputs;

        Metadata.Add(type, metadata);
    }

    public static string GetGenericArgumentsDisplay(Type type)
    {
        if (!type.IsGenericType) return string.Empty;

        var args = type.GetGenericArguments();
        var formatted = string.Join(", ", args.Select(arg => arg.GetFriendlyName()));
        return $"({formatted})";
    }

    private void retrieveNodeTrigger(NodeContext context, NodeMetadata metadata)
    {
        var triggerMethods = context.TriggerMethods;

        switch (triggerMethods.Length)
        {
            case 0:
                return;

            case > 1:
                throw new Exception("A node can only have a single trigger method");

            default:
                metadata.Trigger = new NodeTriggerMetadata(triggerMethods.Single());
                break;
        }
    }

    private void retrieveNodeProcess(NodeContext context, NodeMetadata metadata)
    {
        var process = context.ProcessMethod;
        if (!process.HasValue) throw new Exception("A node must have a node process");

        metadata.Process = new NodeProcessMetadata(process.Value.Item1);
    }

    private void retrieveNodeInputTypes(NodeContext context, NodeMetadata metadata)
    {
        metadata.Process.InputTypes = metadata.Process.Method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray();
    }

    private void retrieveNodeOutputTypes(NodeContext context, NodeMetadata metadata)
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
                if (metadata.Trigger is null) continue;

                var inputValues = new object?[metadata.Process.InputCount];

                for (var i = 0; i < inputValues.Length; i++)
                {
                    inputValues[i] = getValueForInput(memory, node, i);
                }

                var shouldTrigger = (bool)metadata.Trigger!.Method.Invoke(node, inputValues)!;
                if (!shouldTrigger) continue;

                // we've checked to see if the node should trigger, so now process that node
                // TODO: Manage this task
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
            var processMethod = getProcessMethod(currentNode);

            var inputValues = new object?[metadata.Process.InputCount];

            for (var i = 0; i < inputValues.Length; i++)
            {
                inputValues[i] = getValueForInput(memory, currentNode, i);
            }

            var output = processMethod.Method.Invoke(currentNode, inputValues);

            if (processMethod.IsAsync)
            {
                var outputTask = (Task)output!;
                await outputTask.ConfigureAwait(false);
                output = processMethod.OutputTypes.Length == 0 ? null : outputTask.GetType().GetProperty("Result")!.GetValue(outputTask);
            }

            if (processMethod.OutputTypes.Length == 1)
            {
                memory.Write(nextNodeId.Value, [output]);
            }

            if (processMethod.OutputTypes.Length > 1)
            {
                Debug.Assert(output is not null);
                memory.Write(nextNodeId.Value, expandTuple(output));
            }

            var outputFlowSlot = currentNode.NextFlowSlot;

            if (outputFlowSlot >= 0 && currentNode.OutputFlows[outputFlowSlot].Flags.HasFlag(NodeFlowFlag.Loop))
            {
                returnNodes.Push(currentNode.Id);
                memory.Push();
            }

            currentNode.NextFlowSlot = -1;

            var flowConnection = Connections.FirstOrDefault(connection =>
                connection.OutputNodeId == currentNode.Id && connection.OutputSlot == outputFlowSlot && connection.ConnectionType == ConnectionType.Flow);

            if (flowConnection is null)
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
                nextNodeId = flowConnection.InputNodeId;
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

public class NodeContext
{
    public Type Type { get; }

    private MethodInfo[]? processMethods;
    private MethodInfo[]? triggerMethods;

    public MethodInfo[] ProcessMethods => processMethods ??= getProcessMethods().ToArray();
    public MethodInfo[] TriggerMethods => triggerMethods ??= getTriggerMethods().ToArray();

    private IEnumerable<MethodInfo> getProcessMethods()
    {
        var t = Type;

        while (t is not null && t != typeof(object))
        {
            foreach (var method in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (method.HasCustomAttribute<NodeProcessAttribute>())
                    yield return method;
            }

            t = t.BaseType;
        }
    }

    private IEnumerable<MethodInfo> getTriggerMethods()
    {
        var t = Type;

        while (t is not null && t != typeof(object))
        {
            foreach (var method in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (method.HasCustomAttribute<NodeTriggerAttribute>())
                    yield return method;
            }

            t = t.BaseType;
        }
    }

    public (MethodInfo, NodeProcessAttribute)? ProcessMethod => ProcessMethods.Select(methodInfo => (methodInfo, methodInfo.GetCustomAttribute<NodeProcessAttribute>()!)).SingleOrDefault();
    public (MethodInfo, NodeTriggerAttribute)? TriggerMethod => TriggerMethods.Select(methodInfo => (methodInfo, methodInfo.GetCustomAttribute<NodeTriggerAttribute>()!)).SingleOrDefault();

    public NodeContext(Type type)
    {
        Type = type;
    }
}

public class NodeMetadata
{
    public string Title { get; }
    public string Types { get; }
    public NodeTriggerMetadata? Trigger { get; set; }
    public NodeProcessMetadata Process { get; set; }
    public string[] ValueInputNames { get; set; }
    public string[] ValueOutputNames { get; set; }

    public NodeMetadata(string title, string types)
    {
        Title = title;
        Types = types;
    }
}

public class NodeTriggerMetadata
{
    public MethodInfo Method { get; }
    public Type[] InputTypes { get; set; } = [];

    public int InputCount => InputTypes.Length;

    public NodeTriggerMetadata(MethodInfo method)
    {
        Method = method;
    }
}

public class NodeProcessMetadata
{
    public MethodInfo Method { get; }
    public bool IsAsync { get; set; }
    public Type[] InputTypes { get; set; } = [];
    public Type[] OutputTypes { get; set; } = [];

    public int InputCount => InputTypes.Length;
    public int OutputCount => OutputTypes.Length;

    public NodeProcessMetadata(MethodInfo method)
    {
        Method = method;
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