// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Tween", "Math/Interpolation")]
public sealed class TweenNode<T> : Node, IActiveUpdateNode where T : INumber<T>
{
    public int UpdateOffset => -3;

    public GlobalStore<DateTime> StartTimeStore = new();
    public GlobalStore<DateTime> EndTimeStore = new();
    public GlobalStore<double> DeltaStore = new();
    public GlobalStore<bool> RunningStore = new();

    public FlowInput FlowInput = new();
    public FlowOutput FlowOnUpdate = new("On Update", scope: true);
    public FlowOutput FlowOnFinished = new("On Finished");

    public ValueInput<T> From = new();
    public ValueInput<T> To = new();
    public ValueInput<int> Duration = new("Duration (ms)");
    public ValueInput<EasingMode> Easing = new(defaultValue: EasingMode.Linear);
    public ValueOutput<T> Value = new();

    protected override async Task Process(IPulseContext c)
    {
        reset(c);
        var running = true;
        RunningStore.Write(running, c);
        var deltaTime = TimeSpan.FromMilliseconds(c.DeltaTime);

        while (running && !c.IsCancelled)
        {
            await c.Run(Task.Delay(deltaTime));
            running = RunningStore.Read(c);
        }

        await FlowOnFinished.Execute(c);
    }

    public async Task<bool> OnUpdate(IPulseContext c)
    {
        if (!RunningStore.Read(c))
            return false;

        var startTime = StartTimeStore.Read(c);
        var endTime = EndTimeStore.Read(c);
        var duration = Duration.Read(c);

        if (startTime == default)
        {
            startTime = DateTime.Now;
            endTime = startTime + TimeSpan.FromMilliseconds(duration);
            StartTimeStore.Write(startTime, c);
            EndTimeStore.Write(endTime, c);
        }

        var from = From.Read(c);
        var to = To.Read(c);
        var easing = Easing.Read(c);
        var delta = DeltaStore.Read(c) + c.DeltaTime;
        DeltaStore.Write(delta, c);
        var endDelta = (endTime - startTime).TotalMilliseconds;
        var t = Utils.Interpolation.Map(delta, 0d, endDelta, 0d, 1d);

        if (t - 1d > 1e-4d)
        {
            reset(c);
            return false;
        }

        var value = Utils.Interpolation.Ease(from, to, t, easing);
        Value.Write(value, c);

        await FlowOnUpdate.Execute(c);
        return false;
    }

    private void reset(IPulseContext c)
    {
        RunningStore.Write(false, c);
        StartTimeStore.Write(default, c);
        EndTimeStore.Write(default, c);
        DeltaStore.Write(0, c);
        Value.Write(default!, c);
    }
}