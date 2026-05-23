// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Rate Limit", "Flow")]
public sealed class ContinueOnElapsedNode : Node
{
    public GlobalStore<DateTime> LastUpdateStore = new();

    public FlowInput FlowInput = new();
    public FlowOutput Next = new();

    public ValueInput<int> Time = new("Time (ms)");

    protected override async Task Process(IPulseContext c)
    {
        var elapsed = Time.Read(c);
        if (elapsed <= 0) return;

        var dateTimeNow = DateTime.Now;

        if ((dateTimeNow - LastUpdateStore.Read(c)).TotalMilliseconds >= elapsed)
        {
            LastUpdateStore.Write(dateTimeNow, c);
            await Next.Execute(c);
        }
    }
}