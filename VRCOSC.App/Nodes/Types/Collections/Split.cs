// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Split Dictionary", "Collections")]
public sealed class SplitDictionaryNode<TKey, TValue> : ValueConsumeNode<Dictionary<TKey, TValue>?> where TKey : notnull
{
    public ValueOutput<List<TKey>> Keys = new();
    public ValueOutput<List<TValue>> Values = new();

    protected override void ConsumeValue(Dictionary<TKey, TValue>? dictionary, PulseContext c)
    {
        if (dictionary is null) return;

        Keys.Write(dictionary.Keys.ToList(), c);
        Values.Write(dictionary.Values.ToList(), c);
    }
}

[Node("Split KeyValuePair", "Collections")]
public sealed class SplitKeyValuePairNode<TKey, TValue> : ValueConsumeNode<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    public ValueOutput<TKey> Key = new();
    public ValueOutput<TValue> Value = new();

    protected override void ConsumeValue(KeyValuePair<TKey, TValue> pair, PulseContext c)
    {
        Key.Write(pair.Key, c);
        Value.Write(pair.Value, c);
    }
}