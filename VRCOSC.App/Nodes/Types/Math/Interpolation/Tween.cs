// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Tween", "Math/Interpolation")]
public sealed class TweenNode<T> : Node where T : INumber<T>
{
    public FlowInput FlowInput = new();
    public FlowOutput OnUpdate = new("On Update", scope: true);
    public FlowOutput OnFinished = new("On Finished");

    public ValueInput<T> From = new();
    public ValueInput<T> To = new();
    public ValueInput<float> Duration = new("Duration (ms)");
    public ValueOutput<T> Value = new();

    protected override async Task Process(IPulseContext c)
    {
        var startTime = DateTime.Now;
        var milliseconds = Duration.Read(c);
        var endTime = startTime + TimeSpan.FromMilliseconds(milliseconds);
        var from = From.Read(c);
        var to = To.Read(c);

        var t = 0d;
        var fromDouble = double.CreateSaturating(from);
        var toDouble = double.CreateSaturating(to);
        var timeDiff = endTime - startTime;

        do
        {
            t = double.Clamp((DateTime.Now - startTime) / timeDiff, 0d, 1d);
            var value = T.CreateSaturating(fromDouble + (toDouble - fromDouble) * t);
            Value.Write(value, c);

            await OnUpdate.Execute(c);
            await Task.Delay(TimeSpan.FromSeconds(1d / 100d));
        } while (!c.IsCancelled && double.Abs(t - 1d) > double.Epsilon);

        if (c.IsCancelled) return;

        Value.Write(default!, c);
        await OnFinished.Execute(c);
    }
}