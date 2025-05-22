// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Serialisation;

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
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class SerialiableNode
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("type_name")]
    public string TypeName { get; set; } = string.Empty;

    [JsonConstructor]
    public SerialiableNode()
    {
    }

    public SerialiableNode(Node node)
    {
        Id = node.Id;
        TypeName = node.GetType().AssemblyQualifiedName!;
    }
}