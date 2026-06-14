// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation.V1;

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodePresetV1 : SerialisableVersion
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<SerialisableNodeV1> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnectionV1> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableNodeGroupV1> Groups { get; set; } = [];

    [JsonProperty("variables")]
    public List<SerialisableGraphVariableV1> Variables { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodePresetV1()
    {
    }

    public SerialisableNodePresetV1(NodePreset nodePreset)
    {
        Version = 1;

        Id = nodePreset.Id;
        Name = nodePreset.Name.Value;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeGraphV1 : SerialisableVersion
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("nodes")]
    public List<SerialisableNodeV1> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnectionV1> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableNodeGroupV1> Groups { get; set; } = [];

    [JsonProperty("variables")]
    public List<SerialisableGraphVariableV1> Variables { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGraphV1()
    {
    }

    public SerialisableNodeGraphV1(NodeGraph nodeGraph)
    {
        Version = 1;

        Id = nodeGraph.Id;
        Name = nodeGraph.Name.Value;
        Enabled = nodeGraph.Enabled.Value;
        Nodes = nodeGraph.Elements.Values.OfType<Node>().Select(node => new SerialisableNodeV1(node)).ToList();
        Connections = nodeGraph.Connections.Select(connection => new SerialisableConnectionV1(connection)).ToList();
        Groups = nodeGraph.Groups.Values.Select(group => new SerialisableNodeGroupV1(group)).ToList();
        Variables = nodeGraph.GraphVariables.Values.Select(variable => new SerialisableGraphVariableV1(variable)).ToList();
    }
}

[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
public class SerialisableNodeV1
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("position")]
    public Vector2 Position { get; set; }

    [JsonProperty("properties")]
    public Dictionary<string, object?>? Properties { get; set; }

    [JsonProperty("value_input_size")]
    public int? ValueInputSize { get; set; }

    [JsonProperty("value_output_size")]
    public int? ValueOutputSize { get; set; }

    [JsonConstructor]
    public SerialisableNodeV1()
    {
    }

    public SerialisableNodeV1(Node node)
    {
        Id = node.Id;
        Type = node.GetType().GetFriendlyName(true);
        Position = node.Metadata.Position;

        if (node.Metadata.Shared.HasProperties)
        {
            Properties = new Dictionary<string, object?>();
            Properties.AddRange(node.Metadata.Shared.Properties.ToDictionary(pair => pair.Key, pair => pair.Value.GetValue(node)));
        }

        if (node.Metadata.Shared.IsValueInput && node.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput).Last().Metadata.Shared.IsList)
        {
            ValueInputSize = node.Metadata.ElementInstancesFor(ConnectionPoint.ValueInput).Last().Metadata.Size;
        }

        if (node.Metadata.Shared.IsValueOutput && node.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput).Last().Metadata.Shared.IsList)
        {
            ValueOutputSize = node.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput).Last().Metadata.Size;
        }
    }
}

public class SerialisableConnectionV1
{
    [JsonProperty("type")]
    public ConnectionType Type { get; set; }

    [JsonProperty("out_id")]
    public Guid OutputNodeId { get; set; }

    [JsonProperty("out_slot")]
    public int OutputNodeSlot { get; set; }

    [JsonProperty("in_id")]
    public Guid InputNodeId { get; set; }

    [JsonProperty("in_slot")]
    public int InputNodeSlot { get; set; }

    [JsonConstructor]
    public SerialisableConnectionV1()
    {
    }

    public SerialisableConnectionV1(IConnection connection)
    {
        Type = connection is IFlowConnection ? ConnectionType.Flow : ConnectionType.Value;
        OutputNodeId = connection.OutputId;
        OutputNodeSlot = connection.OutputSlot;
        InputNodeId = connection.InputId;
        InputNodeSlot = connection.InputSlot;
    }
}

public enum ConnectionType
{
    Flow,
    Value
}

public class SerialisableNodeGroupV1
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<Guid> Nodes { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGroupV1()
    {
    }

    public SerialisableNodeGroupV1(NodeGroup group)
    {
        Id = group.Id;
        Title = group.Title.Value;
        Nodes = group.Nodes.ToList();
    }
}

public class SerialisableGraphVariableV1
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("persistent")]
    public bool Persistent { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("value")]
    public object Value { get; set; } = null!;

    [JsonConstructor]
    public SerialisableGraphVariableV1()
    {
    }

    public SerialisableGraphVariableV1(IGraphVariable variable)
    {
        Id = variable.GetId();
        Name = variable.GetName();
        Persistent = variable.IsPersistent();
        Type = variable.GetValueType().GetFriendlyName(true);

        if (Persistent)
            Value = variable.GetValue();
    }
}