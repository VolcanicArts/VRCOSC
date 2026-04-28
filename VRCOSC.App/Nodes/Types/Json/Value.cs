// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json.Nodes;
using Json.Path;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Json;

[Node("JsonObject Get Value", "Json")]
public class JsonObjectGetValueNode<T> : ValueComputeNode<T>
{
    public ValueInput<string> Key = new();
    public ValueInput<JsonObject> JsonObject = new();

    protected override T ComputeValue(PulseContext c)
    {
        var key = Key.Read(c);
        var value = JsonObject.Read(c);

        if (string.IsNullOrWhiteSpace(key) || value is null) return default!;
        if (!value.TryGetPropertyValue(key, out var node) || node is null) return default!;

        try
        {
            if (typeof(T) == typeof(JsonArray))
                return (T)(object)node.AsArray();

            if (typeof(T) == typeof(JsonObject))
                return (T)(object)node.AsObject();

            return node.GetValue<T>();
        }
        catch
        {
            return default!;
        }
    }
}

[Node("JsonArray Get Value", "Json")]
public class JsonArrayGetValueNode<T> : ValueComputeNode<T>
{
    public ValueInput<int> Index = new();
    public ValueInput<JsonArray> JsonArray = new();

    protected override T ComputeValue(PulseContext c)
    {
        var index = Index.Read(c);
        var value = JsonArray.Read(c);

        if (value is null) return default!;
        if (index < 0 || index >= value.Count) return default!;

        try
        {
            var node = value[index];
            if (node is null) return default!;

            if (typeof(T) == typeof(JsonArray))
                return (T)(object)node.AsArray();

            if (typeof(T) == typeof(JsonObject))
                return (T)(object)node.AsObject();

            return node.GetValue<T>();
        }
        catch
        {
            return default!;
        }
    }
}

[Node("Json Path Get Value", "Json")]
public class JsonPathGetValueNode<T>() : TryValueComputeNode<T>(typeof(T).GetFriendlyName())
{
    public ValueInput<string> Path = new();
    public ValueInput<JsonNode> Json = new();

    protected override bool TryComputeValue(out T value, PulseContext c)
    {
        var path = Path.Read(c);
        var json = Json.Read(c);

        if (string.IsNullOrWhiteSpace(path))
        {
            value = default!;
            return false;
        }

        if (!JsonPath.TryParse(path, out var jsonPath))
        {
            value = default!;
            return false;
        }

        var result = jsonPath.Evaluate(json);

        try
        {
            value = result.As<T>()!;
            return true;
        }
        catch
        {
            value = default!;
            return false;
        }
    }
}