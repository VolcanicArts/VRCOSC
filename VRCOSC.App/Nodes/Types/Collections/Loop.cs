// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("For Each", "Collections")]
public sealed class ForEachNode<T> : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On Finished"),
        new("On Loop")
    ];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Enumerable")] IEnumerable<T>? enumerable,
        [NodeValue("Element")] Ref<T> outElement
    )
    {
        if (enumerable is null) return;

        foreach (var element in enumerable)
        {
            outElement.Value = element;
            if (context.Token.IsCancellationRequested) break;

            await TriggerFlow(context, 1, true);
        }

        if (context.Token.IsCancellationRequested) return;

        await TriggerFlow(context, 0);
    }
}