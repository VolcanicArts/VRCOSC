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

        var outputsHaveVariableSize = inputParameters.Any(p => p.HasCustomAttribute<NodeVariableSizeAttribute>());

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

        return new NodeMetadata
        {
            Title = nodeAttribute.Title,
            Path = nodeAttribute.Path,
            GenericArguments = type.GetGenericArguments(),
            FlowOutputs = type.IsAssignableTo(typeof(IFlowOutput)) ? getFlowOutputsMetadata(node).ToArray() : [],
            IsFlowInput = isFlowInput,
            IsFlowOutput = isFlowOutput,
            IsValueInput = isValueInput,
            IsValueOutput = isValueOutput,
            IsAsync = isAsync,
            IsTrigger = isTrigger,
            Process = new NodeProcessMetadata
            {
                Method = method,
                Inputs = getIoMetadata(inputParameters).ToArray(),
                Outputs = getIoMetadata(outputParameters).ToArray()
            }
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

    private static IEnumerable<NodeFlowMetadata> getFlowOutputsMetadata(Node node)
    {
        var iFlowOutput = (IFlowOutput)node;
        var flowOutputRefs = iFlowOutput.FlowOutputs;

        foreach (var flowOutputRef in flowOutputRefs)
        {
            if (flowOutputRef is null)
            {
                yield return new NodeFlowMetadata
                {
                    Name = string.Empty,
                    Scope = false
                };

                continue;
            }

            yield return new NodeFlowMetadata
            {
                Name = flowOutputRef.Name,
                Scope = flowOutputRef.Scope
            };
        }
    }

    private static IEnumerable<NodeValueMetadata> getIoMetadata(List<ParameterInfo> parameters)
    {
        foreach (var inputParameter in parameters)
        {
            var name = string.Empty;

            if (inputParameter.TryGetCustomAttribute<NodeValueAttribute>(out var nodeValueAttribute))
            {
                name = nodeValueAttribute.Name;
            }

            var variableSize = inputParameter.GetCustomAttribute<NodeVariableSizeAttribute>();

            yield return new NodeValueMetadata
            {
                Name = name,
                Type = inputParameter.ParameterType.IsByRef ? inputParameter.ParameterType.GetElementType()! : inputParameter.ParameterType,
                VariableSize = variableSize
            };
        }
    }
}

public sealed class NodeMetadata
{
    public string Title { get; set; } = null!;
    public string Path { get; set; } = null!;
    public Type[] GenericArguments { get; set; } = null!;
    public NodeProcessMetadata Process { get; set; } = null!;
    public NodeFlowMetadata[] FlowOutputs { get; set; } = [];
    public bool IsFlowInput { get; set; }
    public bool IsFlowOutput { get; set; }
    public bool IsValueInput { get; set; }
    public bool IsValueOutput { get; set; }
    public bool IsTrigger { get; set; }
    public bool IsAsync { get; set; }

    public bool IsFlow => IsFlowInput || IsFlowOutput;
    public bool IsValue => IsValueInput || IsValueOutput;
    public string GenericArgumentsAsString => string.Join(", ", GenericArguments.Select(arg => arg.GetFriendlyName()));
}

public sealed class NodeProcessMetadata
{
    public MethodInfo Method { get; set; } = null!;
    public NodeValueMetadata[] Inputs { get; set; } = [];
    public NodeValueMetadata[] Outputs { get; set; } = [];
}

public sealed class NodeFlowMetadata
{
    public string Name { get; set; } = null!;
    public bool Scope { get; set; }
}

public sealed class NodeValueMetadata
{
    public string Name { get; set; } = null!;
    public Type Type { get; set; } = null!;
    public NodeVariableSizeAttribute? VariableSize { get; set; }
}