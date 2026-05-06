// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Write Context Store", "Flow/Stores")]
public sealed class ContextStoreWriteNode<T> : ActionNode, IHasTextProperty
{
    [NodeProperty("key")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T> Value = new();

    protected override void DoAction(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        c.WriteKeyedStore(Text, Value.Read(c));
    }
}

[Node("Context Store Source", "Flow/Stores")]
public sealed class ContextStoreSourceNode<T> : ValueComputeNode<T>, IHasTextProperty
{
    [NodeProperty("key")]
    public string Text { get; set; } = string.Empty;

    protected override T ComputeValue(PulseContext c) => string.IsNullOrWhiteSpace(Text) ? default! : c.ReadKeyedStore<T>(Text);
}