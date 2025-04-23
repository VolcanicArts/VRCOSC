// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("While", "Loop")]
public class WhileNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs { get; set; } =
    [
        new("On Finished"),
        new("On Loop", true)
    ];

    [NodeProcess]
    private int process
    (
        [NodeValue("Condition")] bool condition
    )
    {
        return condition ? 1 : 0;
    }
}

[Node("For", "Loop")]
public class ForNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs { get; set; } =
    [
        new("On Finished"),
        new("On Loop", true)
    ];

    private int currentIndex;

    [NodeProcess]
    private int process
    (
        [NodeValue("Count")] int count,
        [NodeValue("Index")] ref int outIndex
    )
    {
        if (currentIndex < count)
        {
            outIndex = currentIndex++;
            return 1;
        }

        currentIndex = 0;
        return 0;
    }
}

[Node("For Each", "Loop")]
public sealed class ForEachNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs { get; set; } =
    [
        new("On Finished"),
        new("On Loop", true)
    ];

    private int currentIndex;

    [NodeProcess]
    private int process
    (
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Element")] ref T outElement
    )
    {
        if (enumerable is null) return -1;

        if (currentIndex < enumerable.Count())
        {
            outElement = enumerable.ElementAt(currentIndex++);
            return 1;
        }

        currentIndex = 0;
        return 0;
    }
}