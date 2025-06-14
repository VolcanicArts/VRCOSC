// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Element At", "Collections")]
public sealed class EnumerableElementAtNode<T> : Node
{
    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueOutput<T> Element = new();

    protected override void Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return;

        var index = Index.Read(c);
        if (index < 0 || index >= enumerable.Count()) return;

        Element.Write(enumerable.ElementAt(index), c);
    }
}