// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("For", "Collections")]
public sealed class ForNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On Finished"),
        new("On Loop")
    ];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Count")] int count,
        [NodeValue("Index")] Ref<int> outIndex
    )
    {
        for (var i = 0; i < count; i++)
        {
            outIndex.Value = i;
            await TriggerFlow(token, 1, true);
        }

        await TriggerFlow(token, 0);
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
    private async Task process
    (
        CancellationToken token,
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Element")] Ref<T> outElement
    )
    {
        if (enumerable is null) return;

        foreach (var element in enumerable)
        {
            outElement.Value = element;
            if (token.IsCancellationRequested) break;

            await TriggerFlow(token, 1, true);
        }

        if (token.IsCancellationRequested) return;

        await TriggerFlow(token, 0);
    }
}