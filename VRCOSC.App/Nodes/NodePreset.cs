// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodePreset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("My New Preset");
    public List<SerialisableNode> Nodes { get; set; } = [];
    public List<SerialisableConnection> Connections { get; set; } = [];

    private readonly SerialisationManager serialiser;

    public NodePreset()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodePresetSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load()
    {
        serialiser.Deserialise();

        Name.Subscribe(_ => serialiser.Serialise());
    }

    public void Serialise()
    {
        serialiser.Serialise();
    }

    public void SpawnTo(NodeGraph targetGraph, double posX, double posY)
    {
        var nodeIdMapping = new Dictionary<Guid, Guid>();

        Nodes.ForEach(sN =>
        {
            try
            {
                var nodeType = TypeResolver.Construct(sN.Type);
                if (nodeType is null) return;

                var newId = Guid.NewGuid();
                nodeIdMapping.Add(sN.Id, newId);
                var node = targetGraph.AddNode(newId, nodeType);

                node.Position = new ObservableVector2(sN.Position.X + posX, sN.Position.Y + posY);
                node.ZIndex.Value = sN.ZIndex;

                if (sN.Properties is not null)
                {
                    foreach (var (propertyKey, propertyValue) in sN.Properties)
                    {
                        var property = node.GetType().GetProperties()
                                           .SingleOrDefault(property => property.TryGetCustomAttribute<NodePropertyAttribute>(out var attribute) && attribute.SerialisedName == propertyKey);

                        if (property is not null)
                        {
                            if (propertyValue is double && property.PropertyType == typeof(float))
                            {
                                property.SetValue(node, Convert.ChangeType(propertyValue, TypeCode.Single));
                                continue;
                            }

                            if (propertyValue is long && property.PropertyType == typeof(int))
                            {
                                property.SetValue(node, Convert.ChangeType(propertyValue, TypeCode.Int32));
                                continue;
                            }

                            property.SetValue(node, propertyValue);
                        }
                    }
                }

                if (node.Metadata.ValueInputHasVariableSize || node.Metadata.ValueOutputHasVariableSize)
                {
                    targetGraph.VariableSizes[node.Id] = new NodeVariableSize
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

        Connections.ForEach(sC =>
        {
            try
            {
                switch (sC.Type)
                {
                    case ConnectionType.Flow:
                        targetGraph.CreateFlowConnection(nodeIdMapping[sC.OutputNodeId], sC.OutputNodeSlot, nodeIdMapping[sC.InputNodeId]);
                        break;

                    case ConnectionType.Value:
                        targetGraph.CreateValueConnection(nodeIdMapping[sC.OutputNodeId], sC.OutputNodeSlot, nodeIdMapping[sC.InputNodeId], sC.InputNodeSlot);
                        break;
                }
            }
            catch
            {
            }
        });
    }
}