// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Dictionary Count", "Collections")]
public sealed class DictionaryCountNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInput<Dictionary<TKey, TValue>> Dictionary = new();
    public ValueOutput<int> Count = new();

    protected override Task Process(PulseContext c)
    {
        var dictionary = Dictionary.Read(c);
        if (dictionary is null) return Task.CompletedTask;

        Count.Write(dictionary.Count, c);
        return Task.CompletedTask;
    }
}

[Node("Dictionary Key To Value", "Collections")]
public sealed class DictionaryKeyToValueNode<TKey, TValue> : Node where TKey : notnull
{
    public ValueInput<Dictionary<TKey, TValue>> Dictionary = new();
    public ValueInput<TKey> Key = new();
    public ValueOutput<TValue> Value = new();

    protected override Task Process(PulseContext c)
    {
        var dictionary = Dictionary.Read(c);
        if (dictionary is null) return Task.CompletedTask;

        var key = Key.Read(c);
        if (key is null) return Task.CompletedTask;

        Value.Write(dictionary.TryGetValue(key, out var value) ? value : default!, c);
        return Task.CompletedTask;
    }
}

[Node("Dictionary Add Element", "Collections")]
public sealed class DictionaryElementAddNode<TKey, TValue> : Node, IFlowInput where TKey : notnull
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Dictionary<TKey, TValue>> Dictionary = new();
    public ValueInput<KeyValuePair<TKey, TValue>> Element = new();
    public ValueOutput<Dictionary<TKey, TValue>> Result = new();

    protected override async Task Process(PulseContext c)
    {
        var dictionary = Dictionary.Read(c);

        if (dictionary is null)
        {
            await Next.Execute(c);
            return;
        }

        var element = Element.Read(c);

        dictionary = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
        dictionary.Add(element.Key, element.Value);
        Result.Write(dictionary, c);

        await Next.Execute(c);
    }
}

[Node("Dictionary Remove Key", "Collections")]
public sealed class DictionaryKeyRemoveNode<TKey, TValue> : Node, IFlowInput where TKey : notnull
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Dictionary<TKey, TValue>> Dictionary = new();
    public ValueInput<TKey> Key = new();
    public ValueOutput<Dictionary<TKey, TValue>> Result = new();

    protected override async Task Process(PulseContext c)
    {
        var dictionary = Dictionary.Read(c);

        if (dictionary is null)
        {
            await Next.Execute(c);
            return;
        }

        var key = Key.Read(c);

        dictionary = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
        dictionary.Remove(key);
        Result.Write(dictionary, c);

        await Next.Execute(c);
    }
}