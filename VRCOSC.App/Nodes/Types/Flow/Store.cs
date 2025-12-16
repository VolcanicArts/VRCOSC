// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Write Context Store", "Flow/Stores")]
public sealed class ContextStoreWriteNode<T> : Node, IFlowInput, IHasTextProperty
{
    [NodeProperty("key")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new("Next");

    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            await Next.Execute(c);
            return;
        }

        var value = Value.Read(c);
        c.WriteKeyedStore(Text, value);

        await Next.Execute(c);
    }
}

[Node("Context Store Source", "Flow/Stores")]
public sealed class ContextStoreSourceNode<T> : Node, IHasTextProperty
{
    [NodeProperty("key")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text))
            return Task.CompletedTask;

        var value = c.ReadKeyedStore<T>(Text);
        Value.Write(value, c);
        return Task.CompletedTask;
    }
}