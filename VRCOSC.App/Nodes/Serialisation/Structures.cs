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

namespace VRCOSC.App.Nodes.Serialisation;

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodePreset : SerialisableVersion
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<SerialisableNode> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnection> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableNodeGroup> Groups { get; set; } = [];

    [JsonProperty("variables")]
    public List<SerialisableGraphVariable> Variables { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodePreset()
    {
    }

    public SerialisableNodePreset(NodePreset nodePreset)
    {
        Version = 1;

        Id = nodePreset.Id;
        Name = nodePreset.Name.Value;
        Nodes = nodePreset.Nodes;
        Connections = nodePreset.Connections;
        Groups = nodePreset.Groups;
        Variables = nodePreset.Variables;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeGraph : SerialisableVersion
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("nodes")]
    public List<SerialisableNode> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnection> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableNodeGroup> Groups { get; set; } = [];

    [JsonProperty("variables")]
    public List<SerialisableGraphVariable> Variables { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGraph()
    {
    }

    public SerialisableNodeGraph(NodeGraph nodeGraph)
    {
        Version = 1;

        Id = nodeGraph.Id;
        Name = nodeGraph.Name.Value;
        Enabled = nodeGraph.Enabled.Value;
        Nodes = nodeGraph.Elements.Values.OfType<Node>().Select(node => new SerialisableNode(node)).ToList();
        Connections = nodeGraph.Connections.Select(connection => new SerialisableConnection(connection)).ToList();
        Groups = nodeGraph.Groups.Values.Select(group => new SerialisableNodeGroup(group)).ToList();
        Variables = nodeGraph.GraphVariables.Values.Select(variable => new SerialisableGraphVariable(variable)).ToList();
    }
}

[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
public class SerialisableNode
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
    public SerialisableNode()
    {
    }

    public SerialisableNode(Node node)
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

public class SerialisableConnection
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
    public SerialisableConnection()
    {
    }

    public SerialisableConnection(IConnection connection)
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

public class SerialisableNodeGroup
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<Guid> Nodes { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGroup()
    {
    }

    public SerialisableNodeGroup(NodeGroup group)
    {
        Id = group.Id;
        Title = group.Title.Value;
        Nodes = group.Nodes.ToList();
    }
}

public class SerialisableGraphVariable
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
    public SerialisableGraphVariable()
    {
    }

    public SerialisableGraphVariable(IGraphVariable variable)
    {
        Id = variable.GetId();
        Name = variable.GetName();
        Persistent = variable.IsPersistent();
        Type = variable.GetValueType().GetFriendlyName(true);

        if (Persistent)
            Value = variable.GetValue();
    }
}