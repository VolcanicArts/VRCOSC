// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public static class NodeMetadataBuilder
{
    public static NodeMetadata BuildFrom(Node node)
    {
        var type = node.GetType();

        if (!type.IsAssignableTo(typeof(Node)))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} from a type that doesn't extend {nameof(Node)}");

        if (!type.TryGetCustomAttribute<NodeAttribute>(out var nodeAttribute))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} from a type that doesn't use the attribute {nameof(NodeAttribute)}");

        var method = getProcessMethod(type);
        var parameters = method.GetParameters();

        var isAsync = method.ReturnParameter.ParameterType.IsAssignableTo(typeof(Task));
        var isFlowInput = type.IsAssignableTo(typeof(IFlowInput));
        var isFlowOutput = type.IsAssignableTo(typeof(IFlowOutput));
        var isTrigger = type.IsAssignableTo(typeof(IFlowTrigger));

        var allRefsAfterNonRefs = parameters.Select(p => p.ParameterType.IsByRef).SkipWhile(isRef => !isRef).All(isRef => isRef);
        if (!allRefsAfterNonRefs) throw new Exception($"Cannot build {nameof(NodeMetadata)} as the defined {nameof(NodeProcessAttribute)} method has non-refs after refs");

        var inputParameters = parameters.TakeWhile(p => !p.ParameterType.IsByRef).ToList();
        var outputParameters = parameters.SkipWhile(p => !p.ParameterType.IsByRef).ToList();

        var isValueInput = inputParameters.Count > 0;
        var isValueOutput = outputParameters.Count > 0;

        var inputsHaveVariableSize = inputParameters.Any(p => p.HasCustomAttribute<NodeVariableSizeAttribute>());

        if (inputsHaveVariableSize)
        {
            var inputsHaveOneVariableSize = inputParameters.Count(p => p.IsDefined(typeof(NodeVariableSizeAttribute), false)) == 1;

            if (!inputsHaveOneVariableSize)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as only a single {nameof(NodeVariableSizeAttribute)} is allowed for inputs");

            var inputsHaveVariableSizeLast = inputParameters.Last().IsDefined(typeof(NodeVariableSizeAttribute), false);

            if (!inputsHaveVariableSizeLast)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as {nameof(NodeVariableSizeAttribute)} is only allowed to be defined on the last input");
        }

        var outputsHaveVariableSize = outputParameters.Any(p => p.HasCustomAttribute<NodeVariableSizeAttribute>());

        if (outputsHaveVariableSize)
        {
            var outputsHaveOneVariableSize = outputParameters.Count(p => p.IsDefined(typeof(NodeVariableSizeAttribute), false)) == 1;

            if (!outputsHaveOneVariableSize)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as only a single {nameof(NodeVariableSizeAttribute)} is allowed for outputs");

            var outputsHaveVariableSizeLas = outputParameters.Last().IsDefined(typeof(NodeVariableSizeAttribute), false);

            if (!outputsHaveVariableSizeLas)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as {nameof(NodeVariableSizeAttribute)} is only allowed to be defined on the last output");
        }

        if (!isFlowOutput && method.ReturnParameter.ParameterType != typeof(void))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} as the node isn't a flow output node but the return type of the process method isn't void");

        // ensure that if a node is marked as trigger, it is also marked as a flow output

        // ensure that if the return parameter is only int or Task<int> if marked as flow

        var inputVariableSize = inputParameters.LastOrDefault()?.GetCustomAttribute<NodeVariableSizeAttribute>();
        var outputVariableSize = outputParameters.LastOrDefault()?.GetCustomAttribute<NodeVariableSizeAttribute>();

        return new NodeMetadata
        {
            Title = nodeAttribute.Title,
            Path = nodeAttribute.Path,
            GenericArguments = type.GetGenericArguments(),
            FlowOutputs = type.IsAssignableTo(typeof(IFlowOutput)) ? ((IFlowOutput)node).FlowOutputs : [],
            IsFlowInput = isFlowInput,
            IsFlowOutput = isFlowOutput,
            IsValueInput = isValueInput,
            IsValueOutput = isValueOutput,
            IsAsync = isAsync,
            IsTrigger = isTrigger,
            ProcessMethod = method,
            Inputs = getIoMetadata(inputParameters).ToArray(),
            Outputs = getIoMetadata(outputParameters).ToArray(),
            InputVariableSize = inputVariableSize,
            InputVariableSizeActual = inputVariableSize?.DefaultSize ?? 0,
            OutputVariableSize = outputVariableSize,
            OutputVariableSizeActual = outputVariableSize?.DefaultSize ?? 0
        };
    }

    private static MethodInfo getProcessMethod(Type type)
    {
        var t = type;

        var processMethods = new List<MethodInfo>();

        while (t is not null && t != typeof(object))
        {
            foreach (var methodInfo in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                    processMethods.Add(methodInfo);
            }

            t = t.BaseType;
        }

        return processMethods.Count switch
        {
            0 => throw new Exception($"Cannot build {nameof(NodeMetadata)} as a {nameof(Node)} must have a method that has a {nameof(NodeProcessAttribute)}"),
            > 1 => throw new Exception($"Cannot build {nameof(NodeMetadata)} as a {nameof(Node)} must have only 1 method that has a {nameof(NodeProcessAttribute)}"),
            _ => processMethods.Single()
        };
    }

    private static IEnumerable<NodeValueMetadata> getIoMetadata(List<ParameterInfo> parameters)
    {
        foreach (var parameter in parameters)
        {
            var name = string.Empty;

            if (parameter.TryGetCustomAttribute<NodeValueAttribute>(out var nodeValueAttribute))
            {
                name = nodeValueAttribute.Name;
            }

            yield return new NodeValueMetadata
            {
                Name = name,
                Type = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType,
            };
        }
    }
}

public sealed class NodeMetadata
{
    public string Title { get; set; } = null!;
    public string Path { get; set; } = null!;
    public Type[] GenericArguments { get; set; } = null!;
    public MethodInfo ProcessMethod { get; set; } = null!;
    public NodeFlowRef[] FlowOutputs { get; set; } = [];
    public NodeValueMetadata[] Inputs { get; set; } = [];
    public NodeValueMetadata[] Outputs { get; set; } = [];
    public NodeVariableSizeAttribute? InputVariableSize { get; set; }
    public int InputVariableSizeActual { get; set; }
    public NodeVariableSizeAttribute? OutputVariableSize { get; set; }
    public int OutputVariableSizeActual { get; set; }
    public bool IsFlowInput { get; set; }
    public bool IsFlowOutput { get; set; }
    public bool IsValueInput { get; set; }
    public bool IsValueOutput { get; set; }
    public bool IsTrigger { get; set; }
    public bool IsAsync { get; set; }

    public bool IsFlow => IsFlowInput || IsFlowOutput;
    public bool IsValue => IsValueInput || IsValueOutput;
    public string GenericArgumentsAsString => string.Join(", ", GenericArguments.Select(arg => arg.GetFriendlyName()));

    public bool InputHasVariableSize => InputVariableSize is not null;
    public bool OutputHasVariableSize => OutputVariableSize is not null;

    public int FlowCount => FlowOutputs.Length;

    public int InputsCount => Inputs.Length;

    public int InputsVirtualCount
    {
        get
        {
            if (Inputs.Length == 0) return 0;
            if (InputVariableSize is null) return Inputs.Length;

            return Inputs.Length - 1 + InputVariableSizeActual;
        }
    }

    public int OutputsCount => Outputs.Length;

    public int OutputsVirtualCount
    {
        get
        {
            if (Outputs.Length == 0) return 0;
            if (OutputVariableSize is null) return Outputs.Length;

            return Outputs.Length - 1 + OutputVariableSizeActual;
        }
    }

    public Type GetTypeOfInputSlot(int index)
    {
        if (InputsCount == 0) throw new Exception("Cannot get type of input slot when there are no inputs");
        if (!InputHasVariableSize && index >= InputsCount) throw new IndexOutOfRangeException();
        if (InputHasVariableSize && index >= InputsVirtualCount) throw new IndexOutOfRangeException();

        if (InputHasVariableSize)
        {
            if (index >= InputsCount - 1) return Inputs.Last().Type.GetElementType()!;
        }

        return Inputs[index].Type;
    }

    public Type GetTypeOfOutputSlot(int index)
    {
        if (OutputsCount == 0) throw new Exception("Cannot get type of output slot when there are no outputs");
        if (!OutputHasVariableSize && index >= OutputsCount) throw new IndexOutOfRangeException();
        if (OutputHasVariableSize && index >= OutputsVirtualCount) throw new IndexOutOfRangeException();

        if (OutputHasVariableSize)
        {
            if (index >= OutputsCount - 1) return Outputs.Last().Type.GetElementType()!;
        }

        return Outputs[index].Type;
    }
}

public sealed class NodeValueMetadata
{
    public string Name { get; set; } = null!;
    public Type Type { get; set; } = null!;
}