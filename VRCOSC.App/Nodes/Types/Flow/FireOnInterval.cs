// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On Interval", "Flow")]
public sealed class FireOnIntervalNode : Node
{
    public FlowCall OnInterval = new("On Interval");

    [NodeReactive]
    public ValueInput<int> DelayMilliseconds = new();

    protected override void Process(PulseContext c)
    {
        var delay = DelayMilliseconds.Read(c);
        if (delay <= 0) return;

        while (!c.IsCancelled)
        {
            OnInterval.Execute(c);
            if (c.IsCancelled) break;

            Task.Delay(delay, c.Token).Wait(c.Token);
            if (c.IsCancelled) break;
        }
    }
}