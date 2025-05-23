// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Interpolation;

[Node("Tween", "Flow")]
public sealed class TweenNode : Node, IFlowInput
{
    public FlowCall OnUpdate = new("On Update");
    public FlowContinuation OnFinished = new("On Finished");

    public ValueInput<float> From = new();
    public ValueInput<float> To = new();
    public ValueInput<float> TimeMilliseconds = new();
    public ValueOutput<float> Value = new();

    protected override void Process(PulseContext c)
    {
        var startTime = DateTime.Now;
        var milliseconds = TimeMilliseconds.Read(c);
        var endTime = startTime + TimeSpan.FromMilliseconds(milliseconds);

        do
        {
            var currentTime = DateTime.Now;
            var percentage = (float)((currentTime - startTime) / (endTime - startTime));
            if (percentage > 1) percentage = 1;

            var nextValue = Utils.Interpolation.Lerp(From.Read(c), To.Read(c), percentage);
            Value.Write(nextValue, c);

            OnUpdate.Execute(c);

            Task.Delay(TimeSpan.FromSeconds(1f / 60f)).Wait(c.Token);
            if (c.IsCancelled || System.Math.Abs(percentage - 1) < float.Epsilon) break;
        } while (true);

        if (c.IsCancelled) return;

        OnFinished.Execute(c);
    }
}