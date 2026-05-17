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

namespace VRCOSC.App.Nodes.Serialisation.V2;

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeGraphV2 : SerialisableVersion
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("nodes")]
    public List<SerialisableNodeV2> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnectionV2> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableNodeGroupV2> Groups { get; set; } = [];

    [JsonProperty("variables")]
    public List<SerialisableGraphVariableV2> Variables { get; set; } = [];

    [JsonProperty("comments")]
    public List<SerialisableComment> Comments { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGraphV2()
    {
    }

    public SerialisableNodeGraphV2(NodeGraph nodeGraph)
    {
        Version = 2;

        Id = nodeGraph.Id;
        Name = nodeGraph.Name.Value;
        Enabled = nodeGraph.Enabled.Value;
        Nodes = nodeGraph.Elements.Values.OfType<Node>().Select(node => new SerialisableNodeV2(node)).ToList();
        Connections = nodeGraph.Connections.Select(connection => new SerialisableConnectionV2(connection)).ToList();
        Groups = nodeGraph.Groups.Values.Select(group => new SerialisableNodeGroupV2(group)).ToList();
        Variables = nodeGraph.GraphVariables.Values.Select(variable => new SerialisableGraphVariableV2(variable)).ToList();
        Comments = nodeGraph.Elements.Values.OfType<Comment>().Select(comment => new SerialisableComment(comment)).ToList();
    }
}

[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
public class SerialisableNodeV2
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("position")]
    public Vector2 Position { get; set; }

    [JsonProperty("sizes")]
    public Dictionary<int, int[]>? Sizes { get; set; }

    [JsonProperty("inlines")]
    public List<object?>? Inlines { get; set; }

    [JsonProperty("properties")]
    public Dictionary<string, object?>? Properties { get; set; }

    [JsonConstructor]
    public SerialisableNodeV2()
    {
    }

    public SerialisableNodeV2(Node node)
    {
        var metadata = node.Metadata;

        Id = node.Id;
        Type = node.GetType().GetFriendlyName(true);
        Position = metadata.Position;

        if (metadata.Shared.HasListElements)
        {
            Sizes = [];

            var flowOutputSizes = metadata.ElementSizesFor(ConnectionPoint.FlowOutput);
            var flowInputSizes = metadata.ElementSizesFor(ConnectionPoint.FlowInput);
            var valueOutputSizes = metadata.ElementSizesFor(ConnectionPoint.ValueOutput);
            var valueInputSizes = metadata.ElementSizesFor(ConnectionPoint.ValueInput);

            if (flowOutputSizes.Any(size => size != 0))
                Sizes[(int)ConnectionPoint.FlowOutput] = flowOutputSizes;

            if (flowInputSizes.Any(size => size != 0))
                Sizes[(int)ConnectionPoint.FlowInput] = flowInputSizes;

            if (valueOutputSizes.Any(size => size != 0))
                Sizes[(int)ConnectionPoint.ValueOutput] = valueOutputSizes;

            if (valueInputSizes.Any(size => size != 0))
                Sizes[(int)ConnectionPoint.ValueInput] = valueInputSizes;
        }

        if (metadata.Shared.IsValueInput && !metadata.Shared.IsCollapsed)
        {
            Inlines = [];

            Inlines.AddRange(metadata.Elements[ConnectionPoint.ValueInput].Select(i =>
            {
                var isList = i.Shared.IsList;
                var isConnectionOnly = i.Instance is IValueInput { Modes: ValueInputMode.Connection };
                var isInlineable = i.Shared.IsInlineable;

                return !isList && !isConnectionOnly && isInlineable ? ((IValueInput)i.Instance).GetField() : null;
            }));
        }

        if (metadata.Shared.HasProperties)
        {
            Properties = [];
            Properties.AddRange(node.Metadata.Shared.Properties.ToDictionary(pair => pair.Key, pair => pair.Value.GetValue(node)));
        }
    }
}

public class SerialisableConnectionV2
{
    [JsonProperty("type")]
    public string Type { get; set; } = null!;

    [JsonProperty("out_id")]
    public Guid OutputId { get; set; }

    [JsonProperty("out_slot")]
    public int OutputSlot { get; set; }

    [JsonProperty("out_idx")]
    public int OutputSlotIndex { get; set; }

    [JsonProperty("in_id")]
    public Guid InputId { get; set; }

    [JsonProperty("in_slot")]
    public int InputSlot { get; set; }

    [JsonProperty("in_idx")]
    public int InputSlotIndex { get; set; }

    [JsonConstructor]
    public SerialisableConnectionV2()
    {
    }

    public SerialisableConnectionV2(IConnection connection)
    {
        Type = connection is IFlowConnection ? "f" : "v";
        OutputId = connection.OutputId;
        OutputSlot = connection.OutputSlot;
        OutputSlotIndex = connection.OutputSlotIndex;
        InputId = connection.InputId;
        InputSlot = connection.InputSlot;
        InputSlotIndex = connection.InputSlotIndex;
    }
}

public class SerialisableNodeGroupV2
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<Guid> Nodes { get; set; } = [];

    [JsonConstructor]
    public SerialisableNodeGroupV2()
    {
    }

    public SerialisableNodeGroupV2(NodeGroup group)
    {
        Id = group.Id;
        Title = group.Title.Value;
        Nodes = group.Nodes.ToList();
    }
}

public class SerialisableGraphVariableV2
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
    public object? Value { get; set; }

    [JsonConstructor]
    public SerialisableGraphVariableV2()
    {
    }

    public SerialisableGraphVariableV2(IGraphVariable variable)
    {
        Id = variable.GetId();
        Name = variable.GetName();
        Persistent = variable.IsPersistent();
        Type = variable.GetValueType().GetFriendlyName(true);

        if (Persistent)
            Value = variable.GetValue();
    }
}

public class SerialisableComment
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("position")]
    public Vector2 Position { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonConstructor]
    public SerialisableComment()
    {
    }

    public SerialisableComment(IComment comment)
    {
        Id = comment.Id;
        Position = comment.Position.Value;
        Text = comment.Text.Value;
    }
}