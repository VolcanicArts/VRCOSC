// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Enumerable Count", "Collections")]
public class EnumerableCountNode<T> : Node
{
    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueOutput<int> Element = new();

    protected override void Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return;

        Element.Write(enumerable.Count(), c);
    }
}