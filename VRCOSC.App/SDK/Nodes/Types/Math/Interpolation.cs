// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types.Math;

[Node("Damp Continuously", "Math/Interpolation")]
public class DampContinuouslyNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Current")] float current,
        [NodeValue("Target")] float target,
        [NodeValue("Half Time Milliseconds")] float halfTimeMilli,
        [NodeValue("Elapsed Time Milliseconds")]
        float elapsedTimeMilli,
        [NodeValue("Result")] ref float result
    )
    {
        result = Interpolation.DampContinuously(current, target, halfTimeMilli, elapsedTimeMilli);
    }
}

[Node("Tween", "Math/Interpolation")]
public sealed class TweenNode : Node, IFlowInput, IFlowOutput, IAsyncNode
{
    public NodeFlowRef[] FlowOutputs => [new("On Complete")];

    [NodeProcess]
    private int process
    (
        [NodeValue("From")] float from,
        [NodeValue("To")] float to,
        [NodeValue("Time Milliseconds")] float timeMilliseconds,
        [NodeValue("Target")] ITarget<float> target,
        CancellationToken cancellationToken
    )
    {
        var startTime = DateTime.Now;
        var endTime = startTime + TimeSpan.FromMilliseconds(timeMilliseconds);

        float percentage;

        do
        {
            var currentTime = DateTime.Now;
            percentage = (float)((currentTime - startTime) / (endTime - startTime));
            if (percentage > 1) percentage = 1;
            target.SetValue(Interpolation.Lerp(from, to, percentage));
        } while (percentage < 1 && !cancellationToken.IsCancellationRequested);

        return 0;
    }
}