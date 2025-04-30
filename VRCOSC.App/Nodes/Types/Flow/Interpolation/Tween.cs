// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
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
        CancellationToken token,
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
            if (token.IsCancellationRequested) break;

            var currentTime = DateTime.Now;
            percentage = (float)((currentTime - startTime) / (endTime - startTime));

            if (percentage > 1) percentage = 1;

            var nextValue = App.Utils.Interpolation.Lerp(from, to, percentage);
            outValue.Value = nextValue;

            await TriggerFlow(token, 1);
        } while (percentage < 1 && !token.IsCancellationRequested);

        if (token.IsCancellationRequested) return;

        await TriggerFlow(token, 0);
    }
}