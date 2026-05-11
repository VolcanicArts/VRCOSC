// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

public abstract class FireOnBaseNode<T> : Node
{
    public FlowContinuation Next = new();

    public GlobalStore<T> PrevValue = new();

    protected override Task Process(PulseContext c) => Next.Execute(c);

    protected override bool ShouldProcess(PulseContext c)
    {
        var value = GetValue(c);
        var prevValue = PrevValue.Read(c);

        PrevValue.Write(value, c);
        return ShouldFire(value, prevValue);
    }

    protected abstract T GetValue(PulseContext c);

    protected abstract bool ShouldFire(T value, T prevValue);
}

[Node("Fire On Change", "Flow")]
public sealed class FireOnChangeNode<T> : FireOnBaseNode<T>
{
    public ValueInput<T> Value = new();

    protected override T GetValue(PulseContext c) => Value.Read(c);

    protected override bool ShouldFire(T value, T prevValue) => !EqualityComparer<T>.Default.Equals(value, prevValue);
}

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : FireOnBaseNode<bool>
{
    public ValueInput<bool> Condition = new();

    protected override bool GetValue(PulseContext c) => Condition.Read(c);

    protected override bool ShouldFire(bool value, bool prevValue) => value && !prevValue;
}

[Node("Fire On False", "Flow")]
public sealed class FireOnFalseNode : FireOnBaseNode<bool>
{
    public ValueInput<bool> Condition = new();

    protected override bool GetValue(PulseContext c) => Condition.Read(c);

    protected override bool ShouldFire(bool value, bool prevValue) => !value && prevValue;
}

[Node("Fire On Change (Multi)", "Flow")]
public sealed class FireOnChangeMultiNode<T> : Node
{
    public override string DisplayName => "Fire On Change";

    public FlowContinuation Next = new();

    public GlobalStore<List<T>> PrevValues = new();

    public ValueInputList<T> Values = new();

    protected override Task Process(PulseContext c)
    {
        PrevValues.Write(Values.Read(c), c);
        return Next.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        var inputs = Values.Read(c);
        var values = PrevValues.Read(c);
        if (values is null || inputs.Count != values.Count) return true;

        return inputs.Where((input, i) => !EqualityComparer<T>.Default.Equals(input, values[i])).Any();
    }
}

[Node("Fire On Change (Enumerable)", "Flow")]
public sealed class FireOnChangeEnumerableNode<T> : Node, IActiveUpdateNode
{
    public override string DisplayName => "Fire On Change";

    public int UpdateOffset => 0;

    public FlowContinuation Next = new();

    public GlobalStore<IEnumerable<T>> EnumerableStore = new();

    public ValueInput<IEnumerable<T>> Enumerable = new();

    protected override Task Process(PulseContext c)
    {
        EnumerableStore.Write(Enumerable.Read(c), c);
        return Next.Execute(c);
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