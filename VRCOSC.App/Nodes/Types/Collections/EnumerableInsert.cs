// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Insert", "Collections")]
public class EnumerableInsertNode<T> : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueInput<T> Element = new();
    public ValueOutput<IEnumerable<T>> Result = new();

    protected override void Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        var index = Index.Read(c);
        var element = Element.Read(c);

        if (enumerable is null) return;

        var list = enumerable.ToList();
        list.Insert(index, element);
        Result.Write(list, c);

        Next.Execute(c);
    }
}