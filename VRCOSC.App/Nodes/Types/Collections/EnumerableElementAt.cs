// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Element At", "Collections")]
public sealed class EnumerableElementAtNode<T> : Node
{
    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueOutput<T> Element = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        var index = Index.Read(c);
        if (index < 0 || index >= enumerable.Count()) return Task.CompletedTask;

        Element.Write(enumerable.ElementAt(index), c);
        return Task.CompletedTask;
    }
}