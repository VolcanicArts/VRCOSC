// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("For Each", "Collections")]
public sealed class ForEachNode<T> : Node, IFlowInput
{
    public FlowCall OnIteration = new("On Iteration");
    public FlowContinuation OnEnd = new("On End");

    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueOutput<T> Element = new();

    protected override async Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return;

        foreach (var element in enumerable)
        {
            Element.Write(element, c);
            if (c.IsCancelled) return;

            await OnIteration.Execute(c);
        }

        if (c.IsCancelled) return;

        await OnEnd.Execute(c);
    }
}