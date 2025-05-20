// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Interpolation;

[Node("Tween", "Flow")]
public sealed class TweenNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("On Complete"), new("On Update")];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("From")] float from,
        [NodeValue("To")] float to,
        [NodeValue("Time Milliseconds")] float timeMilliseconds,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        var startTime = DateTime.Now;
        var endTime = startTime + TimeSpan.FromMilliseconds(timeMilliseconds);

        float percentage;

        do
        {
            if (context.Token.IsCancellationRequested) break;

            var currentTime = DateTime.Now;
            percentage = (float)((currentTime - startTime) / (endTime - startTime));

            if (percentage > 1) percentage = 1;

            var nextValue = Utils.Interpolation.Lerp(from, to, percentage);
            outValue.Value = nextValue;

            await TriggerFlow(context, 1, true);
        } while (percentage < 1 && !context.Token.IsCancellationRequested);

        if (context.Token.IsCancellationRequested) return;

        await TriggerFlow(context, 0);
    }
}