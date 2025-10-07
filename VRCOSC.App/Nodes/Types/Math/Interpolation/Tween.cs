// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Tween", "Math/Interpolation")]
public sealed class TweenNode<T> : Node, IFlowInput where T : INumber<T>
{
    public FlowCall OnUpdate = new("On Update");
    public FlowContinuation OnFinished = new("On Finished");

    public ValueInput<T> From = new();
    public ValueInput<T> To = new();
    public ValueInput<float> TimeMilliseconds = new("Time Milliseconds");
    public ValueOutput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var startTime = DateTime.Now;
        var milliseconds = TimeMilliseconds.Read(c);
        var endTime = startTime + TimeSpan.FromMilliseconds(milliseconds);

        var t = 0d;

        do
        {
            t = double.Clamp((DateTime.Now - startTime) / (endTime - startTime), 0d, 1d);

            var fromDouble = double.CreateSaturating(From.Read(c));
            var toDouble = double.CreateSaturating(To.Read(c));
            var valueDouble = fromDouble + (toDouble - fromDouble) * t;

            var value = T.CreateSaturating(valueDouble);
            Value.Write(value, c);

            await OnUpdate.Execute(c);
            await Task.Delay(TimeSpan.FromSeconds(1d / 60d));
        } while (!c.IsCancelled && System.Math.Abs(t - 1d) > double.Epsilon);

        await OnFinished.Execute(c);
    }
}