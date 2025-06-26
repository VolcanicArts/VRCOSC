// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation;

public class NodeGraphSerialiser : ProfiledSerialiser<NodeGraph, SerialisableNodeGraph>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "graphs");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodeGraphSerialiser(Storage storage, NodeGraph reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeGraph data)
    {
        Reference.Name.Value = data.Name;

        foreach (var sN in data.Nodes)
        {
            try
            {
                var nodeType = TypeResolver.Construct(sN.Type);
                if (nodeType is null) continue;

                var node = Reference.AddNode(nodeType, new Point(sN.Position.X, sN.Position.Y), sN.Id);

                if (sN.Properties is not null)
                {
                    foreach (var (propertyKey, propertyValue) in sN.Properties)
                    {
                        var property = node.GetType().GetProperties()
                                           .SingleOrDefault(property => property.TryGetCustomAttribute<NodePropertyAttribute>(out var attribute) && attribute.SerialisedName == propertyKey);

                        if (property is not null)
                        {
                            if (TryConvertToTargetType(propertyValue, property.PropertyType, out var convertedValue))
                            {
                                property.SetValue(node, convertedValue);
                            }
                        }
                    }
                }

                if (node.Metadata.ValueInputHasVariableSize || node.Metadata.ValueOutputHasVariableSize)
                {
                    Reference.VariableSizes[node.Id] = new NodeVariableSize
                    {
                        ValueInputSize = sN.ValueInputSize ?? 1,
                        ValueOutputSize = sN.ValueOutputSize ?? 1
                    };
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }

        var nodeIds = data.Nodes.Select(sN => sN.Id).ToList();

        foreach (var sC in data.Connections)
        {
            if (!nodeIds.Contains(sC.InputNodeId) || !nodeIds.Contains(sC.OutputNodeId)) continue;

            try
            {
                if (sC.Type == ConnectionType.Flow)
                    Reference.CreateFlowConnection(sC.OutputNodeId, sC.OutputNodeSlot, sC.InputNodeId);

                if (sC.Type == ConnectionType.Value)
                    Reference.CreateValueConnection(sC.OutputNodeId, sC.OutputNodeSlot, sC.InputNodeId, sC.InputNodeSlot);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }

        foreach (var sG in data.Groups)
        {
            try
            {
                var group = Reference.AddGroup(sG.Nodes, sG.Id);
                group.Title.Value = sG.Title;
                group.Nodes.RemoveIf(nodeId => !nodeIds.Contains(nodeId));
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }

        foreach (var sV in data.Variables)
        {
            try
            {
                var resolvedType = TypeResolver.Construct(sV.Type);
                if (resolvedType is null) continue;

                if (resolvedType == typeof(float)) sV.Value = (float)Convert.ChangeType(sV.Value!, typeof(float));
                if (resolvedType == typeof(int)) sV.Value = (int)Convert.ChangeType(sV.Value!, typeof(int));

                var valueRef = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(resolvedType), sV.Value)!;
                Reference.PersistentVariables.Add(sV.Key, valueRef);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }

        return false;
    }
}