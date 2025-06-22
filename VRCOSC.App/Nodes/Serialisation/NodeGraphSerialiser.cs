// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation;

public class NodeGraphSerialiser : ProfiledSerialiser<NodeGraph, SerialisableNodeGraph>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "graphs");
    protected override string FileName => $"{Reference.Id}.json";

    public NodeGraphSerialiser(Storage storage, NodeGraph reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeGraph data)
    {
        Reference.Name.Value = data.Name;

        data.Nodes.ForEach(sN =>
        {
            try
            {
                var nodeType = TypeResolver.Construct(sN.Type);
                if (nodeType is null) return;

                var node = Reference.AddNode(sN.Id, nodeType);

                node.Position = new ObservableVector2(sN.Position.X, sN.Position.Y);
                node.ZIndex.Value = sN.ZIndex;

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
            catch
            {
            }
        });

        data.Connections.ForEach(sC =>
        {
            try
            {
                if (sC.Type == ConnectionType.Flow)
                    Reference.CreateFlowConnection(sC.OutputNodeId, sC.OutputNodeSlot, sC.InputNodeId);

                if (sC.Type == ConnectionType.Value)
                    Reference.CreateValueConnection(sC.OutputNodeId, sC.OutputNodeSlot, sC.InputNodeId, sC.InputNodeSlot);
            }
            catch
            {
            }
        });

        data.Groups.ForEach(sG =>
        {
            try
            {
                var group = Reference.AddGroup(sG.Id);
                group.Title.Value = sG.Title;
                group.Nodes.AddRange(sG.Nodes);
            }
            catch
            {
            }
        });

        data.Variables.ForEach(sV =>
        {
            try
            {
                var resolvedType = TypeResolver.Construct(sV.Type);
                if (resolvedType is null) return;

                if (resolvedType == typeof(float)) sV.Value = (float)Convert.ChangeType(sV.Value!, typeof(float));
                if (resolvedType == typeof(int)) sV.Value = (int)Convert.ChangeType(sV.Value!, typeof(int));

                var valueRef = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(resolvedType), sV.Value)!;
                Reference.PersistentVariables.Add(sV.Key, valueRef);
            }
            catch
            {
            }
        });

        return false;
    }
}