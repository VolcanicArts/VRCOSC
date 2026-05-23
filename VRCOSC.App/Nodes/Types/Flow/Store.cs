// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Write Context Store", "Utility/Stores")]
public sealed class ContextStoreWriteNode<T> : ActionNode
{
    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override void DoAction(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return;

        c.WriteKeyedStore(name, Value.Read(c));
    }
}

[Node("Context Store Source", "Utility/Stores")]
public sealed class ContextStoreSourceNode<T> : ValueComputeNode<T>
{
    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    protected override T ComputeValue(IPulseContext c)
    {
        var name = Name.Read(c);
        return string.IsNullOrWhiteSpace(name) ? default! : c.ReadKeyedStore<T>(name);
    }
}