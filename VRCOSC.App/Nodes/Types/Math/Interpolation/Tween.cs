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
    public FlowInput FlowInput = new();
    public FlowOutput OnUpdate = new("On Update", scope: true);
    public FlowOutput OnFinished = new("On Finished");

    public ValueInput<T> From = new();
    public ValueInput<T> To = new();
    public ValueInput<int> Duration = new("Duration (ms)");
    public ValueInput<EasingMode> Easing = new(defaultValue: EasingMode.Linear);
    public ValueOutput<T> Value = new();

    protected override async Task Process(IPulseContext c)
    {
        var milliseconds = Duration.Read(c);
        var from = From.Read(c);
        var to = To.Read(c);
        var easing = Easing.Read(c);

        var t = 0d;
        var startTime = DateTime.Now;
        var endTime = startTime + TimeSpan.FromMilliseconds(milliseconds);
        var timeDiff = endTime - startTime;
        var updateDelay = TimeSpan.FromSeconds(1d / 100d);

        do
        {
            t = double.Clamp((DateTime.Now - startTime) / timeDiff, 0d, 1d);
            Value.Write(Utils.Interpolation.Ease(from, to, t, easing), c);
            await OnUpdate.Execute(c);
            await c.Run(Task.Delay(updateDelay));
        } while (!c.IsCancelled && double.Abs(t - 1d) > 1e-4);

        Value.Write(default!, c);
        await OnFinished.Execute(c);
    }
}