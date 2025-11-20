// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Split Dictionary", "Collections")]
public sealed class SplitDictionaryNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInput<Dictionary<TKey, TValue>> Input = new();
    public ValueOutput<List<TKey>> Keys = new();
    public ValueOutput<List<TValue>> Values = new();

    protected override Task Process(PulseContext c)
    {
        var dictionary = Input.Read(c);

        Keys.Write(dictionary.Keys.ToList(), c);
        Values.Write(dictionary.Values.ToList(), c);

        return Task.CompletedTask;
    }
}

[Node("Split KeyValuePair", "Collections")]
public sealed class SplitKeyValuePairNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInput<KeyValuePair<TKey, TValue>> Input = new();
    public ValueOutput<TKey> Key = new();
    public ValueOutput<TValue> Value = new();

    protected override Task Process(PulseContext c)
    {
        var pair = Input.Read(c);

        Key.Write(pair.Key, c);
        Value.Write(pair.Value, c);

        return Task.CompletedTask;
    }
}