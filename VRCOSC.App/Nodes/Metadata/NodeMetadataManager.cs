// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Metadata;

public static class NodeMetadataManager
{
    private static Dictionary<Type, INodeSharedMetadata> sharedMetadata { get; } = [];
    private static Dictionary<Guid, INodeMetadata> instanceMetadata { get; } = [];

    public static Result<INodeMetadata> GetFor(Node node)
    {
        if (instanceMetadata.TryGetValue(node.Id, out var nodeMetadata))
        {
            return Result<INodeMetadata>.Success(nodeMetadata);
        }

        return createFor(node);
    }

    public static Result<INodeSharedMetadata> GetFor(Type nodeType)
    {
        if (sharedMetadata.TryGetValue(nodeType, out var metadata))
        {
            return Result<INodeSharedMetadata>.Success(metadata);
        }

        return createSharedFor(nodeType);
    }

    private static INodeElementSharedMetadata[] createElementMetadataFor(Type nodeType, Type elementBase, Type elementListBase)
    {
        var fields = nodeType.GetFieldsByType(elementBase).ToArray();

        var arr = new INodeElementSharedMetadata[fields.Length];

        for (var i = 0; i < fields.Length; i++)
        {
            var f = fields[i];
            var modes = f.TryGetCustomAttribute<InputMode>(out var valueModeAttribute) ? valueModeAttribute.Modes : InputModes.Connection | InputModes.Inline;
            var isInlineable = f.FieldType.IsAssignableTo(typeof(IValueInput)) && NodeConstants.IsInputType(f.FieldType.GetGenericArguments()[0]) && modes.HasFlag(InputModes.Inline);

            arr[i] = new NodeElementSharedMetadata
            {
                FieldInfo = f,
                Slot = i,
                IsList = f.FieldType.IsAssignableTo(elementListBase),
                Modes = modes,
                IsInlineable = isInlineable
            };
        }

        return arr;
    }

    private static IEnumerable<INodeElementMetadata> getElementInstancesFor(Node node, INodeElementSharedMetadata[] elements)
    {
        foreach (var shared in elements)
        {
            var instance = new NodeElementMetadata
            {
                Shared = shared,
                Instance = (INodeElement)shared.FieldInfo.GetValue(node)!,
                Size = shared.IsList ? 1 : 0
            };

            instance.Instance.Metadata = instance;
            yield return instance;
        }
    }

    private static Result<INodeMetadata> createFor(Node node)
    {
        if (!sharedMetadata.TryGetValue(node.GetType(), out var shared))
        {
            var sharedResult = createSharedFor(node.GetType());
            if (!sharedResult.IsSuccess) return sharedResult.Exception;

            shared = sharedResult.Value;
        }

        var flowInputInstances = getElementInstancesFor(node, shared.Elements[ConnectionPoint.FlowInput]).ToArray();
        var flowOutputInstances = getElementInstancesFor(node, shared.Elements[ConnectionPoint.FlowOutput]).ToArray();
        var valueInputInstances = getElementInstancesFor(node, shared.Elements[ConnectionPoint.ValueInput]).ToArray();
        var valueOutputInstances = getElementInstancesFor(node, shared.Elements[ConnectionPoint.ValueOutput]).ToArray();

        var elementInstances = new Dictionary<ConnectionPoint, INodeElementMetadata[]>
        {
            { ConnectionPoint.FlowInput, flowInputInstances },
            { ConnectionPoint.FlowOutput, flowOutputInstances },
            { ConnectionPoint.ValueInput, valueInputInstances },
            { ConnectionPoint.ValueOutput, valueOutputInstances }
        };

        var metadata = new NodeMetadata
        {
            Shared = shared,
            Elements = elementInstances
        };

        instanceMetadata[node.Id] = metadata;
        return metadata;
    }

    private static Result<INodeSharedMetadata> createSharedFor(Type nodeType)
    {
        if (nodeType.IsAbstract)
            return new Exception("Node must not be abstract");

        if (!nodeType.TryGetCustomAttribute<NodeAttribute>(out var nodeAttribute))
            return new Exception($"Node must have a {nameof(NodeAttribute)}");

        var typeGenerics = nodeType.IsGenericType ? nodeType.GetGenericArguments() : [];

        var properties = nodeType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(p => p.HasCustomAttribute<NodePropertyAttribute>())
                                 .ToDictionary(p => p.GetCustomAttribute<NodePropertyAttribute>()!.Name, p => p);

        var flowInputElements = createElementMetadataFor(nodeType, typeof(IFlowInputBase), typeof(IFlowInputList));
        var flowOutputElements = createElementMetadataFor(nodeType, typeof(IFlowOutputBase), typeof(IFlowOutputList));
        var valueInputElements = createElementMetadataFor(nodeType, typeof(IValueInputBase), typeof(IValueInputList));
        var valueOutputElements = createElementMetadataFor(nodeType, typeof(IValueOutputBase), typeof(IValueOutputList));

        var elements = new Dictionary<ConnectionPoint, INodeElementSharedMetadata[]>
        {
            { ConnectionPoint.FlowInput, flowInputElements },
            { ConnectionPoint.FlowOutput, flowOutputElements },
            { ConnectionPoint.ValueInput, valueInputElements },
            { ConnectionPoint.ValueOutput, valueOutputElements }
        };

        var reprocess = nodeType.HasCustomAttribute<NodeForceReprocessAttribute>();
        var collapsedAttribute = nodeType.GetCustomAttribute<NodeCollapsedAttribute>();

        if (collapsedAttribute is not null)
        {
            if (flowInputElements.Length > 0 || flowOutputElements.Length > 0)
                return new Exception("Flow nodes cannot be collapsed");

            if (valueInputElements.Any(e => e.IsList) || valueOutputElements.Any(e => e.IsList))
                return new Exception("Node with variable size cannot be collapsed");
        }

        var noCancel = nodeType.HasCustomAttribute<NodeNoCancelAttribute>();
        var isContinuous = nodeType.GetInterfaces().Contains(typeof(IContinuousNode));
        var isActiveUpdate = nodeType.GetInterfaces().Contains(typeof(IActiveUpdateNode));

        var moduleNodeInterface = nodeType.GetInterfaces().SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModuleNode<>));

        var pathRoot = moduleNodeInterface is null
            ? nodeAttribute.Path.Contains('/') ? nodeAttribute.Path.Split('/')[0] : nodeAttribute.Path
            : ModuleManager.GetInstance().GetModuleInstanceFromType(moduleNodeInterface.GetGenericArguments()[0]).Title;

        var metadata = new NodeSharedMetadata
        {
            Type = nodeType,
            TypeGenerics = typeGenerics,
            Name = nodeAttribute.Title,
            Icons = collapsedAttribute?.Icons ?? [],
            Path = nodeAttribute.Path,
            PathRoot = pathRoot,
            GenericsFilter = nodeType.TryGetCustomAttribute<NodeGenerics>(out var genericsAttribute) ? genericsAttribute.Types : [],
            Properties = properties,
            Elements = elements,
            Reprocess = reprocess,
            IsCollapsed = collapsedAttribute is not null,
            NoCancel = noCancel,
            IsContinuous = isContinuous,
            IsActiveUpdate = isActiveUpdate
        };

        sharedMetadata[nodeType] = metadata;
        return metadata;
    }
}