// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.App.SDK.Nodes.Types.Collections;

[Node("For", "Collections")]
public sealed class ForNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On Finished"),
        new("On Loop")
    ];

    [NodeProcess]
    private void process
    (
        [NodeValue("Count")] int count,
        [NodeValue("Index")] ref int outIndex
    )
    {
        for (var i = 0; i < count; i++)
        {
            outIndex = i;
            TriggerFlow(1, true);
        }

        TriggerFlow(0);
    }
}

[Node("For Each", "Collections")]
public sealed class ForEachNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On Finished"),
        new("On Loop", true)
    ];

    [NodeProcess]
    private void process
    (
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Element")] ref T outElement
    )
    {
        if (enumerable is null) return;

        foreach (var element in enumerable)
        {
            outElement = element;
            TriggerFlow(1, true);
        }

        TriggerFlow(0);
    }
}