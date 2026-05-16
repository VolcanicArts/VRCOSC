// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json;

namespace VRCOSC.App.Nodes.Types.Json;

[Node("Pack Json Serializer Options", "Json")]
public sealed class PackJsonSerializerOptionsNode : ValueComputeNode<JsonSerializerOptions>
{
    public ValueInput<bool> AllowTrailingCommas = new();

    protected override JsonSerializerOptions ComputeValue(IPulseContext c)
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = AllowTrailingCommas.Read(c)
        };

        return options;
    }
}