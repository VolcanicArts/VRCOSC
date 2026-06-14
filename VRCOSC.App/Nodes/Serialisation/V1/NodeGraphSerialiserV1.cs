// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
        var nodeIds = new List<Guid>();
        var idMapping = new Dictionary<Guid, Guid>();

        Reference.Name.Value = data.Name;
        Reference.Enabled.Value = data.Enabled;

        foreach (var sV in data.Variables)
        {
            try
            {
                if (!TypeResolver.TryConstruct(sV.Type, out var variableType))
                {
                    Logger.Log($"Unable to construct variable type {sV.Type}");
                    continue;
                }

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
                var serialisableNodeType = NodeGraphBaseHelper.RunTypeMigration(sN.Type);

                if (!TypeResolver.TryConstruct(serialisableNodeType, out var nodeType))
                {
                    Logger.Log($"Unable to construct node type {serialisableNodeType}");
                    continue;
                }

                var nodeId = Guid.NewGuid();
                idMapping.Add(sN.Id, nodeId);

                var nodeResult = Reference.AddNode(nodeType, nodeId);
                Debug.Assert(nodeResult.IsSuccess);

                var node = nodeResult.Value;
                nodeIds.Add(node.Id);
                node.Metadata.Position = sN.Position;

                if (sN.Properties is not null)
                {
                    foreach (var (propertyKey, propertyValue) in sN.Properties)
                    {
                        var migrationResult = NodeGraphBaseHelper.RunPropertyMigration(node, sN.Type, propertyKey, propertyValue);

                        if (migrationResult)
                        {
                            // Since we've migrated from a property to a ValueInput, increase the target slot on all connections to the value inputs
                            foreach (var connection in data.Connections.Where(c => c.Type == ConnectionType.Value && c.InputNodeId == sN.Id))
                            {
                                connection.InputNodeSlot++;
                            }

                            continue;
                        }

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
            if (!idMapping.TryGetValue(sC.OutputNodeId, out var outputNodeId)) continue;
            if (!idMapping.TryGetValue(sC.InputNodeId, out var inputNodeId)) continue;

            var outputNodeResult = Reference.GetNode(outputNodeId);
            if (!outputNodeResult.IsSuccess) continue;

            var inputNodeResult = Reference.GetNode(inputNodeId);
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

                    if (inputElement.Metadata.Shared.Modes == InputModes.Inline) continue;

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
                var groupNodeIds = sG.Nodes.Where(idMapping.ContainsKey).Select(nodeId => idMapping[nodeId]).ToList();
                groupNodeIds.RemoveAll(nodeId => !Reference.Elements.ContainsKey(nodeId));
                if (groupNodeIds.Count == 0) continue;

                var groupId = Guid.NewGuid();
                idMapping[sG.Id] = groupId;
                var group = Reference.AddGroup(groupNodeIds, groupId);
                group.Title.Value = sG.Title;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating a group when deserialising");
            }
        }

        return false;
    }
}