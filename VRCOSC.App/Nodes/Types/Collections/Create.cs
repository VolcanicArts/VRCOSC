// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Create List", "Collections/Enumerable")]
public sealed class CreateListNode<T> : Node
{
    public ValueInputList<T> Inputs = new();
    public ValueOutput<List<T>> Output = new();
    public ValueOutput<int> InputCount = new();

    protected override Task Process(IPulseContext c)
    {
        var list = Inputs.Read(c).ToList();
        Output.Write(list, c);
        InputCount.Write(Inputs.Count, c);
        return Task.CompletedTask;
    }
}

[Node("Create Dictionary", "Collections/Dictionary")]
public sealed class CreateDictionaryNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInputList<KeyValuePair<TKey, TValue>> Inputs = new();
    public ValueOutput<Dictionary<TKey, TValue>> Output = new();
    public ValueOutput<int> InputCount = new();

    protected override Task Process(IPulseContext c)
    {
        var dictionary = new Dictionary<TKey, TValue>();
        var inputs = Inputs.Read(c).ToList();
        inputs.RemoveAll(kvp => kvp.Key is null);
        dictionary.AddRange(inputs);

        Output.Write(dictionary, c);
        InputCount.Write(Inputs.Count, c);
        return Task.CompletedTask;
    }
}

[Node("Pack KeyValuePair", "Collections/Dictionary")]
[NodeCollapsed]
public sealed class CreateKeyValuePairNode<TKey, TValue>() : SimpleResultComputeNode<TKey, TValue, KeyValuePair<TKey, TValue>>((key, value) => new KeyValuePair<TKey, TValue>(key, value), "Key", "Value") where TKey : notnull
{
    public override string DisplayName => "Pack KVP";
}

[Node("Empty Dictionary", "Collections/Dictionary")]
[NodeCollapsed]
public sealed class EmptyDictionaryNode<TKey, TValue>() : SimpleValueComputeNode<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>(), "Dictionary") where TKey : notnull;

[Node("Empty List", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EmptyListNode<T>() : SimpleValueComputeNode<List<T>>(() => new List<T>(), "List");