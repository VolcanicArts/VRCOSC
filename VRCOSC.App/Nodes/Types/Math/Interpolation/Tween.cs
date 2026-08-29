// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Tween", "Math/Interpolation")]
public sealed class TweenNode<T> : Node where T : INumber<T>
{
    public FlowInput Input = new();
    public FlowOutput OnUpdate = new("On Update", scope: true);
    public FlowOutput OnFinished = new("On Finished");

    public ValueInput<T> From = new();
    public ValueInput<T> To = new();
    public ValueInput<int> Duration = new("Duration (ms)");
    public ValueInput<EasingMode> Easing = new();
    public ValueOutput<T> Value = new();

    protected override async Task Process(IPulseContext c)
    {
        var startTime = DateTime.Now;
        var duration = Duration.Read(c);
        var endTime = startTime + TimeSpan.FromMilliseconds(duration);
        var easing = Easing.Read(c);
        var fromDouble = From.Read(c);
        var toDouble = To.Read(c);

        var t = 0d;

        do
        {
            t = double.Clamp((DateTime.Now - startTime) / (endTime - startTime), 0d, 1d);
            var value = Utils.Interpolation.Map(t, 0d, 1d, fromDouble, toDouble, easing);
            Value.Write(value, c);

            await OnUpdate.Execute(c);
            await Task.Delay(TimeSpan.FromSeconds(1d / 100d));
        } while (!c.IsCancelled && System.Math.Abs(t - 1d) > double.Epsilon);

        if (c.IsCancelled) return;

        Value.Write(default!, c);
        await OnFinished.Execute(c);
    }
}