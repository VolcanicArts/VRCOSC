// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Newtonsoft.Json;
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
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeGraph : SerialisableVersion
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
    public SerialisableNodeGraph()
    {
    }

    public SerialisableNodeGraph(NodeGraph nodeGraph)
    {
        Version = 1;

        Id = nodeGraph.Id;
        Name = nodeGraph.Name.Value;
        Nodes = nodeGraph.Nodes.Values.Select(node => new SerialisableNode(node)).ToList();
        Connections = nodeGraph.Connections.Values.Select(connection => new SerialisableConnection(connection)).ToList();
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
        Type = node.GetType().GetFriendlyName();
        Position = new Vector2((float)node.NodePosition.X, (float)node.NodePosition.Y);

        if (node.Metadata.Properties.Count != 0)
        {
            Properties = new Dictionary<string, object?>();
            Properties.AddRange(node.Metadata.Properties.ToDictionary(property => property.GetCustomAttribute<NodePropertyAttribute>()!.SerialisedName, property => property.GetValue(node)));
        }

        if (node.Metadata.ValueInputHasVariableSize)
        {
            ValueInputSize = node.VariableSize.ValueInputSize;
        }

        if (node.Metadata.ValueOutputHasVariableSize)
        {
            ValueOutputSize = node.VariableSize.ValueOutputSize;
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
        Type = variable.GetValueType().GetFriendlyName();

        if (Persistent)
            Value = variable.GetValue();
    }
}