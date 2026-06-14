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
    protected override Dictionary<string, T> TransformValue(JsonObject value, IPulseContext c)
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
public class DictionaryToJsonObjectNode<T> : ValueTransformNode<Dictionary<string, T>, JsonObject?>
{
    protected override JsonObject? TransformValue(Dictionary<string, T> value, IPulseContext c) => value is null ? null : (JsonObject)JsonSerializer.SerializeToNode(value, options: null)!;
}

[Node("JsonArray To Enumerable", "Json")]
[NodeCollapsed]
public class JsonArrayToEnumerableNode<T> : ValueTransformNode<JsonArray, List<T>>
{
    protected override List<T> TransformValue(JsonArray value, IPulseContext c)
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
public class EnumerableToJsonArrayNode<T> : ValueTransformNode<List<T>, JsonArray?>
{
    protected override JsonArray? TransformValue(List<T> value, IPulseContext c) => value is null ? null : (JsonArray)JsonSerializer.SerializeToNode(value, options: null)!;
}