// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;
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

    public void SpawnTo(NodeGraph targetGraph, Point pos)
    {
        var nodeIdMapping = new Dictionary<Guid, Guid>();

        foreach (var sN in Nodes)
        {
            try
            {
                var nodeType = TypeResolver.Construct(sN.Type);
                if (nodeType is null) continue;

                var newId = Guid.NewGuid();
                nodeIdMapping.Add(sN.Id, newId);
                var node = targetGraph.AddNode(nodeType, new Point(pos.X + sN.Position.X, pos.Y + sN.Position.Y), newId);

                if (sN.Properties is not null)
                {
                    foreach (var (propertyKey, propertyValue) in sN.Properties)
                    {
                        var property = node.GetType().GetProperties()
                                           .SingleOrDefault(property => property.TryGetCustomAttribute<NodePropertyAttribute>(out var attribute) && attribute.SerialisedName == propertyKey);

                        if (property is not null)
                        {
                            if (tryConvertToTargetType(propertyValue, property.PropertyType, out var convertedValue))
                            {
                                property.SetValue(node, convertedValue);
                            }
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
            catch (Exception e)
            {
                Logger.Error(e, "Error creating nodes when deserialising preset");
            }
        }

        var nodeIds = Nodes.Select(sN => sN.Id).ToList();

        foreach (var sC in Connections)
        {
            if (!nodeIds.Contains(sC.InputNodeId) || !nodeIds.Contains(sC.OutputNodeId)) continue;

            try
            {
                if (sC.Type == ConnectionType.Flow)
                    targetGraph.CreateFlowConnection(nodeIdMapping[sC.OutputNodeId], sC.OutputNodeSlot, nodeIdMapping[sC.InputNodeId]);

                if (sC.Type == ConnectionType.Value)
                    targetGraph.CreateValueConnection(nodeIdMapping[sC.OutputNodeId], sC.OutputNodeSlot, nodeIdMapping[sC.InputNodeId], sC.InputNodeSlot);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error creating connection when deserialising preset");
            }
        }
    }

    private bool tryConvertToTargetType(object? value, Type targetType, out object? outValue)
    {
        switch (value)
        {
            case null:
                outValue = null;
                return true;

            case JToken token:
                outValue = token.ToObject(targetType)!;
                return true;

            case var subValue when targetType.IsAssignableTo(typeof(Enum)):
                outValue = Enum.ToObject(targetType, subValue);
                return true;

            case long utcTicks when targetType == typeof(DateTimeOffset):
                var utcDateTime = new DateTime(utcTicks, DateTimeKind.Utc);
                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
                outValue = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
                return true;

            default:
                outValue = Convert.ChangeType(value, targetType);
                return true;
        }
    }
}