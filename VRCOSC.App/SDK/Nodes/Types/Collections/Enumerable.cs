// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types.Collections;

[Node("Element At", "Collections")]
public sealed class EnumerableElementAtNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Index")] int index,
        [NodeValue("Element")] ref T outElement
    )
    {
        if (enumerable is null) return;

        outElement = enumerable.ElementAt(index);
    }
}

[Node("Count", "Collections")]
public class EnumerableCountNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Count")] ref int outCount
    )
    {
        if (enumerable is null) return;

        outCount = enumerable.Count();
    }
}

[Node("Insert", "Collections")]
public class EnumerableInsertNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Insertion")];

    [NodeProcess]
    private int process
    (
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Index")] int index,
        [NodeValue("Value")] T value,
        [NodeValue("Enumerable")] ref IEnumerable<T> outEnumerable)
    {
        if (enumerable is null) return -1;

        var list = enumerable.ToList();
        list.Insert(index, value);
        outEnumerable = list;

        return 0;
    }
}