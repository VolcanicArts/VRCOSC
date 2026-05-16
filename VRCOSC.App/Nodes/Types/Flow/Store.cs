// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Write Context Store", "Flow/Stores")]
public sealed class ContextStoreWriteNode<T> : ActionNode
{
    public ValueInput<string> Name = new(modes: ValueInputMode.Inline);
    public ValueInput<T> Value = new();

    protected override void DoAction(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return;

        c.WriteKeyedStore(name, Value.Read(c));
    }
}

[Node("Context Store Source", "Flow/Stores")]
public sealed class ContextStoreSourceNode<T> : ValueComputeNode<T>
{
    public ValueInput<string> Name = new(modes: ValueInputMode.Inline);

    protected override T ComputeValue(IPulseContext c)
    {
        var name = Name.Read(c);
        return string.IsNullOrWhiteSpace(name) ? default! : c.ReadKeyedStore<T>(name);
    }
}