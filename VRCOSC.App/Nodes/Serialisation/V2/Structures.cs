// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Serialisation.V1;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation.V2;

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeGraphBase : SerialisableVersion
{
    [JsonProperty("nodes")]
    public List<SerialisableNode> Nodes { get; set; } = [];

    [JsonProperty("connections")]
    public List<SerialisableConnection> Connections { get; set; } = [];

    [JsonProperty("groups")]
    public List<SerialisableGroup> Groups { get; set; } = [];

    [JsonProperty("variables")]
    public List<SerialisableGraphVariable> Variables { get; set; } = [];

    [JsonProperty("comments")]
    public List<SerialisableComment> Comments { get; set; } = [];
}

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodeGraph : SerialisableNodeGraphBase
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonConstructor]
    public SerialisableNodeGraph()
    {
    }

    public SerialisableNodeGraph(NodeGraph nodeGraph)
    {
        Version = 2;

        Id = nodeGraph.Id;
        Name = nodeGraph.Name.Value;
        Enabled = nodeGraph.Enabled.Value;
        Nodes = nodeGraph.Elements.Values.OfType<Node>().Select(node => new SerialisableNode(node)).ToList();
        Connections = nodeGraph.Connections.Select(connection => new SerialisableConnection(connection)).ToList();
        Groups = nodeGraph.Groups.Values.Where(g => g.Nodes.Any() || g.Comments.Any()).Select(group => new SerialisableGroup(group)).ToList();
        Variables = nodeGraph.GraphVariables.Values.Select(variable => new SerialisableGraphVariable(variable)).ToList();
        Comments = nodeGraph.Elements.Values.OfType<Comment>().Select(comment => new SerialisableComment(comment)).ToList();
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

    [JsonProperty("sizes", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<int, int[]>? Sizes { get; set; }

    [JsonProperty("inlines", NullValueHandling = NullValueHandling.Ignore)]
    public List<object?>? Inlines { get; set; }

    [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object?>? Properties { get; set; }

    [JsonConstructor]
    public SerialisableNode()
    {
    }

    public SerialisableNode(SerialisableNodeV1 v1)
    {
        var type = NodeGraphBaseHelper.RunTypeMigration(v1.Type);

        Id = v1.Id;
        Type = type;
        Position = v1.Position;
        Properties = v1.Properties;

        if (v1.ValueInputSize.HasValue)
        {
            Sizes ??= [];

            var metadata = NodeMetadataManager.GetFor(TypeResolver.Construct(type)!);
            Sizes[(int)ConnectionPoint.ValueInput] = new int[metadata.Value.ValueInputCount];
            var index = metadata.Value.Elements[ConnectionPoint.ValueInput].IndexOf(metadata.Value.Elements[ConnectionPoint.ValueInput].First(i => i.IsList));
            Sizes[(int)ConnectionPoint.ValueInput][index] = v1.ValueInputSize.Value;
        }

        if (v1.ValueOutputSize.HasValue)
        {
            Sizes ??= [];

            var metadata = NodeMetadataManager.GetFor(TypeResolver.Construct(type)!);
            Sizes[(int)ConnectionPoint.ValueOutput] = new int[metadata.Value.ValueOutputCount];
            var index = metadata.Value.Elements[ConnectionPoint.ValueOutput].IndexOf(metadata.Value.Elements[ConnectionPoint.ValueOutput].First(i => i.IsList));
            Sizes[(int)ConnectionPoint.ValueOutput][index] = v1.ValueOutputSize.Value;
        }
    }

    public SerialisableNode(Node node)
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

        if (metadata.Shared.HasAnyInline && !metadata.Shared.IsCollapsed)
        {
            Inlines = [];

            Inlines.AddRange(metadata.Elements[ConnectionPoint.ValueInput].Select(i =>
            {
                var isList = i.Shared.IsList;
                var isConnectionOnly = i.Shared.Modes == InputModes.Connection;
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

public class SerialisableConnection
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
    public SerialisableConnection()
    {
    }

    public SerialisableConnection(SerialisableConnectionV1 v1, List<SerialisableNode> nodes)
    {
        Type = v1.Type == ConnectionType.Flow ? "f" : "v";
        OutputId = v1.OutputNodeId;
        InputId = v1.InputNodeId;

        if (v1.Type == ConnectionType.Value)
        {
            var outputNodeMetadata = NodeMetadataManager.GetFor(TypeResolver.Construct(nodes.Single(sN => sN.Id == OutputId).Type)!).Value;
            var inputNodeMetadata = NodeMetadataManager.GetFor(TypeResolver.Construct(nodes.Single(sN => sN.Id == InputId).Type)!).Value;

            if (outputNodeMetadata.Elements[ConnectionPoint.ValueOutput].Any(i => i.IsList))
            {
                var listSlot = outputNodeMetadata.Elements[ConnectionPoint.ValueOutput].IndexOf(outputNodeMetadata.Elements[ConnectionPoint.ValueOutput].First(i => i.IsList));

                if (v1.OutputNodeSlot >= listSlot)
                {
                    OutputSlot = listSlot;
                    OutputSlotIndex = v1.OutputNodeSlot - listSlot;
                }
            }
            else
            {
                OutputSlot = v1.OutputNodeSlot;
                OutputSlotIndex = 0;
            }

            if (inputNodeMetadata.Elements[ConnectionPoint.ValueInput].Any(i => i.IsList))
            {
                var listSlot = inputNodeMetadata.Elements[ConnectionPoint.ValueInput].IndexOf(inputNodeMetadata.Elements[ConnectionPoint.ValueInput].First(i => i.IsList));

                if (v1.InputNodeSlot >= listSlot)
                {
                    InputSlot = listSlot;
                    InputSlotIndex = v1.InputNodeSlot - listSlot;
                }
            }
            else
            {
                InputSlot = v1.InputNodeSlot;
                InputSlotIndex = 0;
            }
        }
        else
        {
            OutputSlot = v1.OutputNodeSlot;
            OutputSlotIndex = 0;
            InputSlot = v1.InputNodeSlot;
            InputSlotIndex = 0;
        }
    }

    public SerialisableConnection(IConnection connection)
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

public class SerialisableGroup
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("nodes")]
    public List<Guid> Nodes { get; set; } = [];

    [JsonProperty("comments")]
    public List<Guid> Comments { get; set; } = [];

    [JsonConstructor]
    public SerialisableGroup()
    {
    }

    public SerialisableGroup(SerialisableNodeGroupV1 v1)
    {
        Id = v1.Id;
        Title = v1.Title;
        Nodes = v1.Nodes;
    }

    public SerialisableGroup(NodeGroup group)
    {
        Id = group.Id;
        Title = group.Title.Value;
        Nodes = group.Nodes.ToList();
        Comments = group.Comments.ToList();
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

    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public object? Value { get; set; }

    [JsonConstructor]
    public SerialisableGraphVariable()
    {
    }

    public SerialisableGraphVariable(SerialisableGraphVariableV1 v1)
    {
        Id = v1.Id;
        Name = v1.Name;
        Persistent = v1.Persistent;
        Type = v1.Type;
        Value = v1.Value;
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

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableNodePreset : SerialisableNodeGraphBase
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonConstructor]
    public SerialisableNodePreset()
    {
    }

    public SerialisableNodePreset(NodePreset nodePreset)
    {
        Version = 2;

        Id = nodePreset.Id;
        Name = nodePreset.Name.Value;
        Nodes = nodePreset.Structure.Nodes;
        Comments = nodePreset.Structure.Comments;
        Connections = nodePreset.Structure.Connections;
        Groups = nodePreset.Structure.Groups;
        Variables = nodePreset.Structure.Variables;
    }
}