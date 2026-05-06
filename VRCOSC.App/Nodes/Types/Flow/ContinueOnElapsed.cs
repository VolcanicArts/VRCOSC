// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Continue On Elapsed", "Flow")]
public sealed class ContinueOnElapsedNode : Node, IFlowInput
{
    public GlobalStore<DateTime> LastUpdateStore = new();

    public FlowContinuation OnElapsed = new();

    public ValueInput<int> ElapsedMilliseconds = new();

    protected override async Task Process(PulseContext c)
    {
        if (ElapsedMilliseconds.Read(c) <= 0) return;

        var dateTimeNow = DateTime.Now;

        if ((dateTimeNow - LastUpdateStore.Read(c)).TotalMilliseconds >= ElapsedMilliseconds.Read(c))
        {
            LastUpdateStore.Write(dateTimeNow, c);
            await OnElapsed.Execute(c);
        }
    }
}