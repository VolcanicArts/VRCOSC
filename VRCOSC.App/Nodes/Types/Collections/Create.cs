// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Create List", "Collections")]
public sealed class CreateListNode<T> : Node
{
    public ValueInputList<T> Inputs = new();
    public ValueOutput<List<T>> Output = new();
    public ValueOutput<int> InputCount = new("Input Count");

    protected override Task Process(PulseContext c)
    {
        var list = Inputs.Read(c).ToList();
        Output.Write(list, c);
        InputCount.Write(list.Count, c);
        return Task.CompletedTask;
    }
}

[Node("Create Dictionary", "Collections")]
public sealed class CreateDictionaryNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInputList<KeyValuePair<TKey, TValue>> Inputs = new();
    public ValueOutput<Dictionary<TKey, TValue>> Output = new();
    public ValueOutput<int> InputCount = new("Input Count");

    protected override Task Process(PulseContext c)
    {
        var dictionary = new Dictionary<TKey, TValue>();
        dictionary.AddRange(Inputs.Read(c).RemoveIf(pair => pair.Key is null));

        Output.Write(dictionary, c);
        InputCount.Write(dictionary.Count, c);
        return Task.CompletedTask;
    }
}

[Node("Create KeyValuePair", "Collections")]
[NodeCollapsed]
public sealed class CreateKeyValuePairNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInput<TKey> Key = new();
    public ValueInput<TValue> Value = new();
    public ValueOutput<KeyValuePair<TKey, TValue>> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(new KeyValuePair<TKey, TValue>(Key.Read(c), Value.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Empty Dictionary", "Collections")]
[NodeCollapsed]
public sealed class EmptyDictionaryNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueOutput<Dictionary<TKey, TValue>> Dictionary = new();

    protected override Task Process(PulseContext c)
    {
        Dictionary.Write(new Dictionary<TKey, TValue>(), c);
        return Task.CompletedTask;
    }
}

[Node("Empty List", "Collections")]
[NodeCollapsed]
public sealed class EmptyListNode<T> : Node
{
    public ValueOutput<List<T>> List = new();

    protected override Task Process(PulseContext c)
    {
        List.Write(new List<T>(), c);
        return Task.CompletedTask;
    }
}