// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace VRCOSC.App.Nodes.Types.Json;

[Node("JsonObject To Dictionary", "Json")]
[NodeCollapsed]
public class JsonObjectToDictionaryNode<T> : ValueTransformNode<JsonObject, Dictionary<string, T>>
{
    protected override Dictionary<string, T> TransformValue(JsonObject value)
    {
        if (value is null) return [];

        try
        {
            var dict = value.ToDictionary(p => p.Key, p =>
            {
                if (p.Value is null) return default!;

                if (typeof(T) == typeof(JsonArray)) return (T)(object)p.Value.AsArray();
                if (typeof(T) == typeof(JsonObject)) return (T)(object)p.Value.AsObject();

                return p.Value.GetValue<T>();
            });

            return dict;
        }
        catch
        {
            return [];
        }
    }
}

[Node("Dictionary To JsonObject", "Json")]
[NodeCollapsed]
public class DictionaryToJsonObjectNode<T> : ValueComputeNode<JsonObject>
{
    public ValueInput<Dictionary<string, T>> Dictionary = new();

    protected override JsonObject ComputeValue(PulseContext c)
    {
        var input = Dictionary.Read(c);
        if (input is null) return null!;

        return (JsonObject)JsonSerializer.SerializeToNode(input, options: null)!;
    }
}

[Node("JsonArray To Enumerable", "Json")]
[NodeCollapsed]
public class JsonArrayToEnumerableNode<T> : ValueTransformNode<JsonArray, List<T>>
{
    protected override List<T> TransformValue(JsonArray value)
    {
        try
        {
            if (typeof(T) == typeof(JsonArray) || typeof(T) == typeof(JsonObject))
                return new List<T>(value.OfType<T>());

            return new List<T>(value.GetValues<T>());
        }
        catch
        {
            return [];
        }
    }
}

[Node("Enumerable To JsonArray", "Json")]
[NodeCollapsed]
public class EnumerableToJsonArrayNode<T> : ValueComputeNode<JsonArray>
{
    public ValueInput<List<T>> List = new();

    protected override JsonArray ComputeValue(PulseContext c)
    {
        var input = List.Read(c);
        if (input is null) return null!;

        return (JsonArray)JsonSerializer.SerializeToNode(input, options: null)!;
    }
}