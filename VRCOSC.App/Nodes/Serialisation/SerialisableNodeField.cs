// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation;

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeField : SerialisableVersion
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<SerialiableNode> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnection> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableNodeGroup> Groups { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeField()
    {
    }

    public SerialisableNodeField(NodeField nodeField)
    {
        Version = 1;

        Id = nodeField.Id;
        Name = nodeField.Name;
        Nodes = nodeField.Nodes.Values.Select(node => new SerialiableNode(node)).ToList();
        Connections = nodeField.Connections.Select(connection => new SerialisableConnection(connection)).ToList();
        Groups = nodeField.Groups.Select(group => new SerialisableNodeGroup(group)).ToList();
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class SerialiableNode
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("position")]
    public Vector2 Position { get; set; }

    [JsonProperty("zindex")]
    public int ZIndex { get; set; }

    [JsonProperty("properties")]
    public Dictionary<string, object?> Properties { get; set; } = [];

    [JsonConstructor]
    public SerialiableNode()
    {
    }

    public SerialiableNode(Node node)
    {
        Id = node.Id;
        Type = node.GetType().GetFriendlyName();
        Position = new Vector2((float)node.Position.X, (float)node.Position.Y);
        ZIndex = node.ZIndex;
        Properties.AddRange(node.Metadata.Properties.ToDictionary(property => property.GetCustomAttribute<NodePropertyAttribute>()!.SerialisedName, property => property.GetValue(node)));
    }
}

public class SerialisableConnection
{
    [JsonProperty("type")]
    public ConnectionType Type { get; set; }

    [JsonProperty("output_node_id")]
    public Guid OutputNodeId { get; set; }

    [JsonProperty("output_node_slot")]
    public int OutputNodeSlot { get; set; }

    [JsonProperty("input_node_id")]
    public Guid InputNodeId { get; set; }

    [JsonProperty("input_node_slot")]
    public int InputNodeSlot { get; set; }

    [JsonConstructor]
    public SerialisableConnection()
    {
    }

    public SerialisableConnection(NodeConnection connection)
    {
        Type = connection.ConnectionType;
        OutputNodeId = connection.OutputNodeId;
        OutputNodeSlot = connection.OutputSlot;
        InputNodeId = connection.InputNodeId;
        InputNodeSlot = connection.InputSlot;
    }
}

public class SerialisableNodeGroup
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("nodes")]
    public List<Guid> Nodes { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGroup()
    {
    }

    public SerialisableNodeGroup(NodeGroup group)
    {
        Id = group.Id;
        Title = group.Title;
        Nodes = group.Nodes.ToList();
    }
}