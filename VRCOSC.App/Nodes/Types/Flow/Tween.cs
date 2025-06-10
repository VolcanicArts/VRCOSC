// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Tween", "Flow")]
public sealed class TweenNode<T> : Node, IFlowInput where T : INumber<T>
{
    public FlowCall OnUpdate = new("On Update");
    public FlowContinuation OnFinished = new("On Finished");

    public ValueInput<T> From = new();
    public ValueInput<T> To = new();
    public ValueInput<float> TimeMilliseconds = new("Time Milliseconds");
    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var startTime = DateTime.Now;
        var milliseconds = TimeMilliseconds.Read(c);
        var endTime = startTime + TimeSpan.FromMilliseconds(milliseconds);

        do
        {
            var t = double.Clamp((DateTime.Now - startTime) / (endTime - startTime), 0d, 1d);

            var fromDouble = double.CreateChecked(From.Read(c));
            var toDouble = double.CreateChecked(To.Read(c));
            var valueDouble = fromDouble + (toDouble - fromDouble) * t;

            var value = T.CreateChecked(valueDouble);
            Value.Write(value, c);

            OnUpdate.Execute(c);

            Task.Delay(TimeSpan.FromSeconds(1d / 60d)).Wait(c.Token);
            if (c.IsCancelled || System.Math.Abs(t - 1d) < double.Epsilon) break;
        } while (true);

        if (c.IsCancelled) return;

        OnFinished.Execute(c);
    }
}