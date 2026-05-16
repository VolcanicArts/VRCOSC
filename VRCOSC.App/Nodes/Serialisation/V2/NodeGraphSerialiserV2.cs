// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation.V2;

public class NodeGraphSerialiserV2 : ProfiledSerialiser<NodeGraph, SerialisableNodeGraphV2>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "graphs");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodeGraphSerialiserV2(Storage storage, NodeGraph reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeGraphV2 data)
    {
        Reference.Name.Value = data.Name;
        Reference.Enabled.Value = data.Enabled;

        foreach (var sV in data.Variables)
        {
            try
            {
                if (!TypeResolver.TryConstruct(sV.Type, out var variableType)) continue;

                if (TryConvertToTargetType(sV.Value, variableType, out var variableValue))
                {
                    var variable = (IGraphVariable)Activator.CreateInstance(typeof(GraphVariable<>).MakeGenericType(variableType), args: [sV.Id, sV.Name, sV.Persistent, variableValue])!;
                    Reference.GraphVariables.TryAdd(sV.Id, variable);
                }
                else
                {
                    var variable = (IGraphVariable)Activator.CreateInstance(typeof(GraphVariable<>).MakeGenericType(variableType), args: [sV.Id, sV.Name, sV.Persistent])!;
                    Reference.GraphVariables.TryAdd(sV.Id, variable);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a variable when deserialising");
            }
        }

        foreach (var sN in data.Nodes)
        {
            try
            {
                if (!TypeResolver.TryConstruct(sN.Type, out var nodeType)) continue;

                var nodeResult = Reference.AddNode(nodeType, sN.Id);
                Debug.Assert(nodeResult.IsSuccess);

                var node = nodeResult.Value;
                node.Metadata.Position = sN.Position;

                if (sN.Sizes is not null)
                {
                    foreach (var (point, sizes) in sN.Sizes)
                    {
                        var elements = node.Metadata.Elements[(ConnectionPoint)point];

                        for (var i = 0; i < elements.Length; i++)
                        {
                            elements[i].Size = sizes[i];
                        }
                    }
                }

                if (sN.Properties is not null)
                {
                    foreach (var (propertyKey, propertyValue) in sN.Properties)
                    {
                        var property = node.GetType().GetProperties()
                                           .SingleOrDefault(property => property.TryGetCustomAttribute<NodePropertyAttribute>(out var attribute) && attribute.Name == propertyKey);

                        if (property is not null)
                        {
                            if (TryConvertToTargetType(propertyValue, property.PropertyType, out var convertedValue))
                            {
                                property.SetValue(node, convertedValue);
                            }
                        }
                    }
                }

                if (sN.Inlines is not null)
                {
                    var inputs = node.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput);

                    for (var i = 0; i < sN.Inlines.Count; i++)
                    {
                        var input = inputs[i];
                        if (input.Metadata.Shared.IsList) continue;

                        var inline = sN.Inlines[i];

                        if (input.Metadata.Shared.IsInlineable && TryConvertToTargetType(inline, input.Metadata.Shared.ValueType, out var value))
                            ((IValueInput)input).SetField(value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a node when deserialising");
            }
        }

        foreach (var sC in data.Connections)
        {
            var outputNodeResult = Reference.GetNode(sC.OutputId);
            if (!outputNodeResult.IsSuccess) continue;

            var inputNodeResult = Reference.GetNode(sC.InputId);
            if (!inputNodeResult.IsSuccess) continue;

            var outputNode = outputNodeResult.Value;
            var inputNode = inputNodeResult.Value;

            try
            {
                if (sC.Type == "f")
                {
                    var outputElement = outputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowOutput)[sC.OutputSlot];
                    var inputElement = inputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowInput)[sC.InputSlot];

                    Reference.CreateConnection(outputElement, 0, inputElement, 0);
                }

                if (sC.Type == "v")
                {
                    var outputElement = outputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput)[sC.OutputSlot];
                    var inputElement = inputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[sC.InputSlot];

                    Reference.CreateConnection(outputElement, sC.OutputSlotIndex, inputElement, sC.InputSlotIndex);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a connection when deserialising");
            }
        }

        foreach (var sG in data.Groups)
        {
            try
            {
                var group = Reference.AddGroup(sG.Nodes, sG.Id);
                group.Title.Value = sG.Title;
                group.Nodes.RemoveIf(nodeId => !Reference.Elements.ContainsKey(nodeId));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a group when deserialising");
            }
        }

        return false;
    }
}