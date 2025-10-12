// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On Interval", "Flow")]
public sealed class FireOnIntervalNode : Node, IUpdateNode
{
    private readonly GlobalStore<DateTime> lastUpdate = new();

    public FlowCall Next = new();

    public ValueInput<int> DelayMilliseconds = new("Delay Milliseconds");

    protected override async Task Process(PulseContext c)
    {
        var delay = DelayMilliseconds.Read(c);
        if (delay <= 0) return;

        var dateTimeNow = DateTime.Now;
        var shouldContinue = (dateTimeNow - lastUpdate.Read(c)).TotalMilliseconds >= delay;

        if (shouldContinue)
        {
            lastUpdate.Write(dateTimeNow, c);
            await Next.Execute(c);
        }
    }

    public bool OnUpdate(PulseContext c) => true;
}