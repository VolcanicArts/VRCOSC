// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json;
using System.Text.Json.Nodes;

namespace VRCOSC.App.Nodes.Types.Json;

[Node("Json To String", "Json")]
public class JsonToStringNode : ValueComputeNode<string>
{
    public ValueInput<JsonNode> JsonNode = new();
    public ValueInput<JsonSerializerOptions?> Options = new();

    protected override string ComputeValue(IPulseContext c) => JsonNode.Read(c)?.ToJsonString(Options.Read(c))!;
}