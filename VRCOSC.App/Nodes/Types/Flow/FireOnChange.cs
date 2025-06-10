// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On Change", "Flow")]
public class FireOnChangeNode<T> : Node
{
    public FlowCall OnChange = new("On Change");

    public GlobalStore<T> Value = new();

    [NodeReactive]
    public ValueInput<T> Input = new();

    protected override void Process(PulseContext c)
    {
        Value.Write(Input.Read(c), c);
        OnChange.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        return !EqualityComparer<T>.Default.Equals(Input.Read(c), Value.Read(c));
    }
}

[Node("Fire On Change Multi", "Flow")]
public class FireOnChangeMultiNode<T> : Node
{
    public FlowCall OnChange = new("On Change");

    public GlobalStore<List<T>> Values = new();

    [NodeReactive]
    public ValueInputList<T> Inputs = new();

    protected override void Process(PulseContext c)
    {
        Values.Write(Inputs.Read(c), c);
        OnChange.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        var inputs = Inputs.Read(c);
        var values = Values.Read(c);
        if (values is null || inputs.Count != values.Count) return true;

        return inputs.Where((input, i) => !EqualityComparer<T>.Default.Equals(input, values[i])).Any();
    }
}