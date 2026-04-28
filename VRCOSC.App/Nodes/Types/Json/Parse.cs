// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json;
using System.Text.Json.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Json;

[Node("Parse", "Json")]
[NodeGenericTypeFilter(typeof(JsonObject), typeof(JsonArray))]
public class ParseJsonNode<T>() : TryValueComputeNode<T>(typeof(T).GetFriendlyName()) where T : class
{
    public ValueInput<string> String = new();
    public ValueInput<JsonSerializerOptions> Options = new();

    protected override bool TryComputeValue(out T value, PulseContext c)
    {
        value = null!;

        var @string = String.Read(c);
        var options = Options.Read(c);

        if (string.IsNullOrWhiteSpace(@string)) return false;

        var result = JsonSerializerSafe.TryDeserialize<T>(@string, options);
        if (!result.IsSuccess) return false;

        value = result.Value;
        return true;
    }
}