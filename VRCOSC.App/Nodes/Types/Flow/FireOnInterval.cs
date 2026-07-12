// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On Interval", "Flow")]
public sealed class FireOnIntervalNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public GlobalStore<DateTime> LastUpdateStore = new();

    public FlowOutput Next = new();

    public ValueInput<int> Interval = new("Interval (ms)");

    protected override async Task Process(IPulseContext c)
    {
        var delay = Interval.Read(c);
        var dateTimeNow = DateTime.Now;
        var shouldContinue = (dateTimeNow - LastUpdateStore.Read(c)).TotalMilliseconds >= delay;

        if (shouldContinue)
        {
            LastUpdateStore.Write(dateTimeNow, c);
            await Next.Execute(c);
            return;
        }
    }
}