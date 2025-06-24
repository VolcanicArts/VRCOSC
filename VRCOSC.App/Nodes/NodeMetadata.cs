// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FontAwesome6;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public static class NodeMetadataBuilder
{
    public static NodeMetadata BuildFrom(Node node)
    {
        var type = node.GetType();

        if (!type.IsAssignableTo(typeof(Node)))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} from a type that doesn't extend {nameof(Node)}");

        if (!type.TryGetCustomAttribute<NodeAttribute>(out var nodeAttribute))
            throw new Exception($"Cannot build {nameof(NodeMetadata)} from a type that doesn't use the attribute {nameof(NodeAttribute)}");

        var flowOutputs = getFieldsByType(type, typeof(IFlow));
        var valueInputs = getFieldsByType(type, typeof(IValueInput));
        var valueOutputs = getFieldsByType(type, typeof(IValueOutput));

        var isFlowInput = type.IsAssignableTo(typeof(IFlowInput));
        var isFlowOutput = flowOutputs.Count != 0;

        var isValueInput = valueInputs.Count != 0;
        var isValueOutput = valueOutputs.Count != 0;

        var inputsHaveVariableSize = valueInputs.Any(m => m.FieldType.GetGenericTypeDefinition() == typeof(ValueInputList<>));

        if (inputsHaveVariableSize)
        {
            var inputsHaveOneVariableSize = valueInputs.Count(m => m.FieldType.GetGenericTypeDefinition() == typeof(ValueInputList<>)) == 1;

            if (!inputsHaveOneVariableSize)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as only a single ValueInputList<> is allowed for inputs");

            var inputsHaveVariableSizeLast = valueInputs.Last().FieldType.GetGenericTypeDefinition() == typeof(ValueInputList<>);

            if (!inputsHaveVariableSizeLast)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as ValueInputList<> is only allowed to be defined on the last input");
        }

        var outputsHaveVariableSize = valueOutputs.Any(m => m.FieldType.GetGenericTypeDefinition() == typeof(ValueOutputList<>));

        if (outputsHaveVariableSize)
        {
            var outputsHaveOneVariableSize = valueOutputs.Count(m => m.FieldType.GetGenericTypeDefinition() == typeof(ValueOutputList<>)) == 1;

            if (!outputsHaveOneVariableSize)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as only a single ValueOutputList<> is allowed for outputs");

            var outputsHaveVariableSizeLast = valueOutputs.Last().FieldType.GetGenericTypeDefinition() == typeof(ValueOutputList<>);

            if (!outputsHaveVariableSizeLast)
                throw new Exception($"Cannot build {nameof(NodeMetadata)} as ValueOutputList<> is only allowed to be defined on the last output");
        }

        var properties = type.GetProperties().Where(property => property.HasCustomAttribute<NodePropertyAttribute>()).ToList();

        var inputMetadata = getIoMetadata(valueInputs, node);
        var outputMetadata = getIoMetadata(valueOutputs, node);

        var metadata = new NodeMetadata
        {
            Title = nodeAttribute.Title,
            Icon = nodeAttribute.Icon,
            Path = nodeAttribute.Path,
            GenericArguments = type.GetGenericArguments(),
            FlowOutputs = flowOutputs.Select(m => (IFlow)m.GetValue(node)!).ToList(),
            ValueInputs = valueInputs.Select(m => (IValueInput)m.GetValue(node)!).ToList(),
            ValueOutputs = valueOutputs.Select(m => (IValueOutput)m.GetValue(node)!).ToList(),
            IsFlowInput = isFlowInput,
            IsFlowOutput = isFlowOutput,
            IsValueInput = isValueInput,
            IsValueOutput = isValueOutput,
            Inputs = inputMetadata,
            Outputs = outputMetadata,
            ValueInputHasVariableSize = inputsHaveVariableSize,
            ValueOutputHasVariableSize = outputsHaveVariableSize,
            ForceReprocess = type.HasCustomAttribute<NodeForceReprocessAttribute>(),
            Properties = properties
        };

        return metadata;
    }

    private static List<FieldInfo> getFieldsByType(Type? type, Type targetFieldType)
    {
        var fields = new List<FieldInfo>();

        var hierarchy = new Stack<Type>();

        while (type != null && type != typeof(object))
        {
            hierarchy.Push(type);
            type = type.BaseType;
        }

        while (hierarchy.Count > 0)
        {
            var currentType = hierarchy.Pop();

            var currentFields = currentType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                           .Where(f => targetFieldType.IsAssignableFrom(f.FieldType));
            fields.AddRange(currentFields);
        }

        return fields;
    }

    private static NodeValueMetadata[] getIoMetadata(List<FieldInfo> fields, Node node)
    {
        var arr = new NodeValueMetadata[fields.Count];

        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            var instance = (INodeAttribute)field.GetValue(node)!;

            arr[i] = new NodeValueMetadata
            {
                Name = string.IsNullOrEmpty(instance.Name) ? field.Name : instance.Name,
                Parameter = field,
                IsReactive = field.HasCustomAttribute<NodeReactiveAttribute>()
            };
        }

        return arr;
    }
}

public sealed class NodeVariableSize
{
    public int ValueInputSize { get; set; } = 1;
    public int ValueOutputSize { get; set; } = 1;
}

public sealed class NodeMetadata
{
    public string Title { get; internal set; } = null!;
    public EFontAwesomeIcon Icon { get; internal set; }
    public string? Path { get; internal set; }
    public Type[] GenericArguments { get; internal set; } = null!;
    public List<IFlow> FlowOutputs { get; internal set; } = [];
    public List<IValueInput> ValueInputs { get; internal set; } = [];
    public List<IValueOutput> ValueOutputs { get; internal set; } = [];
    public NodeValueMetadata[] Inputs { get; internal set; } = [];
    public NodeValueMetadata[] Outputs { get; internal set; } = [];
    public bool IsFlowInput { get; internal set; }
    public bool IsFlowOutput { get; internal set; }
    public bool IsValueInput { get; internal set; }
    public bool IsValueOutput { get; internal set; }
    public bool ValueInputHasVariableSize { get; internal set; }
    public bool ValueOutputHasVariableSize { get; internal set; }
    public bool ForceReprocess { get; internal set; }

    public List<PropertyInfo> Properties { get; set; } = [];

    public bool IsTrigger => IsFlowOutput && !IsFlowInput;
    public bool IsFlow => IsFlowInput || IsFlowOutput;
    public bool IsValue => IsValueInput || IsValueOutput;
    public string GenericArgumentsAsString => string.Join(", ", GenericArguments.Select(arg => arg.GetFriendlyName()));

    public int FlowCount => FlowOutputs.Count;
    public int InputsCount => Inputs.Length;
    public int OutputsCount => Outputs.Length;
}

public sealed class NodeValueMetadata
{
    public string Name { get; set; } = null!;
    public FieldInfo Parameter { get; set; } = null!;
    public bool IsReactive { get; set; }

    public Type Type => Parameter.FieldType.GenericTypeArguments[0];
}