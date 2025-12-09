// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On Change", "Flow")]
public sealed class FireOnChangeNode<T> : Node
{
    public FlowContinuation Next = new("Next");

    public GlobalStore<T> PrevValue = new();

    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        PrevValue.Write(Value.Read(c), c);
        await Next.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        return !EqualityComparer<T>.Default.Equals(Value.Read(c), PrevValue.Read(c));
    }
}

[Node("Fire On Change Multi", "Flow")]
public sealed class FireOnChangeMultiNode<T> : Node
{
    public FlowContinuation Next = new("Next");

    public GlobalStore<List<T>> PrevValues = new();

    public ValueInputList<T> Values = new();

    protected override async Task Process(PulseContext c)
    {
        PrevValues.Write(Values.Read(c), c);
        await Next.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        var inputs = Values.Read(c);
        var values = PrevValues.Read(c);
        if (values is null || inputs.Count != values.Count) return true;

        return inputs.Where((input, i) => !EqualityComparer<T>.Default.Equals(input, values[i])).Any();
    }
}

[Node("Fire On Change Enumerable", "Flow")]
public sealed class FireOnChangeEnumerableNode<T> : Node, IActiveUpdateNode
{
    public int UpdateOffset => 0;

    public FlowContinuation Next = new("Next");

    public GlobalStore<IEnumerable<T>> EnumerableStore = new();

    public ValueInput<IEnumerable<T>> Enumerable = new();

    protected override async Task Process(PulseContext c)
    {
        EnumerableStore.Write(Enumerable.Read(c), c);
        await Next.Execute(c);
    }

    public Task<bool> OnUpdate(PulseContext c)
    {
        var prevValues = EnumerableStore.Read(c);
        var values = Enumerable.Read(c);

        if (prevValues is null && values is null)
            return Task.FromResult(false);

        if (prevValues is null && values is not null
            || prevValues is not null && values is null)
            return Task.FromResult(true);

        return Task.FromResult(!prevValues!.SequenceEqual(values!));
    }
}