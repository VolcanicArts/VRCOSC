// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Dictionary Count", "Collections/Dictionary")]
[NodeCollapsed]
public sealed class DictionaryCountNode<TKey, TValue>() : SimpleValueTransformNode<Dictionary<TKey, TValue>?, int>(dictionary => dictionary?.Count ?? 0) where TKey : notnull;

[Node("Dictionary Key To Value", "Collections/Dictionary")]
public sealed class DictionaryKeyToValueNode<TKey, TValue>() : ValueComputeNode<TValue>("Value") where TKey : notnull
{
    public ValueInput<Dictionary<TKey, TValue>> Dictionary = new();
    public ValueInput<TKey> Key = new();

    protected override TValue ComputeValue(IPulseContext c)
    {
        var dictionary = Dictionary.Read(c);
        if (dictionary is null) return default!;

        var key = Key.Read(c);
        if (key is null) return default!;

        return dictionary.TryGetValue(key, out var value) ? value : default!;
    }
}

[Node("Dictionary Add Element", "Collections/Dictionary/Modifiers")]
public sealed class DictionaryElementAddNode<TKey, TValue> : ActionValueComputeNode<Dictionary<TKey, TValue>?> where TKey : notnull
{
    public ValueInput<Dictionary<TKey, TValue>?> Dictionary = new();
    public ValueInput<KeyValuePair<TKey, TValue>> Element = new();

    protected override Dictionary<TKey, TValue>? ComputeValue(IPulseContext c)
    {
        var dictionary = Dictionary.Read(c);
        if (dictionary is null) return null;

        var element = Element.Read(c);

        dictionary = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
        dictionary.TryAdd(element.Key, element.Value);
        return dictionary;
    }
}

[Node("Dictionary Remove Key", "Collections/Dictionary/Modifiers")]
public sealed class DictionaryKeyRemoveNode<TKey, TValue> : ActionValueComputeNode<Dictionary<TKey, TValue>?> where TKey : notnull
{
    public ValueInput<Dictionary<TKey, TValue>?> Dictionary = new();
    public ValueInput<TKey> Key = new();

    protected override Dictionary<TKey, TValue>? ComputeValue(IPulseContext c)
    {
        var dictionary = Dictionary.Read(c);
        if (dictionary is null) return null;

        var key = Key.Read(c);

        dictionary = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
        dictionary.Remove(key);
        return dictionary;
    }
}