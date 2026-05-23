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

namespace VRCOSC.App.Nodes.Serialisation.V1;

public class NodeGraphSerialiserV1 : ProfiledSerialiser<NodeGraph, SerialisableNodeGraphV1>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "graphs");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodeGraphSerialiserV1(Storage storage, NodeGraph reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeGraphV1 data)
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

                if (node.Metadata.Elements[ConnectionPoint.ValueInput].Any())
                {
                    var lastValueInput = node.Metadata.Elements[ConnectionPoint.ValueInput].Last();

                    if (lastValueInput.Shared.IsList)
                    {
                        lastValueInput.Size = sN.ValueInputSize ?? 0;
                    }
                }

                if (node.Metadata.Elements[ConnectionPoint.ValueOutput].Any())
                {
                    var lastValueOutput = node.Metadata.Elements[ConnectionPoint.ValueOutput].Last();

                    if (lastValueOutput.Shared.IsList)
                    {
                        lastValueOutput.Size = sN.ValueOutputSize ?? 0;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a node when deserialising");
            }
        }

        // Updating from previous variable system
        foreach (var node in Reference.Elements.Values.ToList())
        {
            if (node.GetType().IsAssignableTo(typeof(IHasVariableReference)) && ((IHasVariableReference)node).VariableId == Guid.Empty)
            {
                Reference.RemoveNode(node.Id);
            }
        }

        foreach (var sC in data.Connections)
        {
            var outputNodeResult = Reference.GetNode(sC.OutputNodeId);
            if (!outputNodeResult.IsSuccess) continue;

            var inputNodeResult = Reference.GetNode(sC.InputNodeId);
            if (!inputNodeResult.IsSuccess) continue;

            var outputNode = outputNodeResult.Value;
            var inputNode = inputNodeResult.Value;

            try
            {
                if (sC.Type == ConnectionType.Flow)
                {
                    var outputElement = outputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowOutput)[sC.OutputNodeSlot];
                    var inputElement = inputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowInput)[sC.InputNodeSlot];

                    Reference.CreateConnection(outputElement, 0, inputElement, 0);
                }

                if (sC.Type == ConnectionType.Value)
                {
                    var (outputSlot, outputIndex) = sC.OutputNodeSlot >= outputNode.Metadata.Shared.ValueOutputCount
                        ? (outputNode.Metadata.Shared.ValueOutputCount - 1, sC.OutputNodeSlot - (outputNode.Metadata.Shared.ValueOutputCount - 1))
                        : (sC.OutputNodeSlot, 0);

                    var (inputSlot, inputIndex) = sC.InputNodeSlot >= inputNode.Metadata.Shared.ValueInputCount
                        ? (inputNode.Metadata.Shared.ValueInputCount - 1, sC.InputNodeSlot - (inputNode.Metadata.Shared.ValueInputCount - 1))
                        : (sC.InputNodeSlot, 0);

                    var outputElement = outputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput)[outputSlot];
                    var inputElement = inputNode.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput)[inputSlot];

                    Reference.CreateConnection(outputElement, outputIndex, inputElement, inputIndex);
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