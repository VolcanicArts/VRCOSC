// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Write Context Store", "Flow/Stores")]
public sealed class ContextStoreWriteNode<T> : Node, IFlowInput, IHasTextProperty
{
    [NodeProperty("key")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new();

    public ValueInput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Next.Execute(c);

        c.WriteKeyedStore(Text, Value.Read(c));
        return Next.Execute(c);
    }
}

[Node("Context Store Source", "Flow/Stores")]
public sealed class ContextStoreSourceNode<T> : ValueComputeNode<T>, IHasTextProperty
{
    [NodeProperty("key")]
    public string Text { get; set; } = string.Empty;

    protected override T ComputeValue(PulseContext c) => string.IsNullOrWhiteSpace(Text) ? default! : c.ReadKeyedStore<T>(Text);
}